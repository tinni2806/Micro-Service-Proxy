using Micromesh.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Micromesh.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("/")]
    public class IngressController : Controller
    {
        public IngressController(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<IngressController> logger)
        {
            Configuration = configuration;
            HttpClientFactory = httpClientFactory;
            Logger = logger;
        }

        private IHttpClientFactory HttpClientFactory { get; }

        private IConfiguration Configuration { get; }

        private ILogger<IngressController> Logger { get; }

        /// <summary>
        /// The primary method used to proxy all incoming requests through Micromesh
        /// </summary>
        [AcceptVerbs("GET", "POST", "PUT", "PATCH", "DELETE")]
        [Route("{*path}")]
        public async Task<ContentResult> ProxyRequest(string path)
        {
            try
            {
                var service = Request.Headers[Headers.ForwardTo];
                if (string.IsNullOrEmpty(service))
                {
                    throw new MeshException("Forward-To header missing in request");
                }
                Logger.LogDebug($"Service: {service}, url: {path}");

                var host = Configuration.GetHostFromServicePool(service);
                Logger.LogDebug($"Got {host} from service pool");

                var requestUri = BuildUri(host, path, Request.QueryString.ToString());
                Logger.LogDebug($"Forwarding request to {requestUri}");

                var response = await HttpClientFactory
                    .CreateClient(HttpClients.ResilientClient)
                    .SendAsync(Request.ToHttpRequestMessage(requestUri));

                Logger.LogDebug($"Received response status code {response.StatusCode}");

                return await response.ToContentResultAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Unable to route request, {ex.Message}");
                throw;
            }
        }

        public static Uri BuildUri(string host, string path, string query)
        {
            var uriBuilder = new UriBuilder(host)
            {
                Query = query
            };

            if (path != "/")
            {
                uriBuilder.Path = (uriBuilder.Path != "/") ? $"{uriBuilder.Path}{path}" : $"{path}";
            }
            return uriBuilder.Uri;
        }
    }
}
