namespace csharp_Storage.Src.Application.Dto
{
    /// <summary>
    /// DTO for creating a new document. Immutable by design.
    /// </summary>
    public record CreateDocumentDto(string FileName, string JsonContent);
}
