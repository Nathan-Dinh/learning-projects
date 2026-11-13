using csharp_Sidecar.Src.Domain.Dto;

namespace csharp_Sidecar.Src.Infrastructure.Cache
{
    public class ConfigCache
    {
        public string Version { get; set; } = string.Empty;
        public IReadOnlyList<AuthPolicyDto> AuthPolicies { get; set; } = [];
        public IReadOnlyList<RouteRuleDto> Routes { get; set; } = [];
    }
}