using csharp_Sidecar.Src.Application.Services;
using csharp_Sidecar.Src.Domain.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace csharp_Sidecar.Src.Presentation.Controllers
{
    /// <summary>
    /// Receives requests from the local service, looks up the destination service in the mesh config, and forwards to the InboundController of the target service's sidecar. The destination service is specified as the first segment of the path, and the rest of the path and query string are forwarded as-is.
    /// </summary>
    /// <param name="http"></param>
    /// <param name="routeRule"></param>
    /// <param name="options"></param>
    [ApiController]
    [Route("outbound")]
    public class OutboundController(
        HttpClient http,
        CheckRouteRule routeRule,
        IOptions<SideCarOptions> options) : ControllerBase
    {
        private readonly SideCarOptions _cfg = options.Value;

        /// <summary>
        /// Accepts all HTTP methods and paths, looks up the destination service in the mesh config, and forwards to the InboundController of the target service's sidecar. The destination service is specified as the first segment of the path, and the rest of the path and query string are forwarded as-is.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        [AcceptVerbs("GET", "POST", "PUT", "DELETE", "PATCH", Route = "{destination}/{**path}")]
        public async Task<IActionResult> Forward(string destination, string path)
        {
            // Look up the destination service in the mesh config to find the target host and port
            var route = routeRule.Find(destination);

            if (route is null) return NotFound($"No route for {destination}");

            var target = $"http://{route.TargetHost}:{route.TargetPort}/inbound/{path}{Request.QueryString}";

            var forward = new HttpRequestMessage(new HttpMethod(Request.Method), target);

            // Copy all headers from the original request to the forwarded request, except for Host and Content-Length which are set by HttpClient
            if (HttpMethods.IsPost(Request.Method) || HttpMethods.IsPut(Request.Method) || HttpMethods.IsPatch(Request.Method))
            {
                // Read the request body as a byte array and forward it with the same content type
                using var ms = new MemoryStream();

                // Note: We have to copy the request body to a memory stream because the original stream can only be read once, and we need to read it here to forward it. In a real implementation, we might want to use a more efficient way to forward the stream without buffering the entire content in memory, especially for large requests.
                await Request.Body.CopyToAsync(ms);

                var bytes = ms.ToArray();

                // Create a new ByteArrayContent for the forwarded request body and set the content type header to match the original request
                forward.Content = new ByteArrayContent(bytes);

                // If the original request has a content type header, copy it to the forwarded request
                if (!string.IsNullOrEmpty(Request.ContentType) && System.Net.Http.Headers.MediaTypeHeaderValue.TryParse(Request.ContentType, out var ct))
                    forward.Content.Headers.ContentType = ct;
            }

            forward.Headers.Add("X-Source-Service", _cfg.ServiceName);

            var response = await http.SendAsync(forward);
            var body = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, body);
        }
    }
}
