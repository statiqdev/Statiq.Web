using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Wyam.Hosting.Middleware
{
    public static class DefaultExtensionsExtensions
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
