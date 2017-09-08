using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Core.Documents;
using Wyam.Core.Meta;
using Wyam.Core.Modules.IO;
using Wyam.Testing;

namespace Wyam.Core.Tests.Modules.IO
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class DownloadFixture : BaseFixture
    {
        public class ExecuteTests : DownloadFixture
        {
            [Test]
            public void SingleHtmlDownloadGetStream()
            {
                IDocument document = Substitute.For<IDocument>();
                Stream stream = null;
                IEnumerable<KeyValuePair<string, object>> metadata = null;
                FilePath source = null;
                IModule download = new Download().WithUris("https://wyam.io/");
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context
                    .When(x => x.GetDocument(Arg.Any<FilePath>(), Arg.Any<Stream>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>(), Arg.Any<bool>()))
                    .Do(x =>
                    {
                        source = x.Arg<FilePath>();
                        stream = x.Arg<Stream>();
                        metadata = x.Arg<IEnumerable<KeyValuePair<string, object>>>();
                    });

                // When
                download.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then

                Assert.IsNotNull(source, "Source cannot be empty");

                var headers = metadata.FirstOrDefault(x => x.Key == Keys.SourceHeaders).Value as Dictionary<string, string>;

                Assert.IsNotNull(headers, "Header cannot be null");
                Assert.IsTrue(headers.Count > 0, "Headers must contain contents");

                foreach (var h in headers)
                {
                    Assert.IsNotEmpty(h.Key, "Header key cannot be empty");
                    Assert.IsNotEmpty(h.Value, "Header value cannot be empty");
                }

                stream.Seek(0, SeekOrigin.Begin);
                var content = new StreamReader(stream).ReadToEnd();
                stream.Dispose();

                Assert.IsNotEmpty(content, "Download cannot be empty");
            }

            [Test]
            public void MultipleHtmlDownload()
            {
                IDocument document = Substitute.For<IDocument>();

                var output = new List<Tuple<Stream, IEnumerable<KeyValuePair<string, object>>>>();

                IExecutionContext context = Substitute.For<IExecutionContext>();
                context
                    .When(x => x.GetDocument(Arg.Any<Stream>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>(), Arg.Any<bool>()))
                    .Do(x =>
                    {
                        output.Add(Tuple.Create(x.Arg<Stream>(), x.Arg<IEnumerable<KeyValuePair<string, object>>>()));
                    });

                IModule download = new Download().WithUris("https://wyam.io/", "https://github.com/Wyamio/Wyam");

                // When
                download.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                foreach (var o in output)
                {
                    var headers = o.Item2.FirstOrDefault(x => x.Key == Keys.SourceHeaders).Value as Dictionary<string, string>;

                    Assert.IsNotNull(headers, "Header cannot be null");
                    Assert.IsTrue(headers.Count > 0, "Headers must contain contents");

                    foreach (var h in headers)
                    {
                        Assert.IsNotEmpty(h.Key, "Header key cannot be empty");
                        Assert.IsNotEmpty(h.Value, "Header value cannot be empty");
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

                IExecutionContext context = Substitute.For<IExecutionContext>();
                context
                    .When(x => x.GetDocument(Arg.Any<FilePath>(), Arg.Any<Stream>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                    .Do(x =>
                    {
                        stream = x.Arg<Stream>();
                        metadata = x.Arg<IEnumerable<KeyValuePair<string, object>>>();
                    });

                IModule download = new Download().WithUris("https://wyam.io/Content/images/nav-logo.png");

                // When
                download.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                stream.Seek(0, SeekOrigin.Begin);
                Assert.AreNotEqual(-1, stream.ReadByte());
                stream.Dispose();
            }

            [Test]
            public void SingleImageDownloadWithRequestHeader()
            {
                IDocument document = Substitute.For<IDocument>();
                Stream stream = null;
                IEnumerable<KeyValuePair<string, object>> metadata = null;

                IExecutionContext context = Substitute.For<IExecutionContext>();
                context
                    .When(x => x.GetDocument(Arg.Any<FilePath>(), Arg.Any<Stream>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                    .Do(x =>
                    {
                        stream = x.Arg<Stream>();
                        metadata = x.Arg<IEnumerable<KeyValuePair<string, object>>>();
                    });

                var header = new RequestHeaders();
                header.Accept.Add("image/jpeg");

                IModule download = new Download().WithUri("https://wyam.io/Content/images/nav-logo.png", header);

                // When
                download.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                stream.Seek(0, SeekOrigin.Begin);
                Assert.AreNotEqual(-1, stream.ReadByte());
                stream.Dispose();
            }
        }
    }
}
