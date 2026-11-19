using IS_AdvokatskeKancelarije.Data;
using IS_AdvokatskeKancelarije.Models;
using Microsoft.EntityFrameworkCore;

namespace IS_AdvokatskeKancelarije.Services
{
    /// <summary>Izdvaja tekst, deli ga na delove, računa embeddings i upisuje ih u bazu.</summary>
    public class DocumentIndexer
    {
        private const int EmbeddingBatchSize = 16;

        private readonly AppDbContext _db;
        private readonly FileStorageService _storage;
        private readonly TextExtractionService _extractor;
        private readonly ChunkingService _chunker;
        private readonly OllamaService _ollama;
        private readonly ILogger<DocumentIndexer> _logger;

        public DocumentIndexer(
            AppDbContext db,
            FileStorageService storage,
            TextExtractionService extractor,
            ChunkingService chunker,
            OllamaService ollama,
            ILogger<DocumentIndexer> logger)
        {
            _db = db;
            _storage = storage;
            _extractor = extractor;
            _chunker = chunker;
            _ollama = ollama;
            _logger = logger;
        }

        public async Task IndexAsync(Guid documentId, CancellationToken ct)
        {
            var document = await _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId, ct);
            if (document == null || string.IsNullOrEmpty(document.StoredFileName))
                return;

            document.IndexStatus = "Indexing";
            document.IndexError = null;
            await _db.SaveChangesAsync(ct);

            try
            {
                // Ako se dokument ponovo indeksira, prvo se brišu stari delovi
                await _db.DocumentChunks.Where(c => c.DocumentId == documentId).ExecuteDeleteAsync(ct);

                using var stream = _storage.Open(document.StoredFileName)
                    ?? throw new FileNotFoundException("Stored file not found");

                var pages = _extractor.Extract(stream, Path.GetExtension(document.StoredFileName));
                var chunks = _chunker.Chunk(pages);

                if (chunks.Count == 0)
                    throw new InvalidOperationException(
                        "No text could be extracted from the file (it may be a scanned document)");

                var entities = new List<DocumentChunk>(chunks.Count);
                for (var i = 0; i < chunks.Count; i += EmbeddingBatchSize)
                {
                    var batch = chunks.Skip(i).Take(EmbeddingBatchSize).ToList();
                    var embeddings = await _ollama.EmbedAsync(batch.Select(c => c.Text).ToList(), ct);

                    for (var j = 0; j < batch.Count; j++)
                    {
                        entities.Add(new DocumentChunk
                        {
                            Id = Guid.NewGuid(),
                            DocumentId = document.Id,
                            CaseId = document.CaseId,
                            ChunkIndex = batch[j].Index,
                            PageNumber = batch[j].PageNumber,
                            Text = batch[j].Text,
                            Embedding = embeddings[j]
                        });
                    }
                }

                _db.DocumentChunks.AddRange(entities);
                document.IndexStatus = "Indexed";
                await _db.SaveChangesAsync(ct);

                _logger.LogInformation("Indexed document {DocumentId}: {Count} chunks", documentId, entities.Count);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to index document {DocumentId}", documentId);

                _db.ChangeTracker.Clear();
                var failed = await _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId, CancellationToken.None);
                if (failed != null)
                {
                    failed.IndexStatus = "Failed";
                    failed.IndexError = ex.Message.Length > 500 ? ex.Message[..500] : ex.Message;
                    await _db.SaveChangesAsync(CancellationToken.None);
                }
            }
        }
    }
}
