using System;
using Microsoft.AspNetCore.Builder;

namespace Statiq.Web.Hosting.Middleware
{
    internal static class VirtualDirectoryExtensions
    {
        public static IApplicationBuilder UseVirtualDirectory(this IApplicationBuilder builder, string virtualDirectory)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<VirtualDirectoryMiddleware>(virtualDirectory);
        }
    }
}