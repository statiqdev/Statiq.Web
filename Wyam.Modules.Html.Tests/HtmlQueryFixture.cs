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

namespace Wyam.Modules.Html.Tests
{
    [TestFixture]
    public class HtmlQueryFixture
    {
        [Test]
        public void GetOuterHtml()
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
            HtmlQuery query = new HtmlQuery("p")
                .GetOuterHtml("Key");

            // When
            query.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(2).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("Key", "<p>This is some Foobar text</p>")
                })));
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("Key", "<p>This is some other text</p>")
                })));
            stream.Dispose();
        }

        [Test]
        public void GetOuterHtmlWithAttributes()
        {
            // Given
            string input = @"<html>
                    <head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <p foo=""bar"">This is some Foobar text</p>
                        <p foo=""baz"" foo=""bat"" a=""A"">This is some other text</p>
                    </body>
                </html>";
            IDocument document = Substitute.For<IDocument>();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            document.GetStream().Returns(stream);
            HtmlQuery query = new HtmlQuery("p")
                .GetOuterHtml("Key");

            // When
            query.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(2).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("Key", @"<p foo=""bar"">This is some Foobar text</p>")
                })));
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("Key", @"<p foo=""baz"" a=""A"">This is some other text</p>")
                })));
            stream.Dispose();
        }

        [Test]
        public void GetOuterHtmlForFirst()
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
            HtmlQuery query = new HtmlQuery("p")
                .GetOuterHtml("Key")
                .First();

            // When
            query.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(1).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("Key", "<p>This is some Foobar text</p>")
                })));
            stream.Dispose();
        }

        [Test]
        public void GetInnerHtml()
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
            HtmlQuery query = new HtmlQuery("p")
                .GetInnerHtml("Key");

            // When
            query.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(2).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("Key", "This is some Foobar text")
                })));
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("Key", "This is some other text")
                })));
            stream.Dispose();
        }

        [Test]
        public void GetInnerHtmlAndOuterHtml()
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
            HtmlQuery query = new HtmlQuery("p")
                .GetInnerHtml("InnerHtmlKey")
                .GetOuterHtml("OuterHtmlKey");

            // When
            query.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(2).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("InnerHtmlKey", "This is some Foobar text"),
                    new KeyValuePair<string, object>("OuterHtmlKey", "<p>This is some Foobar text</p>")
                })));
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("InnerHtmlKey", "This is some other text"),
                    new KeyValuePair<string, object>("OuterHtmlKey", "<p>This is some other text</p>")
                })));
            stream.Dispose();
        }

        [Test]
        public void SetOuterHtmlContent()
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
            HtmlQuery query = new HtmlQuery("p")
                .SetContent();

            // When
            query.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(2).Clone(Arg.Any<string>());
            document.Received().Clone("<p>This is some Foobar text</p>");
            document.Received().Clone("<p>This is some other text</p>");
            stream.Dispose();
        }

        [Test]
        public void SetInnerHtmlContent()
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
            HtmlQuery query = new HtmlQuery("p")
                .SetContent(false);

            // When
            query.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(2).Clone(Arg.Any<string>());
            document.Received().Clone("This is some Foobar text");
            document.Received().Clone("This is some other text");
            stream.Dispose();
        }

        [Test]
        public void SetOuterHtmlContentWithMetadata()
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
            HtmlQuery query = new HtmlQuery("p")
                .SetContent()
                .GetInnerHtml("InnerHtmlKey")
                .GetOuterHtml("OuterHtmlKey");

            // When
            query.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(2).Clone(Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            document.Received().Clone("<p>This is some Foobar text</p>",
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("InnerHtmlKey", "This is some Foobar text"),
                    new KeyValuePair<string, object>("OuterHtmlKey", "<p>This is some Foobar text</p>")
                })));
            document.Received().Clone("<p>This is some other text</p>",
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("InnerHtmlKey", "This is some other text"),
                    new KeyValuePair<string, object>("OuterHtmlKey", "<p>This is some other text</p>")
                })));
            stream.Dispose();
        }

        [Test]
        public void GetTextContent()
        {
            // Given
            string input = @"<html>
                    <head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <p>This is some <b>Foobar</b> text</p>
                        <p>This is some other text</p>
                    </body>
                </html>";
            IDocument document = Substitute.For<IDocument>();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            document.GetStream().Returns(stream);
            HtmlQuery query = new HtmlQuery("p")
                .GetTextContent("TextContentKey");

            // When
            query.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(2).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("TextContentKey", "This is some Foobar text")
                })));
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("TextContentKey", "This is some other text")
                })));
            stream.Dispose();
        }

        [Test]
        public void GetAttributeValue()
        {
            // Given
            string input = @"<html>
                    <head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <p foo=""bar"">This is some <b>Foobar</b> text</p>
                        <p foo=""baz"">This is some other text</p>
                    </body>
                </html>";
            IDocument document = Substitute.For<IDocument>();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            document.GetStream().Returns(stream);
            HtmlQuery query = new HtmlQuery("p")
                .GetAttributeValue("foo", "Foo");

            // When
            query.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(2).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("Foo", "bar")
                })));
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("Foo", "baz")
                })));
            stream.Dispose();
        }

        [Test]
        public void GetAttributeValueWithImplicitKey()
        {
            // Given
            string input = @"<html>
                    <head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <p foo=""bar"">This is some <b>Foobar</b> text</p>
                        <p foo=""baz"">This is some other text</p>
                    </body>
                </html>";
            IDocument document = Substitute.For<IDocument>();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            document.GetStream().Returns(stream);
            HtmlQuery query = new HtmlQuery("p")
                .GetAttributeValue("foo");

            // When
            query.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(2).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("foo", "bar")
                })));
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("foo", "baz")
                })));
            stream.Dispose();
        }

        [Test]
        public void GetAttributeValueWithMoreThanOneMatch()
        {
            // Given
            string input = @"<html>
                    <head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <p foo=""bar"" foo=""bat"">This is some <b>Foobar</b> text</p>
                        <p foo=""baz"">This is some other text</p>
                    </body>
                </html>";
            IDocument document = Substitute.For<IDocument>();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            document.GetStream().Returns(stream);
            HtmlQuery query = new HtmlQuery("p")
                .GetAttributeValue("foo");

            // When
            query.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(2).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("foo", "bar")
                })));
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("foo", "baz")
                })));
            stream.Dispose();
        }

        [Test]
        public void GetAttributeValues()
        {
            // Given
            string input = @"<html>
                    <head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <p foo=""bar"" foo=""bat"" a=""A"" b=""B"">This is some <b>Foobar</b> text</p>
                        <p foo=""baz"" x=""X"">This is some other text</p>
                    </body>
                </html>";
            IDocument document = Substitute.For<IDocument>();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            document.GetStream().Returns(stream);
            HtmlQuery query = new HtmlQuery("p")
                .GetAttributeValues();

            // When
            query.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(2).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("foo", "bar"),
                    new KeyValuePair<string, object>("a", "A"),
                    new KeyValuePair<string, object>("b", "B")
                })));
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("foo", "baz"),
                    new KeyValuePair<string, object>("x", "X")
                })));
            stream.Dispose();
        }

        [Test]
        public void GetAll()
        {
            // Given
            string input = @"<html>
                    <head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <p foo=""bar"" foo=""bat"" a=""A"" b=""B"">This is some <b>Foobar</b> text</p>
                        <p foo=""baz"" x=""X"">This is some other text</p>
                    </body>
                </html>";
            IDocument document = Substitute.For<IDocument>();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            document.GetStream().Returns(stream);
            HtmlQuery query = new HtmlQuery("p")
                .GetAll();

            // When
            query.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(2).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("OuterHtml", @"<p foo=""bar"" a=""A"" b=""B"">This is some <b>Foobar</b> text</p>"),
                    new KeyValuePair<string, object>("InnerHtml", "This is some <b>Foobar</b> text"),
                    new KeyValuePair<string, object>("TextContent", "This is some Foobar text"),
                    new KeyValuePair<string, object>("foo", "bar"),
                    new KeyValuePair<string, object>("a", "A"),
                    new KeyValuePair<string, object>("b", "B")
                })));
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("OuterHtml", @"<p foo=""baz"" x=""X"">This is some other text</p>"),
                    new KeyValuePair<string, object>("InnerHtml", "This is some other text"),
                    new KeyValuePair<string, object>("TextContent", "This is some other text"),
                    new KeyValuePair<string, object>("foo", "baz"),
                    new KeyValuePair<string, object>("x", "X")
                })));
            stream.Dispose();
        }
    }
}
