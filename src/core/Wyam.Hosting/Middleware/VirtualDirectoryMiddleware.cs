using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Wyam.Hosting.Middleware
{
    /// <summary>
    /// Implements OWIN support for mapping virtual directories.
    /// </summary>
    internal class VirtualDirectoryMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _virtualDirectory;

        public VirtualDirectoryMiddleware(RequestDelegate next, string virtualDirectory)
        {
            if (virtualDirectory == null)
            {
                throw new ArgumentNullException(nameof(virtualDirectory));
            }

            _next = next ?? throw new ArgumentNullException(nameof(next));
            _virtualDirectory = NormalizeVirtualDirectory(virtualDirectory);
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.ToString().StartsWith(_virtualDirectory))
            {
                string realPath = context.Request.Path.ToString().Substring(_virtualDirectory.Length);
                if (realPath?.Length == 0)
                {
                    realPath = "/";
                }
                context.Request.Path = new PathString(realPath);
                context.Request.PathBase = new PathString(_virtualDirectory);
                await _next(context);
                return;
            }

            // This isn't under our virtual directory, so it should be a not found
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync(string.Empty);
        }

        public static string NormalizeVirtualDirectory(string virtualDirectory)
        {
            if (!virtualDirectory.StartsWith("/"))
            {
                virtualDirectory = "/" + virtualDirectory;
            }
            return virtualDirectory.TrimEnd('/');
        }
    }
}