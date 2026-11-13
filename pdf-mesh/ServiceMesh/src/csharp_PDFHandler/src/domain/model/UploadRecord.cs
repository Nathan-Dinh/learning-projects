namespace csharp_PDFHandler.Src.Domain.Model
{
    /// <summary>
    /// EF Core entity representing an uploaded PDF file.
    /// Tracks the forwarding status of each upload to Service B.
    /// </summary>
    public class UploadRecord
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public UploadStatus Status { get; set; } = UploadStatus.Received;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
