using IS_AdvokatskeKancelarije.Enums;
using Type = IS_AdvokatskeKancelarije.Enums.Type;

namespace IS_AdvokatskeKancelarije.Models
{
    public class Case
    {
        public Guid Id { get; set; }
        public string? DeadLines { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedDate {  get; set; }
        public Type? CaseType {  get; set; } 
        public Status? Status { get; set; }


        public Guid ClientId { get; set; }
        public Client? Client { get; set; }
        public ICollection<Document>? Documents { get; set; }
        public ICollection<User>? Users { get; set; }
        public ICollection<Meeting>? Meetings { get; set; }
        public ICollection<Trial>? Trials { get; set; }
        public ICollection<Invoice>? Invoices { get; set; }

    }
}
