using System.Text.Json.Serialization;

namespace IS_AdvokatskeKancelarije.Models
{
    public class Document
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Author { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Type { get; set; }

        // Podaci o otpremljenom fajlu
        public string? OriginalFileName { get; set; }
        public string? ContentType { get; set; }
        public long FileSize { get; set; }

        // Stanje indeksiranja za AI pretragu: Pending, Indexing, Indexed, Failed
        public string? IndexStatus { get; set; }
        public string? IndexError { get; set; }

        [JsonIgnore]
        public ICollection<DocumentChunk>? Chunks { get; set; }

        // Ime fajla na disku (generisano na serveru); ne izlaze se klijentu
        [JsonIgnore]
        public string? StoredFileName { get; set; }

        public Guid CaseId { get; set; }

        [JsonIgnore]
        public Case? Case { get; set; }
    }
}
