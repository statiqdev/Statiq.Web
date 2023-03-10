using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Statiq.Common;

namespace Statiq.Web.Hosting.Middleware
{
    internal class CustomHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IReadOnlyDictionary<string, string> _customHeaders;

        public CustomHeadersMiddleware(
            RequestDelegate next,
            IWebHostEnvironment hostingEnv,
            IOptions<CustomHeadersOptions> options,
            ILoggerFactory loggerFactory)
        {
            hostingEnv.ThrowIfNull(nameof(hostingEnv));
            options.ThrowIfNull(nameof(options));
            loggerFactory.ThrowIfNull(nameof(loggerFactory));

            _next = next.ThrowIfNull(nameof(next));
            _customHeaders = (options.Value ?? new CustomHeadersOptions()).CustomHeaders
                ?? new Dictionary<string, string>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            foreach (KeyValuePair<string, string> header in _customHeaders)
            {
                context.Response.Headers.Append(header.Key, header.Value);
            }
        }
    }
}