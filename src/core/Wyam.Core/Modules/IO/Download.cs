using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
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
    /// <category>Input/Output</category>
    public class Download : IModule
    {
        private readonly List<DownloadInstruction> _urls = new List<DownloadInstruction>();
        private bool _cacheResponse = false;

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
        /// <param name="requestHeader">The request header to use.</param>
        public Download(string uri, RequestHeader requestHeader)
        {
            WithUri(uri, requestHeader);
        }

        /// <summary>
        /// Downloads the specified URIs with a default request header.
        /// </summary>
        /// <param name="uris">The URIs to download.</param>
        public Download WithUris(params string[] uris)
        {
            foreach (string uri in uris)
            {
                _urls.Add(new DownloadInstruction(uri));
            }
            return this;
        }

        /// <summary>
        /// Downloads the specified URI with the specified request header.
        /// </summary>
        /// <param name="uri">The URI to download.</param>
        /// <param name="requestHeader">The request header to use.</param>
        public Download WithUri(string uri, RequestHeader requestHeader = null)
        {
            _urls.Add(new DownloadInstruction(uri, requestHeader));
            return this;
        }

        /// <summary>
        /// Indicates whether the downloaded response should be cached between regenerations.
        /// </summary>
        /// <param name="cacheResponse">If set to <c>true</c>, the response is cached (the default is <c>false</c>).</param>
        public Download CacheResponse(bool cacheResponse)
        {
            _cacheResponse = cacheResponse;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            List<Task<DownloadResult>> tasks = _urls.Select(DownloadUrl).ToList();

            Task.WhenAll(tasks).Wait();

            return tasks.Where(x => !x.IsFaulted).Select(t =>
            {
                string key = t.Result.Uri.ToString();
                IDocument doc;

                if (_cacheResponse && context.ExecutionCache.TryGetValue(key, out doc))
                {
                    return doc;
                }

                DownloadResult result = t.Result;

                string uri = result.Uri.ToString();
                doc = context.GetDocument(new FilePath((Uri)null, uri, PathKind.Absolute), result.Stream, new MetadataItems
                {
                    { Keys.SourceUri, uri },
                    { Keys.SourceHeaders, result.Headers }
                });

                if (_cacheResponse)
                {
                    context.ExecutionCache.Set(key, doc);
                }

                return doc;
            });
        }

        private async Task<DownloadResult> DownloadUrl(DownloadInstruction instruction)
        {
            using (HttpClient client = new HttpClient())
            {
                //prepare request headers
                if (instruction.ContainRequestHeader)
                {
                    ModifyRequestHeader(client.DefaultRequestHeaders, instruction.RequestHeader);
                }

                //Now that we are set and ready, go and do the download call
                using (HttpResponseMessage response = await client.GetAsync(instruction.Uri))
                {
                    using (HttpContent content = response.Content)
                    {
                        Stream result = await content.ReadAsStreamAsync();

                        MemoryStream mem = new MemoryStream();
                        result.CopyTo(mem);

                        Dictionary<string, string> headers = content.Headers.ToDictionary(
                            x => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(x.Key), x => string.Join(",", x.Value));
                        return new DownloadResult(instruction.Uri, mem, headers);
                    }
                }
            }
        }

        private void ModifyRequestHeader(HttpRequestHeaders request, RequestHeader requestHeader)
        {
            foreach (string a in requestHeader.Accept)
            {
                request.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(a));
            }

            foreach (string a in requestHeader.AcceptCharset)
            {
                request.AcceptCharset.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue(a));
            }

            foreach (string a in requestHeader.AcceptEncoding)
            {
                request.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue(a));
            }

            foreach (string a in requestHeader.AcceptLanguage)
            {
                request.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue(a));
            }

            if (requestHeader.BasicAuthorization != null)
            {
                Tuple<string, string> auth = requestHeader.BasicAuthorization;

                request.Authorization =
                new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes($"{auth.Item1}:{auth.Item2}")));
            }

            foreach (string c in requestHeader.Connection)
            {
                request.Connection.Add(c);
            }

            if (requestHeader.Date.HasValue)
            {
                request.Date = requestHeader.Date;
            }

            foreach (string e in requestHeader.Expect)
            {
                request.Expect.Add(new NameValueWithParametersHeaderValue(e));
            }

            if (requestHeader.ExpectContinue.HasValue)
            {
                request.ExpectContinue = requestHeader.ExpectContinue;
            }

            if (!string.IsNullOrWhiteSpace(requestHeader.From))
            {
                request.From = requestHeader.From;
            }

            if (!string.IsNullOrWhiteSpace(requestHeader.Host))
            {
                request.Host = requestHeader.Host;
            }

            foreach (string i in requestHeader.IfMatch)
            {
                request.IfMatch.Add(new EntityTagHeaderValue(i));
            }

            if (requestHeader.IfModifiedSince.HasValue)
            {
                request.IfModifiedSince = requestHeader.IfModifiedSince;
            }

            foreach (string i in requestHeader.IfNoneMatch)
            {
                request.IfNoneMatch.Add(new EntityTagHeaderValue(i));
            }

            if (requestHeader.IfUnmodifiedSince.HasValue)
            {
                request.IfUnmodifiedSince = requestHeader.IfUnmodifiedSince;
            }

            if (requestHeader.MaxForwards.HasValue)
            {
                request.MaxForwards = requestHeader.MaxForwards;
            }

            if (requestHeader.Referrer != null)
            {
                request.Referrer = requestHeader.Referrer;
            }

            foreach (string t in requestHeader.TransferEncoding)
            {
                request.TransferEncoding.Add(new TransferCodingHeaderValue(t));
            }

            if (requestHeader.TransferEncodingChunked.HasValue)
            {
                request.TransferEncodingChunked = requestHeader.TransferEncodingChunked;
            }
        }

        private class DownloadInstruction
        {
            public Uri Uri { get; }
            public RequestHeader RequestHeader { get; }
            public bool ContainRequestHeader => RequestHeader != null;

            public DownloadInstruction(string uri)
            {
                Uri = new Uri(uri);
            }

            public DownloadInstruction(string uri, RequestHeader requestHeader) : this(uri)
            {
                RequestHeader = requestHeader;
            }
        }

        private class DownloadResult
        {
            public Uri Uri { get; }
            public Stream Stream { get; }
            public Dictionary<string, string> Headers { get; }

            public DownloadResult(Uri uri, Stream stream, Dictionary<string, string> headers)
            {
                Uri = uri;
                Stream = stream;
                Headers = headers;
            }
        }
    }
}
