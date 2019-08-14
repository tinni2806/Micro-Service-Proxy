using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace Micromesh.Controllers
{
    /// <summary>
    /// ping controller
    /// </summary>
    [Route("")]
    public class PingController : Controller
    {
        /// <summary>
        /// default method
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Ping()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            var ip = HttpContext.Features.Get<IHttpConnectionFeature>()?.RemoteIpAddress;

            return Content($"assembly: {assemblyName.Name}, version: {assemblyName.Version}, client IP: {ip}, date: {DateTime.Now:G}");
        }
    }
}
