using csharp_PDFHandler.Src.Domain.Model;

namespace csharp_PDFHandler.Src.Application.Dto
{
    /// <summary>
    /// DTO for API responses. Immutable by design.
    /// </summary>
    public record UploadRecordDto(int Id, string FileName, long FileSize, UploadStatus Status, DateTime UploadedAt);
}
