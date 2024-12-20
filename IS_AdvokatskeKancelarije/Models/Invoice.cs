namespace IS_AdvokatskeKancelarije.Models
{
    public class Invoice
    {
        public Guid Id { get; set; }
        public User? User { get; set; }
        public string? UserId { get; set; }
        public Case? Case { get; set; }
        public Guid CaseId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? ServiceDescription { get; set; }
        public int? Hours { get; set; }
        public int? HourlyPrice { get; set; }
        public float? TotalPrice { get; set; }

    }
}
