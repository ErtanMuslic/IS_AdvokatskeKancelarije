using IS_AdvokatskeKancelarije.Models;

namespace IS_AdvokatskeKancelarije.DTOs
{
    public class InvoiceDTO
    {
        public string? ServiceDescription { get; set; }
        public int? Hours { get; set; }
        public int? HourlyPrice { get; set; }
        public float? TotalPrice { get; set; }

        public string? UserId { get; set; }
        public Guid? CaseId { get; set; }
    }
}
