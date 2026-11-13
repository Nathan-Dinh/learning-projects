namespace csharp_Storage.Src.Domain.Model
{
    /// <summary>
    /// EF Core entity representing a processed PDF document stored as JSON.
    /// Persisted in the Storage service's SQLite database.
    /// Service B (PDF_Processing) sends extracted JSON data here after parsing a PDF.
    /// </summary>
    public class PdfDocument
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;

        // Full JSON output from Service B (PdfPig extraction result).
        // Structure: { fileName, pageCount, pages[] }
        public string JsonContent { get; set; } = string.Empty;

        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
