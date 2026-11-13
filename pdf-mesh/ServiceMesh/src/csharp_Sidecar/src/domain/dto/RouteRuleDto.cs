namespace csharp_Sidecar.Src.Domain.Dto
{
    /// <summary>
    /// Maps a logical Destination service name to the host/port the sidecar should forward traffic to.
    /// </summary>
    public record RouteRuleDto(
        string Source,
        string Destination,
        string TargetHost,
        int TargetPort
    )
    {
        public int Id { get; init; }
    }
}
