using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace IS_AdvokatskeKancelarije.Models
{
    public class User : IdentityUser
    {
        public string? Name { get; set; }

        [JsonIgnore]
        public ICollection<Case>? Cases { get; set; }
        public ICollection<Invoice>? Invoices { get; set; }
    }
}
