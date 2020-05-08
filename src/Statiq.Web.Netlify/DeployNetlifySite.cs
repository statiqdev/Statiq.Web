using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetlifySharp;
using Statiq.Common;

namespace Statiq.Web.Netlify
{
    public class DeployNetlifySite : MultiConfigModule
    {
        // Config keys
        private const string AccessToken = nameof(AccessToken);
        private const string SiteId = nameof(SiteId);
        private const string ContentProvider = nameof(ContentProvider);

        /// <summary>
        /// Deploys the output folder to Netlify.
        /// </summary>
        /// <param name="siteId">The ID of the site to deploy.</param>
        /// <param name="accessToken">The access token to authenticate with.</param>
        public DeployNetlifySite(Config<string> siteId, Config<string> accessToken)
            : this(siteId, accessToken, Config.FromContext(ctx => ctx.FileSystem.GetOutputPath()))
        {
        }

        /// <summary>
        /// Deploys a specified folder to Netlify.
        /// </summary>
        /// <param name="siteId">The ID of the site to deploy.</param>
        /// <param name="accessToken">The access token to authenticate with.</param>
        /// <param name="directory">
        /// The directory containing the files to deploy (from the root folder, not the input folder).
        /// </param>
        public DeployNetlifySite(Config<string> siteId, Config<string> accessToken, Config<NormalizedPath> directory)
            : this(siteId, accessToken, GetContentProviderFromDirectory(directory))
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
        /// Deploys a specified zip file to Netlify.
        /// </summary>
        /// <param name="siteId">The ID of the site to deploy.</param>
        /// <param name="accessToken">The access token to authenticate with.</param>
        /// <param name="zipPath">The zip file to deploy.</param>
        public DeployNetlifySite(Config<NormalizedPath> zipPath, Config<string> siteId, Config<string> accessToken)
            : this(siteId, accessToken, GetContentProviderFromZipFile(zipPath))
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
        /// Deploys a specified zip stream to Netlify.
        /// </summary>
        /// <param name="siteId">The ID of the site to deploy.</param>
        /// <param name="accessToken">The access token to authenticate with.</param>
        /// <param name="contentProviderFactory">A content provider factory that should provide a ZIP stream content provider to deploy.</param>
        public DeployNetlifySite(Config<string> siteId, Config<string> accessToken, Config<IContentProviderFactory> contentProviderFactory)
            : this(siteId, accessToken, GetContentProviderFromContentProviderFactory(contentProviderFactory))
        {
        }

        private static Config<IContentProvider> GetContentProviderFromContentProviderFactory(Config<IContentProviderFactory> contentProviderFactory)
        {
            _ = contentProviderFactory ?? throw new ArgumentNullException(nameof(contentProviderFactory));
            return contentProviderFactory.Transform(factory => factory?.GetContentProvider());
        }

        /// <summary>
        /// Deploys a specified zip stream to Netlify.
        /// </summary>
        /// <param name="siteId">The ID of the site to deploy.</param>
        /// <param name="accessToken">The access token to authenticate with.</param>
        /// <param name="contentProvider">A content provider that should provide a ZIP stream to deploy.</param>
        public DeployNetlifySite(Config<string> siteId, Config<string> accessToken, Config<IContentProvider> contentProvider)
            : base(
                new Dictionary<string, IConfig>
                {
                    { SiteId, siteId ?? throw new ArgumentNullException(nameof(siteId)) },
                    { AccessToken, accessToken ?? throw new ArgumentNullException(nameof(accessToken)) },
                    { ContentProvider, contentProvider ?? throw new ArgumentNullException(nameof(contentProvider)) }
                },
                false)
        {
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, IMetadata values)
        {
            string siteId = values.GetString(SiteId) ?? throw new ExecutionException("Invalid site ID");
            string accessToken = values.GetString(AccessToken) ?? throw new ExecutionException("Invalid access token");
            IContentProvider contentProvider = values.Get<IContentProvider>(ContentProvider) ?? throw new Exception("Invalid content provider");

            context.LogDebug($"Starting Netlify deployment to {siteId}...");
            try
            {
                using (HttpClient httpClient = context.CreateHttpClient())
                {
                    NetlifyClient client = new NetlifyClient(accessToken, httpClient);
                    using (Stream zipStream = contentProvider.GetStream())
                    {
                        await client.UpdateSiteAsync(zipStream, siteId, context.CancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                context.LogError($"Exception while deploying to Netlify: {ex.Message}");
                throw;
            }

            return await input.YieldAsync();
        }
    }
}
