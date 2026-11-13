using csharp_ControlPlane.Src.Domain.Model;
using csharp_ControlPlane.Src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace csharp_ControlPlane.Src.Application.Services
{
    public class MeshConfigBuilder(ControlPlaneDbContext db)
    {
        public async Task<MeshConfig> BuildForAsync(string serviceName)
        {
            var auth   = await db.AuthPolicies.Where(p => p.Destination == serviceName).ToListAsync();
            var routes = await db.Routes.Where(r => r.Source == serviceName).ToListAsync();

            return new MeshConfig(
                Version: DateTime.UtcNow.Ticks.ToString(),
                AuthPolicies: auth,
                Routes: routes
            );
        }
    }
}
