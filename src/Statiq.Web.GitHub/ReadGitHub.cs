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
    public class ReadGitHub : MultiConfigModule
    {
        private const string Request = nameof(Request); // Func<GitHubClient, Task<object>>
        private const string Url = nameof(Url);
        private const string Username = nameof(Username);
        private const string Password = nameof(Password);
        private const string Token = nameof(Token);
        private const string Throttle = nameof(Throttle);

        /// <summary>
        /// Submits a request to the GitHub client.
        /// </summary>
        /// <param name="request">A function with the request to make.</param>
        public ReadGitHub(Func<GitHubClient, Task<object>> request)
            : base(
                new Dictionary<string, IConfig>
                {
                    { Request, Config.FromValue(request) }
                },
                false)
        {
            request.ThrowIfNull(nameof(request));
        }

        /// <summary>
        /// Submits a request to the GitHub client.
        /// </summary>
        /// <param name="request">A function with the request to make.</param>
        public ReadGitHub(Func<IExecutionContext, GitHubClient, Task<object>> request)
            : base(
                new Dictionary<string, IConfig>
                {
                    { Request, Config.FromContext(ctx => (Func<GitHubClient, Task<object>>)(gh => request(ctx, gh))) }
                },
                false)
        {
            request.ThrowIfNull(nameof(request));
        }

        public ReadGitHub(Func<IDocument, IExecutionContext, GitHubClient, Task<object>> request)
            : base(
                new Dictionary<string, IConfig>
                {
                    { Request, Config.FromDocument((doc, ctx) => (Func<GitHubClient, Task<object>>)(gh => request(doc, ctx, gh))) }
                },
                false)
        {
            request.ThrowIfNull(nameof(request));
        }

        /// <summary>
        /// Creates a connection to the GitHub API with basic authenticated access.
        /// </summary>
        /// <param name="username">The username to use.</param>
        /// <param name="password">The password to use.</param>
        /// <returns>The current module instance.</returns>
        public ReadGitHub WithCredentials(Config<string> username, Config<string> password)
        {
            SetConfig(Username, username);
            SetConfig(Password, password);
            return this;
        }

        /// <summary>
        /// Creates a connection to the GitHub API with OAuth authentication.
        /// </summary>
        /// <param name="token">The token to use.</param>
        /// <returns>The current module instance.</returns>
        public ReadGitHub WithCredentials(Config<string> token) => (ReadGitHub)SetConfig(Token, token);

        /// <summary>
        /// Specifies and alternate API URL (such as to an Enterprise GitHub endpoint).
        /// </summary>
        /// <param name="url">The URL to use.</param>
        /// <returns>The current module instance.</returns>
        public ReadGitHub WithUrl(Config<string> url) => (ReadGitHub)SetConfig(Url, url);

        /// <summary>
        /// Specifies whether to use throttling with the GitHub client. If <c>true</c> (the default), client
        /// requests will be retried automatically for abuse or rate limit exceptions.
        /// </summary>
        /// <param name="throttle"><c>true</c> to automatically throttle requests, <c>false</c> otherwise.</param>
        /// <returns>The current module instance.</returns>
        public ReadGitHub WithThrottling(Config<bool> throttle) => (ReadGitHub)SetConfig(Throttle, throttle);

        protected override async Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, IMetadata values)
        {
            // Create the client
            string url = values.GetString(Url);
            GitHubClient github = new GitHubClient(
                new ProductHeaderValue("Statiq"),
                string.IsNullOrEmpty(url) ? GitHubClient.GitHubApiUrl : new Uri(url));
            if (values.TryGetValue(Token, out string token))
            {
                github.Credentials = new Credentials(token);
            }
            else if (values.TryGetValue(Username, out string username) && values.TryGetValue(Password, out string password))
            {
                github.Credentials = new Credentials(username, password);
            }

            // Get the results
            Func<GitHubClient, Task<object>> request = values.Get<Func<GitHubClient, Task<object>>>(Request);
            bool throttle = values.GetBool(Throttle, true);
            object result = throttle
                ? await github.ThrottleAsync(async x => await request(x), context.CancellationToken)
                : await request(github);
            return result is IEnumerable results
                ? results.Cast<object>().ToDocuments()
                : result.ToDocument().Yield();
        }
    }
}
