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
    public static class BootstrapperExtensions
    {
        /// <summary>
        /// Adds the "preview" and "serve" commands (these are added by default when you
        /// call <see cref="BootstrapperFactoryExtensions.CreateWeb(BootstrapperFactory, string[])"/>.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <returns>The bootstrapper.</returns>
        public static Bootstrapper AddHostingCommands(this Bootstrapper bootstrapper)
        {
            _ = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
            bootstrapper.AddCommand<PreviewCommand>();
            bootstrapper.AddCommand<ServeCommand>();
            return bootstrapper;
        }

        /// <summary>
        /// Configures the set of template modules.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="action">The configuration action.</param>
        /// <returns>The bootstrapper.</returns>
        public static Bootstrapper ConfigureTemplateModules(this Bootstrapper bootstrapper, Action<TemplateModules> action) =>
            bootstrapper.ConfigureServices(services =>
                action(services
                    .BuildServiceProvider()
                    .GetRequiredService<TemplateModules>()));
    }
}
