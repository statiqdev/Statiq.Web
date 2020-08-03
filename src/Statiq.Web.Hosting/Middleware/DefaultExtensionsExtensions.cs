using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Statiq.Common;

namespace Statiq.Web.Hosting.Middleware
{
    internal static class DefaultExtensionsExtensions
    {
        public static IApplicationBuilder UseDefaultExtensions(this IApplicationBuilder app, DefaultExtensionsOptions options)
        {
            app.ThrowIfNull(nameof(app));
            return app.UseMiddleware<DefaultExtensionsMiddleware>(Options.Create(options));
        }
    }
}
