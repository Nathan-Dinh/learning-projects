using csharp_Storage.Src.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace csharp_Storage.Src.Infrastructure.Data
{
    /// <summary>
    /// EF Core database context for the Storage service.
    /// Maps PdfDocument entities to the "Documents" table in storage.db (SQLite).
    /// </summary>
    public class StorageDbContext(DbContextOptions<StorageDbContext> options)
        : DbContext(options)
    {
        // Maps to the PdfDocument table in SQLite
        public DbSet<PdfDocument> Documents => Set<PdfDocument>();
    }
}
