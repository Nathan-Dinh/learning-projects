using csharp_ControlPlane.Src.Application.Services;
using csharp_ControlPlane.Src.Domain.Model;
using Microsoft.AspNetCore.Mvc;

namespace csharp_ControlPlane.Src.Presentation.Controllers
{
    [ApiController]
    [Route("api")]
    public class ConfigController(MeshConfigBuilder builder) : ControllerBase
    {
        [HttpGet("config/{serviceName}")]
        public Task<MeshConfig> GetConfig(string serviceName) =>
            builder.BuildForAsync(serviceName);
    }
}
