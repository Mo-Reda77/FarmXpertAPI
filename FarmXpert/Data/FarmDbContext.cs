using FarmXpert.Models;
using FarmXpert.Models.Client;
using FarmXpert.Models.MilkProduction;
using FarmXpert.Products;
using Microsoft.EntityFrameworkCore;
using System;

namespace FarmXpert.Data
{
    public class FarmDbContext : DbContext

    {
        public FarmDbContext(DbContextOptions<FarmDbContext> options) : base(options) { }


        public DbSet<User> Users { get; set; }
        public DbSet<Veterinarians> Veterinarians { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<Farm> Farms { get; set; }
        public DbSet<Cattle> Cattle { get; set; }
        public DbSet<RevokedToken> RevokedTokens { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<MilkProduction> MilkProductions { get; set; }
        public DbSet<ClientRequest> ClientRequests { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Worker>()
                .HasIndex(w => w.Email)
                .IsUnique();

            modelBuilder.Entity<Farm>()
               .HasIndex(o => o.Name)
               .IsUnique();

            modelBuilder.Entity<Veterinarians>()
               .HasIndex(p => p.Email)
               .IsUnique();


        }
    }

}
