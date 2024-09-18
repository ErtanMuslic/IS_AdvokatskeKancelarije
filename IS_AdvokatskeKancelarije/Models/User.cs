using Microsoft.AspNetCore.Identity;

namespace IS_AdvokatskeKancelarije.Models
{
    public class User : IdentityUser
    {
        public string? Name { get; set; }
   
    }
}
