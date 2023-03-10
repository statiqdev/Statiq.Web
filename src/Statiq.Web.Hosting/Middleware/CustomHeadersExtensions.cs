using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Statiq.Common;

namespace Statiq.Web.Hosting.Middleware
{
    internal static class CustomHeadersExtensions
    {
        public static IApplicationBuilder UseCustomHeaders(this IApplicationBuilder app, CustomHeadersOptions options)
        {
            app.ThrowIfNull(nameof(app));
            return app.UseMiddleware<CustomHeadersMiddleware>(Options.Create(options));
        }
    }
}