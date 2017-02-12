using System;

using Owin;

namespace Wyam.LiveReload
{
    public static class LiveReloadScriptInjectionMiddlewareExtensions
    {
        public static IAppBuilder UseLiveReloadScriptInjections(this IAppBuilder builder, string scriptPath)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            return builder.Use<LiveReloadScriptInjectionMiddleware>(scriptPath);
        }
    }
}