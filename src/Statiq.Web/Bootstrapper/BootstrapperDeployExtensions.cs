using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.Web
{
    public static class BootstrapperDeployExtensions
    {
        public static TBootstrapper DeployToGitHubPages<TBootstrapper>(
            this TBootstrapper bootstrapper,
            Config<string> owner,
            Config<string> name,
            Config<string> username,
            Config<string> password)
            where TBootstrapper : IBootstrapper =>
            bootstrapper
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.GitHubOwner, owner },
                    { WebKeys.GitHubName, name },
                    { WebKeys.GitHubUsername, username },
                    { WebKeys.GitHubPassword, password }
                });

        public static TBootstrapper DeployToGitHubPagesBranch<TBootstrapper>(
            this TBootstrapper bootstrapper,
            Config<string> owner,
            Config<string> name,
            Config<string> username,
            Config<string> password,
            Config<string> branch)
            where TBootstrapper : IBootstrapper =>
            bootstrapper
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.GitHubOwner, owner },
                    { WebKeys.GitHubName, name },
                    { WebKeys.GitHubUsername, username },
                    { WebKeys.GitHubPassword, password },
                    { WebKeys.GitHubBranch, branch }
                });

        public static TBootstrapper DeployToGitHubPages<TBootstrapper>(
            this TBootstrapper bootstrapper,
            Config<string> owner,
            Config<string> name,
            Config<string> token)
            where TBootstrapper : IBootstrapper =>
            bootstrapper
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.GitHubOwner, owner },
                    { WebKeys.GitHubName, name },
                    { WebKeys.GitHubToken, token }
                });

        public static TBootstrapper DeployToGitHubPagesBranch<TBootstrapper>(
            this TBootstrapper bootstrapper,
            Config<string> owner,
            Config<string> name,
            Config<string> token,
            Config<string> branch)
            where TBootstrapper : IBootstrapper =>
            bootstrapper
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.GitHubOwner, owner },
                    { WebKeys.GitHubName, name },
                    { WebKeys.GitHubToken, token },
                    { WebKeys.GitHubBranch, branch }
                });

        public static TBootstrapper DeployToNetlify<TBootstrapper>(
            this TBootstrapper bootstrapper,
            Config<string> siteId,
            Config<string> accessToken)
            where TBootstrapper : IBootstrapper =>
            bootstrapper
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.NetlifySiteId, siteId },
                    { WebKeys.NetlifyAccessToken, accessToken }
                });

        public static TBootstrapper DeployToAzureAppService<TBootstrapper>(
            this TBootstrapper bootstrapper,
            Config<string> siteName,
            Config<string> username,
            Config<string> password)
            where TBootstrapper : IBootstrapper =>
            bootstrapper
                .AddSettingsIfNonExisting(new Dictionary<string, object>
                {
                    { WebKeys.AzureAppServiceSiteName, siteName },
                    { WebKeys.AzureAppServiceUsername, username },
                    { WebKeys.AzureAppServicePassword, password }
                });
    }
}
