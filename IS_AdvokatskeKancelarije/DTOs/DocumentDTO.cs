namespace IS_AdvokatskeKancelarije.DTOs
{
    public class DocumentDTO
    {
        public string? Name { get; set; }
        public string? Author { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public Guid CaseId { get; set; }

    }
}
