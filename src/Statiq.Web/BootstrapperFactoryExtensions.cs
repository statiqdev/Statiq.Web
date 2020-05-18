using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Statiq.App;
using Statiq.Common;
using Statiq.Web.Modules;

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
            factory.CreateDefault(args).AddWeb();

        /// <summary>
        /// Adds Statiq Web functionality to an existing bootstrapper.
        /// This method does not need to be called if using <see cref="CreateWeb(BootstrapperFactory, string[])"/>.
        /// </summary>
        /// <remarks>
        /// This method is useful when you want to add Statiq Web support to an existing bootstrapper,
        /// for example because you created the bootstrapper without certain default functionality
        /// by calling <see cref="Statiq.App.BootstrapperFactoryExtensions.CreateDefaultWithout(BootstrapperFactory, string[], DefaultFeatures)"/>.
        /// </remarks>
        /// <param name="boostrapper">The bootstrapper to add Statiq Web functionality to.</param>
        /// <returns>The bootstrapper.</returns>
        public static Bootstrapper AddWeb(this Bootstrapper boostrapper) =>
            boostrapper
                .AddPipelines(typeof(BootstrapperFactoryExtensions).Assembly)
                .AddHostingCommands()
                .ConfigureEngine(engine => engine.FileSystem.InputPaths.Add("theme/input"))
                .ConfigureServices(services => services.AddSingleton(new Templates()))
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.ContentFiles, "**/{!_,}*.{html,cshtml,md}" },
                    { WebKeys.DataFiles, $"**/{{!_,}}*.{{{string.Join(",", ParseDataContent.SupportedExtensions)}}}" },
                    { WebKeys.DirectoryMetadataFiles, "**/_{d,D}irectory.{json,yaml,yml}" },
                    { WebKeys.Xref, Config.FromDocument(doc => doc.GetTitle().Replace(' ', '-')) },
                    { WebKeys.Excluded, Config.FromDocument(doc => doc.GetPublishedDate(false) > DateTime.Today.AddDays(1)) } // Add +1 days so the threshold is midnight on the current day
                });
    }
}
