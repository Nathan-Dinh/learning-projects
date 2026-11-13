namespace csharp_PDFHandler.Src.Application.Dto
{
    /// <summary>
    /// DTO for creating an upload record. Immutable by design.
    /// </summary>
    public record CreateUploadDto(string FileName, long FileSize);
}
