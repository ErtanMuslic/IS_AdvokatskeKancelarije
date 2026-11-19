namespace IS_AdvokatskeKancelarije.DTOs
{
    public class AskDTO
    {
        public string Question { get; set; } = string.Empty;
        public Guid? CaseId { get; set; }
    }
}
