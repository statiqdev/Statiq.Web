using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Statiq.App;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;
using Statiq.Markdown;
using Statiq.Razor;
using Statiq.Web.Azure;
using Statiq.Web.GitHub;
using Statiq.Web.Modules;
using Statiq.Web.Netlify;
using Statiq.Yaml;

namespace Statiq.Web.Pipelines
{
    public class Deployment : Pipeline
    {
        public Deployment()
        {
            Deployment = true;

            OutputModules = new ModuleList
            {
                // GitHub
                new ExecuteIf(Config.FromSettings(x => x.ContainsKeys(WebKeys.GitHubOwner, WebKeys.GitHubName)))
                {
                    new LogMessage(Config.FromSettings(x => $"Deploying to GitHub repository {x.GetString(WebKeys.GitHubOwner)}/{x.GetString(WebKeys.GitHubName)}")),
                    new ExecuteIf(
                        Config.FromSettings(x => x.ContainsKeys(WebKeys.GitHubUsername, WebKeys.GitHubPassword)),
                        new DeployGitHubPages(
                            Config.FromSetting<string>(WebKeys.GitHubOwner),
                            Config.FromSetting<string>(WebKeys.GitHubName),
                            Config.FromSetting<string>(WebKeys.GitHubUsername),
                            Config.FromSetting<string>(WebKeys.GitHubPassword))
                            .ToBranch(Config.FromSetting(WebKeys.GitHubBranch, DeployGitHubPages.DefaultBranch)))
                        .ElseIf(
                            Config.FromSettings(x => x.ContainsKey(WebKeys.GitHubToken)),
                            new DeployGitHubPages(
                                Config.FromSetting<string>(WebKeys.GitHubOwner),
                                Config.FromSetting<string>(WebKeys.GitHubName),
                                Config.FromSetting<string>(WebKeys.GitHubToken))
                                .ToBranch(Config.FromSetting(WebKeys.GitHubBranch, DeployGitHubPages.DefaultBranch)))
                        .Else(new LogMessage(LogLevel.Error, $"Either {WebKeys.GitHubUsername} and {WebKeys.GitHubPassword} need to be specified or {WebKeys.GitHubToken} needs to be specified"))
                },

                // Netlify
                new ExecuteIf(Config.FromSettings(x => x.ContainsKeys(WebKeys.NetlifySiteId, WebKeys.NetlifyAccessToken)))
                {
                    new LogMessage(Config.FromSettings(x => $"Deploying to Netlify site {x.GetString(WebKeys.NetlifySiteId)}")),
                    new DeployNetlifySite(
                        Config.FromSetting<string>(WebKeys.NetlifySiteId),
                        Config.FromSetting<string>(WebKeys.NetlifyAccessToken))
                },

                // Azure App Service
                new ExecuteIf(Config.FromSettings(x => x.ContainsKeys(WebKeys.AzureAppServiceSiteName, WebKeys.AzureAppServiceUsername, WebKeys.AzureAppServicePassword)))
                {
                    new LogMessage(Config.FromSettings(x => $"Deploying to Azure App Service site {x.GetString(WebKeys.AzureAppServiceSiteName)}")),
                    new DeployAppService(
                        Config.FromSetting<string>(WebKeys.AzureAppServiceSiteName),
                        Config.FromSetting<string>(WebKeys.AzureAppServiceUsername),
                        Config.FromSetting<string>(WebKeys.AzureAppServicePassword))
                }
            };
        }
    }
}
