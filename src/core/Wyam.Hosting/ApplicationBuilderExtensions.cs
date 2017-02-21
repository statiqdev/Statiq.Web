using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Owin.Builder;
using Owin;

namespace Wyam.Hosting
{
    internal static class ApplicationBuilderExtensions
    {
        // http://stackoverflow.com/a/30742029/2001966
        public static void UseOwinBuilder(this IApplicationBuilder app, Action<IAppBuilder> owinConfiguration)
        {
            app.UseOwin(
                addToPipeline =>
                {
                    addToPipeline(
                        next =>
                        {
                            AppBuilder builder = new AppBuilder();
                            owinConfiguration(builder);
                            builder.Run(ctx => next(ctx.Environment));

                            // ReSharper disable once SuggestVarOrType_Elsewhere
                            var appFunc = (Func<IDictionary<string, object>, Task>)builder.Build(typeof(Func<IDictionary<string, object>, Task>));

                            return appFunc;
                        });
                });
        }
    }
}
