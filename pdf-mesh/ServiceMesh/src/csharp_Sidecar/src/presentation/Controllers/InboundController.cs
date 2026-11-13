using csharp_Sidecar.Src.Application.Services;
using csharp_Sidecar.Src.Domain.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace csharp_Sidecar.Src.Presentation.Controllers
{
    /// <summary>
    /// Receives forwarded requests from the OutboundController, validates the source service against auth policies, and forwards to the local service.
    /// </summary>
    /// <param name="http"></param>
    /// <param name="authPolicy"></param>
    /// <param name="options"></param>
    [ApiController]
    [Route("inbound")]
    public class InboundController(
        HttpClient http,
        CheckAuthPolicy authPolicy,
        IOptions<SideCarOptions> options) : ControllerBase
    {
        private readonly SideCarOptions _cfg = options.Value;

        /// <summary>
        /// Accepts all HTTP methods and paths, validates the source service from the X-Source-Service header, checks auth policies, and forwards to the local service URL defined in config.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [AcceptVerbs("GET", "POST", "PUT", "DELETE", "PATCH", Route = "{**path}")]
        public async Task<IActionResult> Forward(string path)
        {
            // Validate source service from header
            var source = Request.Headers["X-Source-Service"].ToString();

            if (string.IsNullOrEmpty(source)) return BadRequest("Missing X-Source-Service header");

            if (!authPolicy.IsAllowed(source)) return StatusCode(403, $"{source} is not allowed to call {_cfg.ServiceName}");

            // Forward to service URL defined in mesh config
            var target = $"{_cfg.LocalServiceUrl}/{path}{Request.QueryString}";

            // Create a new HttpRequestMessage with the same method and target URL
            var forward = new HttpRequestMessage(new HttpMethod(Request.Method), target);

            // Post/Put/Patch requests need to include the body and content type to the new request
            if (HttpMethods.IsPost(Request.Method) || HttpMethods.IsPut(Request.Method) || HttpMethods.IsPatch(Request.Method))
            {
                // Read the request body as a byte array and forward it with the same content type
                using var ms = new MemoryStream();

                // Note: We have to copy the request body to a memory stream because the original stream can only be read once, and we need to read it here to forward it. In a real implementation, we might want to use a more efficient way to forward the stream without buffering the entire content in memory, especially for large requests.
                await Request.Body.CopyToAsync(ms);

                var bytes = ms.ToArray();
                forward.Content = new ByteArrayContent(bytes);

                // If the original request has a content type header, copy it to the forwarded request
                if (!string.IsNullOrEmpty(Request.ContentType) && System.Net.Http.Headers.MediaTypeHeaderValue.TryParse(Request.ContentType, out var ct))
                    forward.Content.Headers.ContentType = ct;
            }

            var response = await http.SendAsync(forward);
            var body = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, body);
        }
    }
}
