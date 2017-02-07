using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;
using Wyam.Common.Util;

namespace Wyam.GitHub
{
    /// <summary>
    /// Outputs metadata for information from GitHub.
    /// </summary>
    /// <remarks>
    /// This modules uses the Octokit library and associated types to submit requests to GitHub. Because of the
    /// large number of different kinds of requests, this module does not attempt to provide a fully abstract wrapper
    /// around the Octokit library. Instead, it simplifies the housekeeping involved in setting up an Octokit client
    /// and requires you to provide functions that fetch whatever data you need. Each request will be sent for each input
    /// document.
    /// </remarks>
    /// <category>Metadata</category>
    public class GitHub : IModule, IAsNewDocuments
    {
        private readonly Credentials _credentials;
        private Uri _url;
        private readonly Dictionary<string, Func<IDocument, IExecutionContext, GitHubClient, object>> _requests 
            = new Dictionary<string, Func<IDocument, IExecutionContext, GitHubClient, object>>();

        /// <summary>
        /// Creates a connection to the GitHub API with basic authenticated access.
        /// </summary>
        /// <param name="username">The username to use.</param>
        /// <param name="password">The password to use.</param>
        public GitHub(string username, string password)
        {
            _credentials = new Credentials(username, password);
        }

        /// <summary>
        /// Creates a connection to the GitHub API with OAuth authentication.
        /// </summary>
        /// <param name="token">The token to use.</param>
        public GitHub(string token)
        {
            _credentials = new Credentials(token);
        }

        /// <summary>
        /// Creates an unauthenticated connection to the GitHub API.
        /// </summary>
        public GitHub()
        {
        }

        /// <summary>
        /// Specifies and alternate API URL (such as to an Enterprise GitHub endpoint).
        /// </summary>
        /// <param name="url">The URL to use.</param>
        public GitHub WithUrl(string url)
        {
            _url = new Uri(url);
            return this;
        }

        /// <summary>
        /// Submits a request to the GitHub client.
        /// </summary>
        /// <param name="key">The metadata key in which to store the return value of the request function.</param>
        /// <param name="request">A function with the request to make.</param>
        public GitHub WithRequest(string key, Func<GitHubClient, object> request)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Argument is null or empty", nameof(key));
            }
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            _requests[key] = (doc, ctx, github) => request(github);
            return this;
        }

        /// <summary>
        /// Submits a request to the GitHub client. This allows you to incorporate data from the execution context in your request.
        /// </summary>
        /// <param name="key">The metadata key in which to store the return value of the request function.</param>
        /// <param name="request">A function with the request to make.</param>
        public GitHub WithRequest(string key, Func<IExecutionContext, GitHubClient, object> request)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Argument is null or empty", nameof(key));
            }
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            _requests[key] = (doc, ctx, github) => request(ctx, github);
            return this;
        }

        /// <summary>
        /// Submits a request to the GitHub client. This allows you to incorporate data from the execution context and current document in your request.
        /// </summary>
        /// <param name="key">The metadata key in which to store the return value of the request function.</param>
        /// <param name="request">A function with the request to make.</param>
        public GitHub WithRequest(string key, Func<IDocument, IExecutionContext, GitHubClient, object> request)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Argument is null or empty", nameof(key));
            }
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            _requests[key] = request;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            GitHubClient github = new GitHubClient(new ProductHeaderValue("Wyam"), _url ?? GitHubClient.GitHubApiUrl);
            if (_credentials != null)
            {
                github.Credentials = _credentials;
            }
            return inputs.AsParallel().Select(context, input =>
            {
                ConcurrentDictionary<string, object> results = new ConcurrentDictionary<string, object>();
                foreach (KeyValuePair<string, Func<IDocument, IExecutionContext, GitHubClient, object>> request in _requests.AsParallel())
                {
                    Trace.Verbose("Submitting {0} GitHub request for {1}", request.Key, input.SourceString());
                    try
                    {
                        results[request.Key] = request.Value(input, context, github);
                    }
                    catch (Exception ex)
                    {
                        Trace.Warning("Exception while submitting {0} GitHub request for {1}: {2}", request.Key, input.SourceString(), ex.ToString());
                    }
                }
                return context.GetDocument(input, results);
            });
        }
    }
}
