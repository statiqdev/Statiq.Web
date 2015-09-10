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
    public class DownloadResult
    {
        public Uri Uri { get; set; }

        public Stream Stream { get; set; }

        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        public DownloadResult()
        {

        }

        public DownloadResult(Uri uri, Stream stream, Dictionary<string, string> headers)
        {
            Uri = uri;
            Stream = stream;
            Headers = headers;
        }
    }

    public class Download : IModule
    {
        List<Uri> _urls = new List<Uri>();

        bool _isCacheResponse = false;

        public Download()
        {

        }

        async Task<DownloadResult> DownloadUrl(Uri url)
        {
            using (var client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(url))
            using (HttpContent content = response.Content)
            {
                Stream result = await content.ReadAsStreamAsync();

                var mem = new MemoryStream();
                result.CopyTo(mem);

                var headers = content.Headers.ToDictionary(x => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(x.Key), x => string.Join(",", x.Value));

                return new DownloadResult(url, mem, headers);
            }
        }

        public Download Uris(params string[] uris)
        {
            foreach (var u in uris)
            {
                _urls.Add(new Uri(u));
            }

            return this;
        }

        public Download CacheResponse(bool isCache)
        {
            _isCacheResponse = isCache;

            return this;
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

                    var metadata = new List<KeyValuePair<string, object>>(){
                        new KeyValuePair<string, object>(MetadataKeys.SourceUri, result.Uri.ToString()),
                        new KeyValuePair<string, object>(MetadataKeys.SourceHeaders, result.Headers)
                     };

                    doc = input.Clone(stream, metadata);

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
