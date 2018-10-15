using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace Wyam.Hosting.Middleware
{
    public static class ScriptInjectionExtensions
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