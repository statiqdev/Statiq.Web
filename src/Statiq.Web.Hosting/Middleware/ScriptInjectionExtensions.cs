using System;
using Microsoft.AspNetCore.Builder;

namespace Statiq.Web.Hosting.Middleware
{
    internal static class ScriptInjectionExtensions
    {
        public static IApplicationBuilder UseScriptInjection(this IApplicationBuilder builder, params string[] scriptUrls)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<ScriptInjectionMiddleware>(new object[] { scriptUrls });
        }
    }
}