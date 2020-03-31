using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Statiq.Web.Hosting.Middleware
{
    internal class DisableCacheMiddleware
    {
        private readonly IWebHostEnvironment _hostingEnv;
        private readonly ILoggerFactory _loggerFactory;
        private readonly RequestDelegate _next;

        public DisableCacheMiddleware(RequestDelegate next, IWebHostEnvironment hostingEnv, ILoggerFactory loggerFactory)
        {
            _next = next;
            _hostingEnv = hostingEnv;
            _loggerFactory = loggerFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            context.Response.Headers.Append("Pragma", "no-cache");
            context.Response.Headers.Append("Expires", "0");
        }
    }
}