using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Azure
{
    /// <summary>
    /// Deploys output files to Azure App Service using a zip file.
    /// </summary>
    /// <category>Deployment</category>
    public class DeployAppService : MultiConfigModule
    {
        // Config keys
        private const string SiteName = nameof(SiteName);
        private const string Username = nameof(Username);
        private const string Password = nameof(Password);
        private const string ContentProvider = nameof(ContentProvider);

        /// <summary>
        /// Deploys the output folder to Azure App Service.
        /// </summary>
        /// <param name="siteName">The name of the site to deploy.</param>
        /// <param name="username">The username to authenticate with.</param>
        /// <param name="password">The password to authenticate with.</param>
        public DeployAppService(Config<string> siteName, Config<string> username, Config<string> password)
            : this(siteName, username, password, Config.FromContext(ctx => ctx.FileSystem.GetOutputPath()))
        {
        }

        /// <summary>
        /// Deploys a specified folder to Azure App Service.
        /// </summary>
        /// <param name="siteName">The name of the site to deploy.</param>
        /// <param name="username">The username to authenticate with.</param>
        /// <param name="password">The password to authenticate with.</param>
        /// <param name="directory">
        /// The directory containing the files to deploy (from the root folder, not the input folder).
        /// </param>
        public DeployAppService(Config<string> siteName, Config<string> username, Config<string> password, Config<NormalizedPath> directory)
            : this(siteName, username, password, GetContentProviderFromDirectory(directory))
        {
        }

        private static Config<IContentProvider> GetContentProviderFromDirectory(Config<NormalizedPath> directory)
        {
            _ = directory ?? throw new ArgumentNullException(nameof(directory));
            return directory.Transform(GetContentProvider);

            static IContentProvider GetContentProvider(NormalizedPath path, IExecutionContext context)
            {
                if (path.IsNull)
                {
                    throw new ExecutionException("Invalid directory");
                }
                IFile zipFile = ZipFileHelper.CreateZipFile(context, path);
                return zipFile.GetContentProvider();
            }
        }

        /// <summary>
        /// Deploys a specified zip file to Azure App Service.
        /// </summary>
        /// <param name="zipPath">The zip file to deploy.</param>
        /// <param name="siteName">The name of the site to deploy.</param>
        /// <param name="username">The username to authenticate with.</param>
        /// <param name="password">The password to authenticate with.</param>
        public DeployAppService(Config<NormalizedPath> zipPath, Config<string> siteName, Config<string> username, Config<string> password)
            : this(siteName, username, password, GetContentProviderFromZipFile(zipPath))
        {
        }

        private static Config<IContentProvider> GetContentProviderFromZipFile(Config<NormalizedPath> zipPath)
        {
            _ = zipPath ?? throw new ArgumentNullException(nameof(zipPath));
            return zipPath.Transform(GetContentProvider);

            static IContentProvider GetContentProvider(NormalizedPath filePath, IExecutionContext context)
            {
                if (filePath.IsNull)
                {
                    throw new ExecutionException("Invalid zip path");
                }
                IFile zipFile = context.FileSystem.GetFile(filePath);
                if (!zipFile.Exists)
                {
                    throw new ExecutionException("Zip file does not exist");
                }
                return zipFile.GetContentProvider();
            }
        }

        /// <summary>
        /// Deploys a specified folder to Azure App Service.
        /// </summary>
        /// <param name="siteName">The name of the site to deploy.</param>
        /// <param name="username">The username to authenticate with.</param>
        /// <param name="password">The password to authenticate with.</param>
        /// <param name="contentProviderFactory">A content provider factory that should provide a ZIP stream content provider to deploy.</param>
        public DeployAppService(Config<string> siteName, Config<string> username, Config<string> password, Config<IContentProviderFactory> contentProviderFactory)
            : this(siteName, username, password, GetContentProviderFromContentProviderFactory(contentProviderFactory))
        {
        }

        private static Config<IContentProvider> GetContentProviderFromContentProviderFactory(Config<IContentProviderFactory> contentProviderFactory)
        {
            _ = contentProviderFactory ?? throw new ArgumentNullException(nameof(contentProviderFactory));
            return contentProviderFactory.Transform(factory => factory?.GetContentProvider());
        }

        /// <summary>
        /// Deploys a specified zip file to Azure App Service.
        /// </summary>
        /// <param name="siteName">The name of the site to deploy.</param>
        /// <param name="username">The username to authenticate with.</param>
        /// <param name="password">The password to authenticate with.</param>
        /// <param name="contentProvider">A content provider that should provide a ZIP stream to deploy.</param>
        public DeployAppService(Config<string> siteName, Config<string> username, Config<string> password, Config<IContentProvider> contentProvider)
            : base(
                new Dictionary<string, IConfig>
                {
                    { SiteName, siteName ?? throw new ArgumentNullException(nameof(siteName)) },
                    { Username, username ?? throw new ArgumentNullException(nameof(username)) },
                    { Password, password ?? throw new ArgumentNullException(nameof(password)) },
                    { ContentProvider, contentProvider ?? throw new ArgumentNullException(nameof(contentProvider)) }
                },
                false)
        {
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, IMetadata values)
        {
            // Get the site name
            string siteName = values.GetString(SiteName) ?? throw new ExecutionException("Invalid site name");

            // Get the username and password
            // See https://stackoverflow.com/a/45083787/807064 if we ever want to accept an authorization file
            string username = values.GetString(Username) ?? throw new ExecutionException("Invalid username");
            string password = values.GetString(Password) ?? throw new ExecutionException("Invalid password");
            byte[] authParameterBytes = Encoding.ASCII.GetBytes(username + ":" + password);
            string authParameter = Convert.ToBase64String(authParameterBytes);

            IContentProvider contentProvider = values.Get<IContentProvider>(ContentProvider) ?? throw new Exception("Invalid content provider");

            // Upload it via Kudu REST API
            context.LogDebug($"Starting App Service deployment to {siteName}...");
            try
            {
                using (Stream zipStream = contentProvider.GetStream())
                {
                    using (HttpClient client = context.CreateHttpClient())
                    {
                        client.Timeout = TimeSpan.FromMinutes(10);  // Set a long timeout for App Service uploads
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authParameter);
                        System.Net.Http.StreamContent zipContent = new System.Net.Http.StreamContent(zipStream);
                        HttpResponseMessage response = await client.PostAsync($"https://{siteName}.scm.azurewebsites.net/api/zipdeploy", zipContent, context.CancellationToken);
                        if (!response.IsSuccessStatusCode)
                        {
                            string responseContent = await response.Content.ReadAsStringAsync();
                            context.LogError($"App Service deployment error: {response.StatusCode} {responseContent}");
                            response.EnsureSuccessStatusCode();
                        }
                        else
                        {
                            context.LogDebug($"App Service deployment success to {siteName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                context.LogError($"Exception while deploying App Service {ex.Message}");
                throw;
            }

            return await input.YieldAsync();
        }
    }
}
