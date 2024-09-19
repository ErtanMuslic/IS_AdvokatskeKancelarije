namespace IS_AdvokatskeKancelarije.Models
{
    public class Client
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone_Number { get; set; }

        public ICollection<Case> Cases {  get; set; }
    }
}
