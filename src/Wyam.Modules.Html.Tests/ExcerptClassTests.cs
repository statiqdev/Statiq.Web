using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Testing;

namespace Wyam.Modules.Html.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ExcerptClassTests : BaseFixture
    {
        public class ExecuteMethodTests : ExcerptClassTests
        {
            [Test]
            public void ExcerptFirstParagraph()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                            <p>This is some other text</p>
                        </body>
                    </html>";
                IDocument document = Substitute.For<IDocument>();
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                document.GetStream().Returns(stream);
                Excerpt excerpt = new Excerpt();

                // When
                excerpt.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

                // Then
                document.Received(1).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
                document.Received().Clone(Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new[]
                {
                    new KeyValuePair<string, object>("Excerpt", "<p>This is some Foobar text</p>")
                })));
                stream.Dispose();
            }

            [Test]
            public void ExcerptAlternateQuerySelector()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                            <div>This is some other text</div>
                        </body>
                    </html>";
                IDocument document = Substitute.For<IDocument>();
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                document.GetStream().Returns(stream);
                Excerpt excerpt = new Excerpt("div");

                // When
                excerpt.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

                // Then
                document.Received(1).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
                document.Received().Clone(Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new[]
                {
                    new KeyValuePair<string, object>("Excerpt", "<div>This is some other text</div>")
                })));
                stream.Dispose();
            }

            [Test]
            public void ExcerptAlternateMetadataKey()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                            <p>This is some other text</p>
                        </body>
                    </html>";
                IDocument document = Substitute.For<IDocument>();
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                document.GetStream().Returns(stream);
                Excerpt excerpt = new Excerpt().SetMetadataKey("Baz");

                // When
                excerpt.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

                // Then
                document.Received(1).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
                document.Received().Clone(Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new[]
                {
                    new KeyValuePair<string, object>("Baz", "<p>This is some Foobar text</p>")
                })));
                stream.Dispose();
            }

            [Test]
            public void ExcerptInnerHtml()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                            <p>This is some other text</p>
                        </body>
                    </html>";
                IDocument document = Substitute.For<IDocument>();
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                document.GetStream().Returns(stream);
                Excerpt excerpt = new Excerpt().GetOuterHtml(false);

                // When
                excerpt.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

                // Then
                document.Received(1).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
                document.Received().Clone(Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new[]
                {
                    new KeyValuePair<string, object>("Excerpt", "This is some Foobar text")
                })));
                stream.Dispose();
            }

            [Test]
            public void NoExcerptReturnsSameDocument()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <div>This is some Foobar text</div>
                        </body>
                    </html>";
                IDocument document = Substitute.For<IDocument>();
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                document.GetStream().Returns(stream);
                Excerpt excerpt = new Excerpt("p");

                // When
                excerpt.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

                // Then
                document.DidNotReceiveWithAnyArgs().Clone((string)null);
                stream.Dispose();
            }
        }
    }
}