namespace csharp_Sidecar.Src.Domain.Dto
{
    public record MeshConfigDto(
        string Version,
        IReadOnlyList<AuthPolicyDto> AuthPolicies,
        IReadOnlyList<RouteRuleDto> Routes
    );
}
