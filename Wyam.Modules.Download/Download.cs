using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using System.IO;
using System.Globalization;

namespace Wyam.Modules.Download
{
    public class RequestHeader
    {
        public List<string> Accept { get; set; } = new List<string>();
    }

    class DownloadInstruction
    {
        public Uri Uri { get; set; }

        public RequestHeader RequestHeader { get; set; }

        public bool ContainRequestHeader => RequestHeader != null;

        public DownloadInstruction()
        {

        }

        public DownloadInstruction(Uri uri)
        {
            Uri = uri;
        }

        public DownloadInstruction(Uri uri, RequestHeader requestHeader) : this(uri)
        {
            RequestHeader = requestHeader;
        }
    }

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
                    //create the necessary request header

                    var requestHeader = instruction.RequestHeader;
                    
                    if (requestHeader.Accept.Any())
                    {
                        foreach(var a in requestHeader.Accept)
                        {
                            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(a));
                        }
                    }
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
