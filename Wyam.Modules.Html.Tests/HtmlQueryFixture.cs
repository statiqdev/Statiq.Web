using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common;

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
                .GetOuterHtml("OuterHtml");

            // When
            query.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(2).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("OuterHtml", "<p>This is some Foobar text</p>")
                })));
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("OuterHtml", "<p>This is some other text</p>")
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
                .GetOuterHtml("OuterHtml")
                .First();

            // When
            query.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(1).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("OuterHtml", "<p>This is some Foobar text</p>")
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
                .GetInnerHtml("InnerHtml");

            // When
            query.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(2).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("InnerHtml", "This is some Foobar text")
                })));
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("InnerHtml", "This is some other text")
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
                .GetInnerHtml("InnerHtml")
                .GetOuterHtml("OuterHtml");

            // When
            query.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(2).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("InnerHtml", "This is some Foobar text"),
                    new KeyValuePair<string, object>("OuterHtml", "<p>This is some Foobar text</p>")
                })));
            document.Received().Clone(
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("InnerHtml", "This is some other text"),
                    new KeyValuePair<string, object>("OuterHtml", "<p>This is some other text</p>")
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
                .GetInnerHtml("InnerHtml")
                .GetOuterHtml("OuterHtml");

            // When
            query.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(2).Clone(Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            document.Received().Clone("<p>This is some Foobar text</p>",
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("InnerHtml", "This is some Foobar text"),
                    new KeyValuePair<string, object>("OuterHtml", "<p>This is some Foobar text</p>")
                })));
            document.Received().Clone("<p>This is some other text</p>",
                Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("InnerHtml", "This is some other text"),
                    new KeyValuePair<string, object>("OuterHtml", "<p>This is some other text</p>")
                })));
            stream.Dispose();
        }
    }
}
