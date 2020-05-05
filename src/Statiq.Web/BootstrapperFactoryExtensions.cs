using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Statiq.App;
using Statiq.Common;

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
                    { WebKeys.ContentFiles, "**/{!_,}*.{html,cshtml,md}" },
                    { WebKeys.DataFiles, "**/{!_,}*.{json,yaml,yml}" },
                    { WebKeys.DirectoryMetadataFiles, "**/_{d,D}irectory.{json,yaml,yml}" },
                    { WebKeys.MirrorResources, true },
                    { WebKeys.ValidateRelativeLinks, true },
                    { WebKeys.GenerateSitemap, true },
                    { WebKeys.Xref, Config.FromDocument(doc => doc.GetTitle().Replace(' ', '-')) },
                    { WebKeys.Excluded, Config.FromDocument(doc => doc.GetPublishedDate(false) > DateTime.Today.AddDays(1)) } // Add +1 days so the threshold is midnight on the current day
                });
    }
}
