namespace IS_AdvokatskeKancelarije.DTOs
{
    public class TrialDTO
    {
        public string? Court { get; set; }
        public string? Judge { get; set; }
        public string? Evidence { get; set; }
        public string? Judgement { get; set; }
        public float PreTrialPreparation { get; set; }
        public float Duration { get; set; }

        public Guid CaseId { get; set; }
    }
}
