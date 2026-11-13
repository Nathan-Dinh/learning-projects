using csharp_Sidecar.Src.Domain.Model;
using csharp_Sidecar.Src.Infrastructure.Cache;
using Microsoft.Extensions.Options;

namespace csharp_Sidecar.Src.Application.Services
{
    public class CheckAuthPolicy(ConfigCache cache, IOptions<SideCarOptions> options)
    {
        private readonly SideCarOptions _cfg = options.Value;

        public bool IsAllowed(string source)
        {
            var policy = cache.AuthPolicies.FirstOrDefault(p => p.Source == source && p.Destination == _cfg.ServiceName);
            return policy is not null && policy.Allow;
        }
    }
}
