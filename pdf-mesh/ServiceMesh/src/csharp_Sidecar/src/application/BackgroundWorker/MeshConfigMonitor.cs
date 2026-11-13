using csharp_Sidecar.Src.Application.Services;
using csharp_Sidecar.Src.Domain.Dto;
using csharp_Sidecar.Src.Domain.Model;
using Microsoft.Extensions.Options;

namespace csharp_Sidecar.Src.Application.BackgroundWorker
{
    public class MeshConfigMonitor(HttpClient http, MeshConfig meshConfig, IOptions<SideCarOptions> options) : BackgroundService
    {
        private readonly SideCarOptions _cfg = options.Value;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var url = $"{_cfg.ControlPlaneUrl}/api/config/{_cfg.ServiceName}";
                var config = await http.GetFromJsonAsync<MeshConfigDto>(url, stoppingToken);

                if (config is not null) meshConfig.UpdateCache(config);

                await Task.Delay(TimeSpan.FromSeconds(_cfg.RefreshSeconds), stoppingToken);
            }
        }
    }
}
