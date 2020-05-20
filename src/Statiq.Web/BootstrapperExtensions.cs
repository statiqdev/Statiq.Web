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

        public static Bootstrapper SetDefaultTemplate(this Bootstrapper bootstrapper, string defaultTemplate) =>
            bootstrapper.ConfigureTemplates(templates => templates.DefaultTemplate = defaultTemplate);

        /// <summary>
        /// Configures the set of template modules.
        /// </summary>
        /// <param name="bootstrapper">The current bootstrapper.</param>
        /// <param name="action">The configuration action.</param>
        /// <returns>The bootstrapper.</returns>
        public static Bootstrapper ConfigureTemplates(this Bootstrapper bootstrapper, Action<Templates> action) =>
            bootstrapper.ConfigureServices(services =>
                action(services
                    .BuildServiceProvider() // We need to build an intermediate service provider to get access to the singleton
                    .GetRequiredService<Templates>()));

        public static Bootstrapper DeployToGitHubPages(
            this Bootstrapper bootstrapper,
            Config<string> owner,
            Config<string> name,
            Config<string> username,
            Config<string> password) =>
            bootstrapper
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.GitHubOwner, owner },
                    { WebKeys.GitHubName, name },
                    { WebKeys.GitHubUsername, username },
                    { WebKeys.GitHubPassword, password }
                });

        public static Bootstrapper DeployToGitHubPagesBranch(
            this Bootstrapper bootstrapper,
            Config<string> owner,
            Config<string> name,
            Config<string> username,
            Config<string> password,
            Config<string> branch) =>
            bootstrapper
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.GitHubOwner, owner },
                    { WebKeys.GitHubName, name },
                    { WebKeys.GitHubUsername, username },
                    { WebKeys.GitHubPassword, password },
                    { WebKeys.GitHubBranch, branch }
                });

        public static Bootstrapper DeployToGitHubPages(
            this Bootstrapper bootstrapper,
            Config<string> owner,
            Config<string> name,
            Config<string> token) =>
            bootstrapper
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.GitHubOwner, owner },
                    { WebKeys.GitHubName, name },
                    { WebKeys.GitHubToken, token }
                });

        public static Bootstrapper DeployToGitHubPagesBranch(
            this Bootstrapper bootstrapper,
            Config<string> owner,
            Config<string> name,
            Config<string> token,
            Config<string> branch) =>
            bootstrapper
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.GitHubOwner, owner },
                    { WebKeys.GitHubName, name },
                    { WebKeys.GitHubToken, token },
                    { WebKeys.GitHubBranch, branch }
                });

        public static Bootstrapper DeployToNetlify(
            this Bootstrapper bootstrapper,
            Config<string> siteId,
            Config<string> accessToken) =>
            bootstrapper
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.NetlifySiteId, siteId },
                    { WebKeys.NetlifyAccessToken, accessToken }
                });

        public static Bootstrapper DeployToAzureAppService(
            this Bootstrapper bootstrapper,
            Config<string> siteName,
            Config<string> username,
            Config<string> password) =>
            bootstrapper
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.AzureAppServiceSiteName, siteName },
                    { WebKeys.AzureAppServiceUsername, username },
                    { WebKeys.AzureAppServicePassword, password }
                });
    }
}
