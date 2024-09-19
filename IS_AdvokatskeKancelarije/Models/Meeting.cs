namespace IS_AdvokatskeKancelarije.Models
{
    public class Meeting
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string? Location { get; set; }
        public string? Subject { get; set; }
        public string? Participants { get; set; }
        public float Duration { get; set; }

        public Guid CaseId { get; set; }
        public Case? Case { get; set; }
    }
}
