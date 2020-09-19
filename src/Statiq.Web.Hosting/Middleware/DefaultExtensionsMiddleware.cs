using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Statiq.Common;

namespace Statiq.Web.Hosting.Middleware
{
    internal class DefaultExtensionsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Microsoft.Extensions.FileProviders.IFileProvider _fileProvider;
        private readonly string[] _extensions;
        private readonly ILogger _logger;

        public DefaultExtensionsMiddleware(RequestDelegate next, IWebHostEnvironment hostingEnv, IOptions<DefaultExtensionsOptions> options, ILoggerFactory loggerFactory)
        {
            hostingEnv.ThrowIfNull(nameof(hostingEnv));
            options.ThrowIfNull(nameof(options));
            loggerFactory.ThrowIfNull(nameof(loggerFactory));

            _next = next.ThrowIfNull(nameof(next));
            _fileProvider = hostingEnv.WebRootFileProvider;
            _extensions = (options.Value ?? new DefaultExtensionsOptions()).Extensions.Select(x => x.StartsWith(".") ? x : ("." + x)).ToArray();
            _logger = loggerFactory.CreateLogger<DefaultExtensionsMiddleware>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (IsGetOrHeadMethod(context.Request.Method)
                && !PathEndsInSlash(context.Request.Path))
            {
                // Check if there's a file with a matched extension, and rewrite the request if found
                foreach (string extension in _extensions)
                {
                    string filePath = context.Request.Path.Value + extension;
                    IFileInfo fileInfo = _fileProvider.GetFileInfo(filePath);
                    if (fileInfo?.Exists == true)
                    {
                        _logger.LogInformation($"Rewriting extensionless path to {filePath}");
                        context.Request.Path = new PathString(filePath);
                        break;
                    }
                }
            }
            await _next(context);
        }

        private static bool IsGetOrHeadMethod(string method) =>
            HttpMethods.IsGet(method) || HttpMethods.IsHead(method);

        private static bool PathEndsInSlash(PathString path) =>
            path.Value.EndsWith("/", StringComparison.Ordinal);
    }
}
