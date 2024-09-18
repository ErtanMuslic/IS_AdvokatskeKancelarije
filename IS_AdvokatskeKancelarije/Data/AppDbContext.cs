using System.Collections.Generic;
using IS_AdvokatskeKancelarije.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IS_AdvokatskeKancelarije.Data
{
    
        public class AppDbContext(DbContextOptions options) : IdentityDbContext<User>(options)
        {
            public DbSet<Client> Clients { get; set; }
        }
}
