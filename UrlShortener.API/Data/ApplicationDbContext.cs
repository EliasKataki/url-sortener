using Microsoft.EntityFrameworkCore;
using UrlShortener.API.Models;

namespace UrlShortener.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Url> Urls { get; set; }
        public DbSet<UrlAccess> UrlAccesses { get; set; }
        public DbSet<UrlLog> UrlLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Url>()
                .HasMany(u => u.UrlAccesses)
                .WithOne(ua => ua.Url)
                .HasForeignKey(ua => ua.UrlId);
        }
    }
} 