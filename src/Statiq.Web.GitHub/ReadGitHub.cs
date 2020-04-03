using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using Statiq.Common;

namespace Statiq.Web.GitHub
{
    /// <summary>
    /// Reads information from GitHub.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This modules uses the Octokit library and associated types to submit requests to GitHub. Because of the
    /// large number of different kinds of requests, this module does not attempt to provide a fully abstract wrapper
    /// around the Octokit library. Instead, it simplifies the housekeeping involved in setting up an Octokit client
    /// and requires you to provide functions that fetch whatever data you need.
    /// </para>
    /// <para>
    /// This module outputs entirely new documents that are representations of the Octokit result object for the
    /// request. If the request delegate requires a document, outputs are generated for each input document
    /// and concatenated. If the request delegate does not require a document, one set of outputs are generated
    /// for the specified request.
    /// </para>
    /// </remarks>
    /// <category>Metadata</category>
    public class ReadGitHub : ConfigModule<Func<GitHubClient, Task<object>>>
    {
        private Credentials _credentials;
        private Uri _url;

        /// <summary>
        /// Submits a request to the GitHub client.
        /// </summary>
        /// <param name="request">A function with the request to make.</param>
        public ReadGitHub(Func<GitHubClient, Task<object>> request)
            : base(request, false)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));
        }

        /// <summary>
        /// Submits a request to the GitHub client.
        /// </summary>
        /// <param name="request">A function with the request to make.</param>
        public ReadGitHub(Func<IExecutionContext, GitHubClient, Task<object>> request)
            : base(Config.FromContext(ctx => (Func<GitHubClient, Task<object>>)(gh => request(ctx, gh))), false)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));
        }

        public ReadGitHub(Func<IDocument, IExecutionContext, GitHubClient, Task<object>> request)
            : base(Config.FromDocument((doc, ctx) => (Func<GitHubClient, Task<object>>)(gh => request(doc, ctx, gh))), false)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));
        }

        /// <summary>
        /// Creates a connection to the GitHub API with basic authenticated access.
        /// </summary>
        /// <param name="username">The username to use.</param>
        /// <param name="password">The password to use.</param>
        /// <returns>The current module instance.</returns>
        public ReadGitHub WithCredentials(string username, string password)
        {
            _credentials = new Credentials(username, password);
            return this;
        }

        /// <summary>
        /// Creates a connection to the GitHub API with OAuth authentication.
        /// </summary>
        /// <param name="token">The token to use.</param>
        /// <returns>The current module instance.</returns>
        public ReadGitHub WithCredentials(string token)
        {
            _credentials = new Credentials(token);
            return this;
        }

        /// <summary>
        /// Specifies and alternate API URL (such as to an Enterprise GitHub endpoint).
        /// </summary>
        /// <param name="url">The URL to use.</param>
        /// <returns>The current module instance.</returns>
        public ReadGitHub WithUrl(string url)
        {
            _url = new Uri(url);
            return this;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteConfigAsync(
            IDocument input,
            IExecutionContext context,
            Func<GitHubClient, Task<object>> value)
        {
            GitHubClient github = new GitHubClient(new ProductHeaderValue("Statiq"), _url ?? GitHubClient.GitHubApiUrl);
            if (_credentials != null)
            {
                github.Credentials = _credentials;
            }
            object result = await value(github);
            return result is IEnumerable results
                ? results.Cast<object>().Where(x => x != null).Select(x => x.ToDocument())
                : result.ToDocument().Yield();
        }
    }
}
