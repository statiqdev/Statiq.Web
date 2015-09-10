using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using Wyam.Common;
using NUnit.Framework;
using System.IO;

namespace Wyam.Modules.Download.Tests
{
    [TestFixture]
    public class DownloadFixture
    {
        [Test]
        public void SingleDownload()
        {
            IDocument document = Substitute.For<IDocument>();
            Stream stream = null;
            IEnumerable<KeyValuePair<string, object>> metadata = null;

            document
                .When(x => x.Clone(Arg.Any<Stream>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>(), Arg.Any<bool>()))
                .Do(x => {
                    stream = x.Arg<Stream>();
                    metadata = x.Arg<IEnumerable<KeyValuePair<string, object>>>();
                    }
                );

            IModule download = new Download().Uris("http://www.siwawi.com/");
            IExecutionContext context = Substitute.For<IExecutionContext>();

            // When
            download.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list


            // Then
            var headers = metadata.FirstOrDefault(x => x.Key == MetadataKeys.SourceHeaders).Value as Dictionary<string, string>;

            Assert.IsNotNull(headers, "Header cannot be null");
            Assert.IsTrue(headers.Count > 0, "Headers must contain contents");

            foreach (var h in headers)
            {
                Console.WriteLine($"{h.Key} - {h.Value}");
            }

            stream.Seek(0, SeekOrigin.Begin);
            var content = new StreamReader(stream).ReadToEnd();
            stream.Dispose();

            Assert.IsNotEmpty(content, "Download cannot be empty");
            Console.WriteLine("Content " + content);

        }
    }
}
