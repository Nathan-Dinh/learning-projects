namespace csharp_Sidecar.Src.Domain.Dto
{
    /// <summary>
    /// Decides whether Source is allowed to call Destination.
    /// </summary>
    public record AuthPolicyDto(
        string Source,
        string Destination,
        bool Allow
    )
    {
        public int Id { get; init; }
    }
}
