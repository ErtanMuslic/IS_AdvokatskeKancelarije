using IS_AdvokatskeKancelarije.Data;
using Microsoft.EntityFrameworkCore;

namespace IS_AdvokatskeKancelarije.Services
{
    public record SearchHit(
        Guid DocumentId,
        string? DocumentName,
        Guid CaseId,
        int? PageNumber,
        int ChunkIndex,
        string Text,
        double Score);

   
    public class SearchService
    {
        private readonly AppDbContext _db;
        private readonly OllamaService _ollama;

        public SearchService(AppDbContext db, OllamaService ollama)
        {
            _db = db;
            _ollama = ollama;
        }

        public async Task<List<SearchHit>> SearchAsync(string query, Guid? caseId, int top, CancellationToken ct)
        {
            var queryVector = (await _ollama.EmbedAsync(new[] { query }, ct))[0];

            var chunks = _db.DocumentChunks.AsNoTracking().Where(c => c.Embedding != null);
            if (caseId.HasValue)
                chunks = chunks.Where(c => c.CaseId == caseId.Value);

            var rows = await chunks
                .Select(c => new
                {
                    c.DocumentId,
                    DocumentName = c.Document!.Name,
                    c.CaseId,
                    c.PageNumber,
                    c.ChunkIndex,
                    c.Text,
                    c.Embedding
                })
                .ToListAsync(ct);

            return rows
                .Select(r => new SearchHit(
                    r.DocumentId,
                    r.DocumentName,
                    r.CaseId,
                    r.PageNumber,
                    r.ChunkIndex,
                    r.Text,
                    VectorMath.CosineSimilarity(queryVector, r.Embedding!)))
                .OrderByDescending(h => h.Score)
                .Take(top)
                .ToList();
        }
    }
}
