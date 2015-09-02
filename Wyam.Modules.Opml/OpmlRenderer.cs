using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;

namespace Wyam.Modules.Opml
{
    public class OpmlRenderer : IModule
    {
        OpmlDoc _doc = new OpmlDoc();

        public async Task<OpmlRenderer> Download(string url)
        {
            var outline = await DownloadUrl(url);

            _doc.LoadFromXML(outline);

            return this;
        }

        async Task<string> DownloadUrl(string url)
        {
            using (var client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(url))
            using (HttpContent content = response.Content)
            {
                string result = await content.ReadAsStringAsync();
                return result;
            }
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {

            return inputs.ToList();
        }
    }
}
