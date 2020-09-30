using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Statiq.App;
using Statiq.Common;
using Statiq.Core;
using Statiq.Web.Commands;
using Statiq.Web.Modules;

namespace Statiq.Web
{
    public static class BootstrapperDeployExtensions
    {
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
