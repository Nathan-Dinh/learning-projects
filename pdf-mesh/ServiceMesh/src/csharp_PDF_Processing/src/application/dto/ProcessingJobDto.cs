using csharp_PDF_Processing.Src.Domain.Model;

namespace csharp_PDF_Processing.Src.Application.Dto
{
    /// <summary>
    /// DTO for API responses. Immutable by design.
    /// </summary>
    public record ProcessingJobDto(int Id, string FileName, JobStatus Status, DateTime CreatedAt);
}
