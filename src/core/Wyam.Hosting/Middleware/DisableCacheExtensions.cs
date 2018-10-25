using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Wyam.Hosting.Middleware;

namespace Wyam.Hosting.Middleware
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