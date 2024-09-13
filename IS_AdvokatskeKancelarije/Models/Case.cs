namespace IS_AdvokatskeKancelarije.Models
{
    public class Case
    {
        public Guid Id { get; set; }
        public Client Client { get; set; }
        public User User { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate {  get; set; }
        public Type Type {  get; set; } 
        public List<Document> Documents { get; set; }
        public Status Status { get; set; }
        public string DeadLines { get; set; }
        public List<Meeting> Meetings { get; set; }
        public List<Trial> Trials { get; set; }

    }
}
