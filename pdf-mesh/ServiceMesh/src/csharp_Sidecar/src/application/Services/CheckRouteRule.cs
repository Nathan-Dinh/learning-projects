using csharp_Sidecar.Src.Domain.Dto;
using csharp_Sidecar.Src.Infrastructure.Cache;

namespace csharp_Sidecar.Src.Application.Services
{
    public class CheckRouteRule(ConfigCache cache)
    {
        public RouteRuleDto? Find(string destination)
        {
            return cache.Routes.FirstOrDefault(r => r.Destination == destination);
        }
    }
}
