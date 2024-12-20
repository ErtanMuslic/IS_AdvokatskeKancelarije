namespace IS_AdvokatskeKancelarije.Models
{
    public class Trial
    {
        public Guid Id { get; set; }
        public string? Court { get; set; }
        public string? Judge { get; set; }
        public string? Evidence { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Judgement { get; set; }
        public float? PreTrialPreparation {  get; set; }
        public float? Duration { get; set; }

        public Guid? CaseId { get; set; }
        public Case? Case { get; set; }
    }
}
