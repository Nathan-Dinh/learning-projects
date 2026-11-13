namespace csharp_PDF_Processing.Src.Domain.Model
{
    /// <summary>
    /// EF Core entity representing a PDF processing job.
    /// Tracks the status of each PDF file submitted for text extraction.
    /// </summary>
    public class ProcessingJob
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public JobStatus Status { get; set; } = JobStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
