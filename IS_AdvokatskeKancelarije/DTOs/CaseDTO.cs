using IS_AdvokatskeKancelarije.Enums;
using Type = IS_AdvokatskeKancelarije.Enums.Type;

namespace IS_AdvokatskeKancelarije.DTOs
{
    public class CaseDTO
    {
        public string? DeadLines { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Type? CaseType { get; set; }
        public Status? Status { get; set; }
        public Guid ClientId { get; set; }
    }
}
