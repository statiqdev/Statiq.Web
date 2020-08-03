using System;
using Microsoft.AspNetCore.Builder;
using Statiq.Common;

namespace Statiq.Web.Hosting.Middleware
{
    internal static class ScriptInjectionExtensions
    {
        public static IApplicationBuilder UseScriptInjection(this IApplicationBuilder builder, params string[] scriptUrls)
        {
            builder.ThrowIfNull(nameof(builder));
            return builder.UseMiddleware<ScriptInjectionMiddleware>(new object[] { scriptUrls });
        }
    }
}