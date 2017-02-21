using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owin;

namespace Wyam.Hosting.Owin
{
    public static class ScriptInjectionExtensions
    {
        public static IAppBuilder UseScriptInjection(this IAppBuilder builder, params string[] scriptUrls)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.Use<ScriptInjectionMiddleware>(new object[] { scriptUrls });
        }
    }
}
