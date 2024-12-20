using System.Collections.Generic;
using IS_AdvokatskeKancelarije.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IS_AdvokatskeKancelarije.Data
{
    public class AppDbContext : IdentityDbContext<User> 
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Trial> Trials { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Invoice> Invoices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // This ensures Identity schema is included

            modelBuilder.Entity<Case>()
            .HasMany(c => c.Users)
            .WithMany(u => u.Cases)
            .UsingEntity(j => j.ToTable("UserCases"));


            // Client to Case relationship
            modelBuilder.Entity<Client>()
                .HasMany(c => c.Cases)
                .WithOne(c => c.Client)
                .HasForeignKey(c => c.ClientId);

            // Case to Meeting relationship
            modelBuilder.Entity<Case>()
                .HasMany(c => c.Meetings)
                .WithOne(m => m.Case)
                .HasForeignKey(m => m.CaseId);

            // Case to Trial relationship
            modelBuilder.Entity<Case>()
                .HasMany(c => c.Trials)
                .WithOne(t => t.Case)
                .HasForeignKey(t => t.CaseId);

            // Case to Document relationship
            modelBuilder.Entity<Case>()
                .HasMany(c => c.Documents)
                .WithOne(d => d.Case)
                .HasForeignKey(d => d.CaseId);

            // User to Invoice relationship
            modelBuilder.Entity<User>()
                .HasMany(u => u.Invoices)
                .WithOne(i => i.User)
                .HasForeignKey(i => i.UserId);

            //Case To Invoices relationship 
            modelBuilder.Entity<Case>()
                .HasMany(u => u.Invoices)
                .WithOne(i => i.Case)
                .HasForeignKey(i => i.CaseId);
        }
    }
}