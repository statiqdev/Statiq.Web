using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit;
using NUnit.Framework;
using Wyam.Common;
using NSubstitute;
using System.Net.Http;
using System.IO;

namespace Wyam.Modules.Opml.Tests
{
    [TestFixture]
    public class OpmlRendererFixture
    {
        [Test]
        public async Task SimpleReplacementOutput()
        {
            var opmlDoc = await DownloadUrl("http://hosting.opml.org/dave/spec/placesLived.opml");

            var opml = new OpmlRenderer(level:1);

            IDocument document = Substitute.For<IDocument>();

            IEnumerable<KeyValuePair<string, object>> metadata = null;
            document.Content.Returns(opmlDoc);
            document
                .When(x => x.Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                .Do(x => metadata = x.Arg<IEnumerable<KeyValuePair<string, object>>>());

            var result = opml.Execute(new IDocument[] { document }, null).ToList();

            Assert.Greater(result.Count, 0, "Must contains outlines");
            foreach(var x in result)
            {
                Console.WriteLine(x.Content);
            }
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
    }
}
