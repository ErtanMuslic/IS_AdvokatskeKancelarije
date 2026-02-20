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

        public Guid CaseId { get; set; }

        [JsonIgnore]
        public Case? Case { get; set; }
    }
}
