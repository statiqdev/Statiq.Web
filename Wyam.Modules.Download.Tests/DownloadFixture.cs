using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using Wyam.Common;
using NUnit.Framework;
using System.IO;
using System.Reflection;

namespace Wyam.Modules.Download.Tests
{
    [TestFixture]
    public class DownloadFixture
    {
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static byte[] ReadToByte(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        [Test]
        public void SingleHtmlDownloadGetStream()
        {
            IDocument document = Substitute.For<IDocument>();
            Stream stream = null;
            IEnumerable<KeyValuePair<string, object>> metadata = null;
            string source = null;
            document
                .When(x => x.Clone(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>(), Arg.Any<bool>()))
                .Do(x =>
                {
                    source = x.Arg<string>();
                    stream = x.Arg<Stream>();
                    metadata = x.Arg<IEnumerable<KeyValuePair<string, object>>>();
                });

            IModule download = new Download().Uris("http://www.siwawi.com/");
            IExecutionContext context = Substitute.For<IExecutionContext>();

            // When
            download.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then

            Assert.IsNotEmpty(source, "Source cannot be empty");
            Console.WriteLine("Source " + source);

            var headers = metadata.FirstOrDefault(x => x.Key == MetadataKeys.SourceHeaders).Value as Dictionary<string, string>;

            Assert.IsNotNull(headers, "Header cannot be null");
            Assert.IsTrue(headers.Count > 0, "Headers must contain contents");

            foreach (var h in headers)
            {
                Assert.IsNotEmpty(h.Key, "Header key cannot be empty");
                Assert.IsNotEmpty(h.Value, "Header value cannot be empty");
                Console.WriteLine($"{h.Key} - {h.Value}");
            }

            stream.Seek(0, SeekOrigin.Begin);
            var content = new StreamReader(stream).ReadToEnd();
            stream.Dispose();

            Assert.IsNotEmpty(content, "Download cannot be empty");
            Console.WriteLine("Content " + content);
        }

        [Test]
        public void MultipleHtmlDownload()
        {
            IDocument document = Substitute.For<IDocument>();

            var output = new List<Tuple<Stream, IEnumerable<KeyValuePair<string, object>>>>();

            IEnumerable<KeyValuePair<string, object>> metadata = null;

            document
                .When(x => x.Clone(Arg.Any<Stream>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>(), Arg.Any<bool>()))
                .Do(x =>
                {
                    output.Add(Tuple.Create(x.Arg<Stream>(), x.Arg<IEnumerable<KeyValuePair<string, object>>>()));
                });

            IModule download = new Download().Uris("http://www.siwawi.com/", "http://stackoverflow.com/questions/221925/creating-a-byte-array-from-a-stream");
            IExecutionContext context = Substitute.For<IExecutionContext>();

            // When
            download.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            Console.WriteLine("Length " + output.Count);
            // Then
            foreach(var o in output)
            {
                var headers = o.Item2.FirstOrDefault(x => x.Key == MetadataKeys.SourceHeaders).Value as Dictionary<string, string>;

                Assert.IsNotNull(headers, "Header cannot be null");
                Assert.IsTrue(headers.Count > 0, "Headers must contain contents");

                foreach (var h in headers)
                {
                    Assert.IsNotEmpty(h.Key, "Header key cannot be empty");
                    Assert.IsNotEmpty(h.Value, "Header value cannot be empty");
                    Console.WriteLine($"{h.Key} - {h.Value}");
                }

                o.Item1.Seek(0, SeekOrigin.Begin);
                var content = new StreamReader(o.Item1).ReadToEnd();
                o.Item1.Dispose();

                Assert.IsNotEmpty(content, "Download cannot be empty");
            }
        }


        [Test]
        public void SingleImageDownload()
        {
            IDocument document = Substitute.For<IDocument>();
            Stream stream = null;
            IEnumerable<KeyValuePair<string, object>> metadata = null;

            document
                .When(x => x.Clone(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                .Do(x =>
                {
                    stream = x.Arg<Stream>();
                    metadata = x.Arg<IEnumerable<KeyValuePair<string, object>>>();
                });

            IModule download = new Download().Uris("http://siwawi.com/images/cover/617215_113386155490459_1547184305_o-cover.jpg");
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.OutputFolder.Returns(x => AssemblyDirectory);

            // When
            download.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            stream.Seek(0, SeekOrigin.Begin);

            var path = Path.Combine(context.OutputFolder, "test.jpg");
            File.WriteAllBytes(path, ReadToByte(stream));
            stream.Dispose();

            Assert.IsTrue(File.Exists(path), "Download cannot be empty");
        }

        [Test]
        public void SingleImageDownloadWithRequestHeader()
        {
            IDocument document = Substitute.For<IDocument>();
            Stream stream = null;
            IEnumerable<KeyValuePair<string, object>> metadata = null;

            document
                .When(x => x.Clone(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                .Do(x =>
                {
                    stream = x.Arg<Stream>();
                    metadata = x.Arg<IEnumerable<KeyValuePair<string, object>>>();
                });

            var header = new RequestHeader();
            header.Accept.Add("image/jpeg");

            IModule download = new Download().UriWithRequestHeader("http://siwawi.com/images/cover/617215_113386155490459_1547184305_o-cover.jpg", header);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.OutputFolder.Returns(x => AssemblyDirectory);

            // When
            download.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            stream.Seek(0, SeekOrigin.Begin);

            var path = Path.Combine(context.OutputFolder, "test-with-request-header.jpg");
            File.WriteAllBytes(path, ReadToByte(stream));
            stream.Dispose();

            Assert.IsTrue(File.Exists(path), "Download cannot be empty");
        }
    }
}
