using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
namespace Wyam.Modules.Opml.Tests
{
    [TestFixture]
    public class OpmlDocFixture
    {

        [Test]
        public async Task SimpleTest()
        {
            var urls = new string[] { "http://hosting.opml.org/dave/spec/subscriptionList.opml",
                "http://hosting.opml.org/dave/spec/states.opml",
                "http://hosting.opml.org/dave/spec/simpleScript.opml",
                "http://hosting.opml.org/dave/spec/placesLived.opml",
                "http://hosting.opml.org/dave/spec/directory.opml"
            };

            var pending = new List<Task<string>>();

            foreach(var x in urls)
            {
                pending.Add(DownloadUrl(x));
            }

            var results = await Task.WhenAll(pending);

            foreach(var opml in results)
            {
                var doc = new OpmlDoc();
                doc.LoadFromXML(opml);

                Assert.IsTrue(doc.Outlines.Any(), "There must be outlines");
            }
        }

        [Test]
        public async Task CountTest()
        {
            var urls = new string[] { "http://hosting.opml.org/dave/spec/subscriptionList.opml"};

            var pending = new List<Task<string>>();

            foreach (var x in urls)
            {
                pending.Add(DownloadUrl(x));
            }

            var results = await Task.WhenAll(pending);

            foreach (var opml in results)
            {
                var doc = new OpmlDoc();
                doc.LoadFromXML(opml);

                Assert.IsTrue(doc.Count() > 0, $"Count must be great instead of {doc.Count()} at {doc.Title}");

                Console.WriteLine($"Count {doc.Count()}");
            }
        }

        [Test]
        public async Task EnumeratorTest()
        {
            var urls = new string[] { "http://hosting.opml.org/dave/spec/placesLived.opml" };

            var pending = new List<Task<string>>();

            foreach (var x in urls)
            {
                pending.Add(DownloadUrl(x));
            }

            var results = await Task.WhenAll(pending);

            foreach (var opml in results)
            {
                var doc = new OpmlDoc();
                doc.LoadFromXML(opml);

                Assert.IsTrue(doc.Count() > 0, $"Count must be great instead of {doc.Count()} at {doc.Title}");

                Assert.IsTrue(doc.Count() == 19, $"Must be 19 instead of {doc.Count()}");

                foreach(var o in doc)
                {
                    Console.WriteLine($"{o.Attributes["text"]}");
                }
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
