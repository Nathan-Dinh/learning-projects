using csharp_ControlPlane.Src.Domain.Rules;
using Microsoft.EntityFrameworkCore;

namespace csharp_ControlPlane.Src.Infrastructure.Data
{
    public class ControlPlaneDbContext(DbContextOptions<ControlPlaneDbContext> options)
        : DbContext(options)
    {
        public DbSet<AuthPolicy> AuthPolicies => Set<AuthPolicy>();
        public DbSet<RouteRule> Routes => Set<RouteRule>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuthPolicy>().HasData(
                new AuthPolicy("PDF_Handler",    "PDF_Processing", true)  { Id = 1 },
                new AuthPolicy("PDF_Handler",    "Storage",        false) { Id = 2 },
                new AuthPolicy("PDF_Processing", "Storage",        true)  { Id = 3 }
            );
        }
    }
}
