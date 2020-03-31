using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Statiq.Web.Hosting.Middleware
{
    internal static class DefaultExtensionsExtensions
    {
        public static IApplicationBuilder UseDefaultExtensions(this IApplicationBuilder app, DefaultExtensionsOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<DefaultExtensionsMiddleware>(Options.Create(options));
        }
    }
}
