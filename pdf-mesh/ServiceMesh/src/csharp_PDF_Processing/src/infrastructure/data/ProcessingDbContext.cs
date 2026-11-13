using csharp_PDF_Processing.Src.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace csharp_PDF_Processing.Src.Infrastructure.Data
{
    /// <summary>
    /// EF Core database context for the PDF Processing service.
    /// Maps ProcessingJob entities to the "Jobs" table in processing.db (SQLite).
    /// </summary>
    public class ProcessingDbContext(DbContextOptions<ProcessingDbContext> options)
        : DbContext(options)
    {
        // Maps to the ProcessingJob table in SQLite
        public DbSet<ProcessingJob> Jobs => Set<ProcessingJob>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Store JobStatus as string in DB for readability (e.g. "Pending" not 0)
            modelBuilder.Entity<ProcessingJob>()
                .Property(j => j.Status)
                .HasConversion<string>();
        }
    }
}
