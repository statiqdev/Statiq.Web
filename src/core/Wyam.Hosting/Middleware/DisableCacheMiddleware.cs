using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Wyam.Hosting.Middleware
{
    public class DisableCacheMiddleware
    {
        private readonly IHostingEnvironment _hostingEnv;
        private readonly ILoggerFactory _loggerFactory;
        private readonly RequestDelegate _next;

        public DisableCacheMiddleware(RequestDelegate next, IHostingEnvironment hostingEnv, ILoggerFactory loggerFactory)
        {
            _next = next;
            _hostingEnv = hostingEnv;
            _loggerFactory = loggerFactory;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);

            context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            context.Response.Headers.Append("Pragma", "no-cache");
            context.Response.Headers.Append("Expires", "0");
        }
    }
}