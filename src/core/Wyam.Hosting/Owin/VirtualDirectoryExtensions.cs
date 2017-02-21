using System;
using Owin;

namespace Wyam.Hosting.Owin
{
    public static class VirtualDirectoryExtensions
    {
        public static IAppBuilder UseVirtualDirectory(this IAppBuilder builder, string virtualDirectory)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.Use<VirtualDirectoryMiddleware>(virtualDirectory);
        }
    }
}