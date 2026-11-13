using csharp_Sidecar.Src.Domain.Dto;
using csharp_Sidecar.Src.Infrastructure.Cache;

namespace csharp_Sidecar.Src.Application.Services
{
    public class MeshConfig(ConfigCache cache)
    {
        public void UpdateCache(MeshConfigDto config)
        {
            cache.Version = config.Version;
            cache.AuthPolicies = config.AuthPolicies;
            cache.Routes = config.Routes;
        }
    }
}
