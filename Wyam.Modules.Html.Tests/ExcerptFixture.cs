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
    public class ExcerptFixture
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
            document.Stream.Returns(stream);
            IEnumerable<KeyValuePair<string, object>> metadata = null;
            document
                .When(x => x.Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                .Do(x => metadata = x.Arg<IEnumerable<KeyValuePair<string, object>>>());
            Excerpt excerpt = new Excerpt();

            // When
            excerpt.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received().Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            CollectionAssert.AreEqual(new [] { new KeyValuePair<string, object>("Excerpt", "<p>This is some Foobar text</p>") }, metadata);
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
            document.Stream.Returns(stream);
            IEnumerable<KeyValuePair<string, object>> metadata = null;
            document
                .When(x => x.Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                .Do(x => metadata = x.Arg<IEnumerable<KeyValuePair<string, object>>>());
            Excerpt excerpt = new Excerpt("div");

            // When
            excerpt.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received().Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            CollectionAssert.AreEqual(new[] { new KeyValuePair<string, object>("Excerpt", "<div>This is some other text</div>") }, metadata);
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
            document.Stream.Returns(stream);
            IEnumerable<KeyValuePair<string, object>> metadata = null;
            document
                .When(x => x.Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                .Do(x => metadata = x.Arg<IEnumerable<KeyValuePair<string, object>>>());
            Excerpt excerpt = new Excerpt().SetMetadataKey("Baz");

            // When
            excerpt.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received().Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            CollectionAssert.AreEqual(new[] { new KeyValuePair<string, object>("Baz", "<p>This is some Foobar text</p>") }, metadata);
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
            document.Stream.Returns(stream);
            IEnumerable<KeyValuePair<string, object>> metadata = null;
            document
                .When(x => x.Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                .Do(x => metadata = x.Arg<IEnumerable<KeyValuePair<string, object>>>());
            Excerpt excerpt = new Excerpt().SetOuterHtml(false);

            // When
            excerpt.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received().Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            CollectionAssert.AreEqual(new[] { new KeyValuePair<string, object>("Excerpt", "This is some Foobar text") }, metadata);
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
            document.Stream.Returns(stream);
            bool cloned = false;
            document
                .When(x => x.Clone(Arg.Any<string>()))
                .Do(x => cloned = true);
            Excerpt excerpt = new Excerpt("p");

            // When
            excerpt.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.DidNotReceive().Clone(Arg.Any<string>());
            Assert.IsFalse(cloned);
            stream.Dispose();
        }
    }
}
