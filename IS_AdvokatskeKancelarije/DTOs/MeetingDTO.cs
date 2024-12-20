namespace IS_AdvokatskeKancelarije.DTOs
{
    public class MeetingDTO
    {
        public DateTime Date { get; set; }
        public string? Location { get; set; }
        public string? Subject { get; set; }
        public string? Participants { get; set; }
        public float Duration { get; set; }

        public Guid CaseId { get; set; }
    }
}
