using System;
using Microsoft.AspNetCore.Builder;
using Statiq.Common;

namespace Statiq.Web.Hosting.Middleware
{
    internal static class VirtualDirectoryExtensions
    {
        public static IApplicationBuilder UseVirtualDirectory(this IApplicationBuilder builder, string virtualDirectory)
        {
            builder.ThrowIfNull(nameof(builder));
            return builder.UseMiddleware<VirtualDirectoryMiddleware>(virtualDirectory);
        }
    }
}