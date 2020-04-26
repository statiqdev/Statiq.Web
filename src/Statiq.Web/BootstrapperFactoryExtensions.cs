using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Statiq.App;
using Statiq.Common;
using Statiq.Web.Commands;
using Statiq.Web.Shortcodes;

namespace Statiq.Web
{
    public static class BootstrapperFactoryExtensions
    {
        /// <summary>
        /// Creates a bootstrapper with all functionality for Statiq Web.
        /// </summary>
        /// <param name="factory">The bootstrapper factory.</param>
        /// <param name="args">The command line arguments.</param>
        /// <returns>A bootstrapper.</returns>
        public static Bootstrapper CreateWeb(this BootstrapperFactory factory, string[] args) =>
            factory
                .CreateDefault(args)
                .AddPipelines(typeof(BootstrapperFactoryExtensions).Assembly)
                .AddHostingCommands()
                .ConfigureEngine(engine => engine.FileSystem.InputPaths.Add("theme"))
                .ConfigureServices(services => services.AddSingleton(new Templates()))
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.MirrorResources, true },
                    { WebKeys.ValidateRelativeLinks, true },
                    { WebKeys.Xref, Config.FromDocument(doc => doc.GetTitle().Replace(' ', '-')) }
                });
    }
}
