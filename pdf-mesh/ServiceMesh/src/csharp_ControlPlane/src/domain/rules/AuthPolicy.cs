namespace csharp_ControlPlane.Src.Domain.Rules
{
    /// <summary>
    /// Decides whether Source is allowed to call Destination.
    /// </summary>
    public record AuthPolicy(
        string Source,
        string Destination,
        bool Allow
    )
    {
        public int Id { get; init; }
    }
}
