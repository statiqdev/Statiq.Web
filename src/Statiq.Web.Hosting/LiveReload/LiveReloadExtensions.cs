using System;
using Microsoft.AspNetCore.Builder;
using Statiq.Common;

namespace Statiq.Web.Hosting.LiveReload
{
    internal static class LiveReloadExtensions
    {
        public static IApplicationBuilder UseLiveReload(this IApplicationBuilder builder, LiveReloadServer liveReloadServer)
        {
            builder.ThrowIfNull(nameof(builder));
            return builder.UseMiddleware<LiveReloadMiddleware>(liveReloadServer);
        }
    }
}