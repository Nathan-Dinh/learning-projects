namespace csharp_PDF_Processing.Src.Application.Dto
{
    /// <summary>
    /// DTO for creating a new processing job. Immutable by design.
    /// </summary>
    public record CreateJobDto(string FileName);
}
