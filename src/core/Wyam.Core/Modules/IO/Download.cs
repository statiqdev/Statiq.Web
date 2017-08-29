using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;
using Wyam.Core.Documents;
using Wyam.Core.Meta;
using Wyam.Core.Modules.Control;

namespace Wyam.Core.Modules.IO
{
    /// <summary>
    /// Downloads from HTTP and outputs the results as new documents.
    /// </summary>
    /// <remarks>
    /// The original input documents are ignored and are not part of the output
    /// of this module. If you want to retain the original input documents, consider using the
    /// <see cref="ConcatBranch"/> module.
    /// </remarks>
    /// <metadata cref="Keys.SourceUri" usage="Output" />
    /// <metadata cref="Keys.SourceHeaders" usage="Output" />
    /// <category>Input/Output</category>
    public class Download : IModule
    {
        private readonly List<DownloadRequest> _requests = new List<DownloadRequest>();
        private List<DownloadResponse> _cachedResponses;
        private bool _cacheResponses;

        /// <summary>
        /// Downloads the specified URIs with a default request header.
        /// </summary>
        /// <param name="uris">The URIs to download.</param>
        public Download(params string[] uris)
        {
            WithUris(uris);
        }

        /// <summary>
        /// Downloads the specified URI with the specified request header.
        /// </summary>
        /// <param name="uri">The URI to download.</param>
        /// <param name="requestHeaders">The request header to use.</param>
        public Download(string uri, RequestHeaders requestHeaders)
        {
            WithUri(uri, requestHeaders);
        }

        /// <summary>
        /// Downloads the specified URIs with a default request header.
        /// </summary>
        /// <param name="uris">The URIs to download.</param>
        /// <returns>The current module instance.</returns>
        public Download WithUris(params string[] uris)
        {
            foreach (string uri in uris)
            {
                _requests.Add(new DownloadRequest(uri));
            }
            return this;
        }

        /// <summary>
        /// Downloads the specified URI with the specified request header.
        /// </summary>
        /// <param name="uri">The URI to download.</param>
        /// <param name="requestHeaders">The request header to use.</param>
        /// <returns>The current module instance.</returns>
        public Download WithUri(string uri, RequestHeaders requestHeaders = null)
        {
            _requests.Add(new DownloadRequest(uri)
            {
                RequestHeaders = requestHeaders
            });
            return this;
        }

        /// <summary>
        /// Downloads the specified requests.
        /// </summary>
        /// <param name="requests">The requests to download.</param>
        /// <returns>The current module instance.</returns>
        public Download WithRequests(params DownloadRequest[] requests)
        {
            if (requests == null)
            {
                throw new ArgumentNullException(nameof(requests));
            }
            _requests.AddRange(requests.Where(x => x != null));
            return this;
        }

        /// <summary>
        /// Indicates whether the downloaded response should be cached between regenerations.
        /// </summary>
        /// <param name="cacheResponses">If set to <c>true</c>, the response is cached (the default is <c>false</c>).</param>
        /// <returns>The current module instance.</returns>
        public Download CacheResponses(bool cacheResponses = true)
        {
            _cacheResponses = cacheResponses;
            return this;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            List<DownloadResponse> responses = _cachedResponses;
            if (responses == null)
            {
                List<Task<DownloadResponse>> tasks = _requests.Select(GetResponse).ToList();
                Task.WhenAll(tasks).Wait();
                responses = tasks
                    .Where(x => !x.IsFaulted)
                    .Select(t => t.Result)
                    .Where(x => x != null)
                    .ToList();
                if (_cacheResponses)
                {
                    _cachedResponses = responses;
                }
            }
            return responses.AsParallel().Select(r =>
            {
                string uri = r.Uri.ToString();
                IDocument doc = context.GetDocument(
                    new FilePath((Uri) null, uri, PathKind.Absolute),
                    r.Stream,
                    new MetadataItems
                    {
                        { Keys.SourceUri, uri },
                        { Keys.SourceHeaders, r.Headers }
                    });
                return doc;
            });
        }

        private static readonly Dictionary<HttpMethod, Func<HttpClient, Uri, HttpContent, Task<HttpResponseMessage>>> MethodMapping =
            new Dictionary<HttpMethod, Func<HttpClient, Uri, HttpContent, Task<HttpResponseMessage>>>
            {
                { HttpMethod.Get, (client, uri, content) => client.GetAsync(uri) },
                { HttpMethod.Post, (client, uri, content) => client.PostAsync(uri, content) },
                { HttpMethod.Delete, (client, uri, content) => client.DeleteAsync(uri) },
                { HttpMethod.Put, (client, uri, content) => client.PutAsync(uri, content) }
            };

        private async Task<DownloadResponse> GetResponse(DownloadRequest request)
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            if (request.Credentials != null)
            {
                clientHandler.Credentials = request.Credentials;
            }
            using (HttpClient client = new HttpClient(clientHandler))
            {
                // Apply request headers
                request.RequestHeaders?.ApplyTo(client.DefaultRequestHeaders);

                // Apply the query string
                Uri uri = ApplyQueryString(request.Uri, request.QueryString);

                // Now that we are set and ready, go and do the download call
                Func<HttpClient, Uri, HttpContent, Task<HttpResponseMessage>> requestFunc;
                if (!MethodMapping.TryGetValue(request.Method, out requestFunc))
                {
                    Trace.Error($"Invalid download method for {request.Uri}: {request.Method.Method}");
                    return null;
                }
                using (HttpResponseMessage response = await requestFunc(client, uri, request.Content))
                {
                    using (HttpContent content = response.Content)
                    {
                        Stream result = await content.ReadAsStreamAsync();
                        MemoryStream mem = new MemoryStream();
                        result.CopyTo(mem);
                        Dictionary<string, string> headers = content.Headers.ToDictionary(
                            x => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(x.Key), x => string.Join(",", x.Value));
                        return new DownloadResponse(request.Uri, mem, headers);
                    }
                }
            }
        }

        private Uri ApplyQueryString(Uri uri, IDictionary<string, string> queryString)
        {
            UriBuilder builder = new UriBuilder(uri);
            if (queryString.Count > 0)
            {
                string query = builder.Query;
                if (string.IsNullOrEmpty(query))
                {
                    query = "?";
                }
                else
                {
                    query = query + "&";
                }
                query = query
                    + string.Join(
                        "&",
                        queryString.Select(x => string.IsNullOrEmpty(x.Value)
                            ? WebUtility.UrlEncode(x.Key)
                            : $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));
                builder.Query = query;
            }
            return builder.Uri;
        }

        private class DownloadResponse
        {
            public Uri Uri { get; }
            public Stream Stream { get; }
            public Dictionary<string, string> Headers { get; }

            public DownloadResponse(Uri uri, Stream stream, Dictionary<string, string> headers)
            {
                Uri = uri;
                Stream = stream;
                Headers = headers;
            }
        }
    }
}
