using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using System.IO;
using System.Globalization;
using System.Net.Http.Headers;

namespace Wyam.Modules.Download
{
    public class Download : IModule
    {
        List<DownloadInstruction> _urls = new List<DownloadInstruction>();

        bool _isCacheResponse = false;

        public Download Uris(params string[] uris)
        {
            foreach (var u in uris.Select(x => new Uri(x)))
            {
                _urls.Add(new DownloadInstruction(u));
            }

            return this;
        }

        public Download UriWithRequestHeader(string uri, RequestHeader requestHeader)
        {
            if (requestHeader == null)
                throw new ArgumentNullException("requestHeader cannot be null");

            _urls.Add(new DownloadInstruction(new Uri(uri), requestHeader));

            return this;
        }

        public Download CacheResponse(bool isCache)
        {
            _isCacheResponse = isCache;

            return this;
        }

        async Task<DownloadResult> DownloadUrl(DownloadInstruction instruction)
        {
            using (var client = new HttpClient())
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

                        var mem = new MemoryStream();
                        result.CopyTo(mem);

                        var headers = content.Headers.ToDictionary(x => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(x.Key), x => string.Join(",", x.Value));
                        return new DownloadResult(instruction.Uri, mem, headers);
                    }
                }
            }
        }

        void ModifyRequestHeader(HttpRequestHeaders request, RequestHeader requestHeader)
        {
            foreach (var a in requestHeader.Accept)
            {
                request.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(a));
            }

            foreach (var a in requestHeader.AcceptCharset)
            {
                request.AcceptCharset.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue(a));
            }

            foreach (var a in requestHeader.AcceptEncoding)
            {
                request.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue(a));
            }

            foreach (var a in requestHeader.AcceptLanguage)
            {
                request.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue(a));
            }

            if (requestHeader.BasicAuthorization != null)
            {
                var auth = requestHeader.BasicAuthorization;

                request.Authorization =
                new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes($"{auth.Item1}:{auth.Item2}")));
            }

            foreach (var c in requestHeader.Connection)
            {
                request.Connection.Add(c);
            }

            if (requestHeader.Date.HasValue)
            {
                request.Date = requestHeader.Date;
            }

            foreach (var e in requestHeader.Expect)
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

            foreach (var i in requestHeader.IfMatch)
            {
                request.IfMatch.Add(new EntityTagHeaderValue(i));
            }

            if (requestHeader.IfModifiedSince.HasValue)
            {
                request.IfModifiedSince = requestHeader.IfModifiedSince;
            }

            foreach (var i in requestHeader.IfNoneMatch)
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

            foreach (var t in requestHeader.TransferEncoding)
            {
                request.TransferEncoding.Add(new TransferCodingHeaderValue(t));
            }

            if (requestHeader.TransferEncodingChunked.HasValue)
            {
                request.TransferEncodingChunked = requestHeader.TransferEncodingChunked;
            }
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            var tasks = new List<Task<DownloadResult>>();

            foreach (var u in _urls)
            {
                tasks.Add(DownloadUrl(u));
            }

            Task.WhenAll(tasks).Wait();

            return inputs.SelectMany((IDocument input) =>
            {
                return tasks.Where(x => !x.IsFaulted).Select(t =>
                {
                    var key = t.Result.Uri.ToString();
                    IDocument doc;

                    if (_isCacheResponse && context.ExecutionCache.TryGetValue(key, out doc))
                        return doc;

                    var result = t.Result;
                    var stream = result.Stream;
                    stream.Seek(0, SeekOrigin.Begin);

                    var uri = result.Uri.ToString();
                    var metadata = new List<KeyValuePair<string, object>>(){
                        new KeyValuePair<string, object>(MetadataKeys.SourceUri, uri),
                        new KeyValuePair<string, object>(MetadataKeys.SourceHeaders, result.Headers)
                     };

                    doc = input.Clone(uri, stream, metadata);

                    if (_isCacheResponse)
                    {
                        context.ExecutionCache.Set(key, doc);
                    }

                    return doc;
                });
            });
        }
    }
}
