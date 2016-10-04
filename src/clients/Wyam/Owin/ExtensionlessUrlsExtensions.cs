using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace Wyam.Owin
{
    public static class ExtensionlessUrlsExtensions
    {
        public static IAppBuilder UseExtensionlessUrls(this IAppBuilder builder)
        {
            return builder.UseExtensionlessUrls(new ExtensionlessUrlsOptions());
        }

        public static IAppBuilder UseExtensionlessUrls(this IAppBuilder builder, ExtensionlessUrlsOptions options)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.Use<ExtensionlessUrlsMiddleware>(options);
        }
    }
}
