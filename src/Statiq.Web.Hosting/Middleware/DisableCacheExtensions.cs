using System;
using Microsoft.AspNetCore.Builder;

namespace Statiq.Web.Hosting.Middleware
{
    internal static class DisableCacheExtensions
    {
        public static IApplicationBuilder UseDisableCache(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<DisableCacheMiddleware>();
        }
    }
}