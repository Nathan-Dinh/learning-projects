namespace csharp_Storage.Src.Application.Dto
{
    /// <summary>
    /// DTO for API responses. Immutable by design.
    /// </summary>
    public record PdfDocumentDto(int Id, string FileName, string JsonContent, DateTime ProcessedAt);
}
