using System.Text.Json.Serialization;

namespace IS_AdvokatskeKancelarije.Models
{
    /// <summary>
    /// Deo teksta dokumenta (chunk) zajedno sa njegovim embeddingom.
    /// Embedding se čuva kao niz realnih brojeva (real[] u PostgreSQL-u).
    /// </summary>
    public class DocumentChunk
    {
        public Guid Id { get; set; }

        public Guid DocumentId { get; set; }

        [JsonIgnore]
        public Document? Document { get; set; }

        // Dupliran CaseId radi brzog filtriranja pretrage po predmetu
        public Guid CaseId { get; set; }

        public int ChunkIndex { get; set; }

        // Broj strane (samo za PDF); null za DOCX i TXT
        public int? PageNumber { get; set; }

        public string Text { get; set; } = string.Empty;

        public float[]? Embedding { get; set; }
    }
}
