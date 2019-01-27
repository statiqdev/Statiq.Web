using System;
using Microsoft.AspNetCore.Builder;

namespace Wyam.Hosting.LiveReload
{
    internal static class LiveReloadExtensions
    {
        public static IApplicationBuilder UseLiveReload(this IApplicationBuilder builder, LiveReloadServer liveReloadServer)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<LiveReloadMiddleware>(liveReloadServer);
        }
    }
}