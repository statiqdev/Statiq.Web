using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Web.Azure
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
        private const string AsyncDeployment = nameof(AsyncDeployment);
        private const string Timeout = nameof(Timeout);

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
            directory.ThrowIfNull(nameof(directory));
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
            zipPath.ThrowIfNull(nameof(zipPath));
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
            contentProviderFactory.ThrowIfNull(nameof(contentProviderFactory));
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
                    { SiteName, siteName.ThrowIfNull(nameof(siteName)) },
                    { Username, username.ThrowIfNull(nameof(username)) },
                    { Password, password.ThrowIfNull(nameof(password)) },
                    { ContentProvider, contentProvider.ThrowIfNull(nameof(contentProvider)) },
                    { AsyncDeployment, Config.FromValue(true) },
                    { Timeout, Config.FromValue(TimeSpan.FromMinutes(30)) }
                },
                false)
        {
        }

        /// <summary>
        /// Sets the timeout for deployment (the default is 30 minutes).
        /// </summary>
        /// <param name="timeout">
        /// The timeout for deployment. If a successful deployment is not
        /// indicated after this time, an exception will be thrown.
        /// </param>
        /// <returns>The current module instance.</returns>
        public DeployAppService WithTimeout(Config<TimeSpan> timeout) => (DeployAppService)SetConfig(Timeout, timeout);

        /// <summary>
        /// Configures Kudu to use asynchronous deployment (the default is <c>true</c>).
        /// </summary>
        /// <remarks>
        /// See https://github.com/projectkudu/kudu/wiki/Deploying-from-a-zip-file-or-url#asynchronous-zip-deployment
        /// for more information about Kudu and asychronous deployment. When using asynchronous deployment, the module
        /// will poll for completion every 10 seconds until the <see cref="WithTimeout(Config{TimeSpan})"/>
        /// is reached.
        /// </remarks>
        /// <param name="isAsync"><c>true</c> to use asynchronous deployment, <c>false</c> otherwise.</param>
        /// <returns>The current module instance.</returns>
        public DeployAppService WithAsyncDeployment(Config<bool> isAsync) => (DeployAppService)SetConfig(AsyncDeployment, isAsync);

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

            // Get other settings
            IContentProvider contentProvider = values.Get<IContentProvider>(ContentProvider) ?? throw new Exception("Invalid content provider");
            bool asyncDeployment = values.GetBool(AsyncDeployment);
            TimeSpan timeout = values.Get<TimeSpan>(Timeout);

            // Upload it via Kudu REST API
            context.LogDebug($"Starting {(asyncDeployment ? "asynchronous" : "synchronous")} App Service zip deployment to {siteName}...");
            try
            {
                using (Stream zipStream = contentProvider.GetStream())
                {
                    using (HttpClient client = context.CreateHttpClient())
                    {
                        client.Timeout = timeout;
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authParameter);
                        System.Net.Http.StreamContent zipContent = new System.Net.Http.StreamContent(zipStream);
                        if (asyncDeployment)
                        {
                            // Async, see https://github.com/projectkudu/kudu/wiki/Deploying-from-a-zip-file-or-url#asynchronous-zip-deployment
                            HttpResponseMessage response = await client.PostAsync($"https://{siteName}.scm.azurewebsites.net/api/zipdeploy?isAsync=true", zipContent, context.CancellationToken);

                            // Ensure success
                            if (!response.IsSuccessStatusCode)
                            {
                                string responseContent = await response.Content.ReadAsStringAsync();
                                context.LogError($"App Service deployment error to {siteName}: {response.StatusCode} {responseContent}");
                                response.EnsureSuccessStatusCode();
                            }

                            // Poll for deployment success
                            Uri pollingLocation = response.Headers.Location;
                            context.LogDebug($"App Service zip upload success to {siteName}, polling {pollingLocation} for success");
                            DateTime endTime = DateTime.Now.Add(timeout);
                            int c = 0;
                            while (true)
                            {
                                c++;

                                // Check timeout
                                if (DateTime.Now > endTime)
                                {
                                    throw new Exception($"Timeout expired waiting for App Service zip deployment success to {siteName} while polling {pollingLocation}");
                                }

                                // Request the polling response
                                HttpResponseMessage pollingResponse = await client.GetAsync(pollingLocation);
                                context.LogDebug($"App Service zip deployment to {siteName}, polling attempt {c}: {pollingResponse.StatusCode}");

                                // Ensure success
                                if (!response.IsSuccessStatusCode)
                                {
                                    string responseContent = await response.Content.ReadAsStringAsync();
                                    context.LogError($"App Service deployment error to {siteName}: {response.StatusCode} {responseContent}");
                                    response.EnsureSuccessStatusCode();
                                }

                                // Check for completion
                                if (pollingResponse.StatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    context.LogDebug($"App Service deployment success to {siteName}");
                                    break;
                                }

                                // Wait to try again
                                await Task.Delay(TimeSpan.FromSeconds(10));
                            }
                        }
                        else
                        {
                            // Synchronous
                            HttpResponseMessage response = await client.PostAsync($"https://{siteName}.scm.azurewebsites.net/api/zipdeploy", zipContent, context.CancellationToken);

                            // Ensure success
                            if (!response.IsSuccessStatusCode)
                            {
                                string responseContent = await response.Content.ReadAsStringAsync();
                                context.LogError($"App Service deployment error to {siteName}: {response.StatusCode} {responseContent}");
                                response.EnsureSuccessStatusCode();
                            }

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
