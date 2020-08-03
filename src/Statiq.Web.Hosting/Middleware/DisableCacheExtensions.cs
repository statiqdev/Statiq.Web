using System;
using Microsoft.AspNetCore.Builder;
using Statiq.Common;

namespace Statiq.Web.Hosting.Middleware
{
    internal static class DisableCacheExtensions
    {
        public static IApplicationBuilder UseDisableCache(this IApplicationBuilder builder)
        {
            builder.ThrowIfNull(nameof(builder));
            return builder.UseMiddleware<DisableCacheMiddleware>();
        }
    }
}