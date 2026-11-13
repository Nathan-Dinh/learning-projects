using csharp_PDFHandler.Src.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace csharp_PDFHandler.Src.Infrastructure.Data
{
    /// <summary>
    /// EF Core database context for the PDFHandler service.
    /// Maps UploadRecord entities to the "Uploads" table in handler.db (SQLite).
    /// </summary>
    public class HandlerDbContext(DbContextOptions<HandlerDbContext> options)
        : DbContext(options)
    {
        // Maps to the Uploads table in SQLite
        public DbSet<UploadRecord> Uploads => Set<UploadRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Store UploadStatus as string in DB for readability (e.g. "Received" not 0)
            modelBuilder.Entity<UploadRecord>()
                .Property(u => u.Status)
                .HasConversion<string>();
        }
    }
}
