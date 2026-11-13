using csharp_ControlPlane.Src.Domain.Rules;

namespace csharp_ControlPlane.Src.Domain.Model
{
    public record MeshConfig(
        string Version,
        IReadOnlyList<AuthPolicy> AuthPolicies,
        IReadOnlyList<RouteRule> Routes
    );
}
