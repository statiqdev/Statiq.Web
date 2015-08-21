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
    public class AutoLinkFixture
    {
        [Test]
        public void NoReplacementReturnsSameDocument()
        {
            // Given
            string input = @"<html>
                    <head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <p>This is some Foobar text</p>
                    </body>
                </html>";
            IDocument document = Substitute.For<IDocument>();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            document.Stream.Returns(stream);
            bool cloned = false;
            document
                .When(x => x.Clone(Arg.Any<string>()))
                .Do(x => cloned = true);
            AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
            {
                { "Foobaz", "http://www.google.com" }
            });

            // When
            autoLink.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.DidNotReceive().Clone(Arg.Any<string>());
            Assert.IsFalse(cloned);
            stream.Dispose();
        }

        [Test]
        public void AddsLink()
        {
            // Given
            string input = @"<html>
                    <head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <p>This is some Foobar text</p>
                    </body>
                </html>";
            string output = @"<html><head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <p>This is some <a href=""http://www.google.com"">Foobar</a> text</p>
                    
                </body></html>".Replace("\r\n", "\n");
            IDocument document = Substitute.For<IDocument>();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            document.Stream.Returns(stream);
            string content = null;
            document
                .When(x => x.Clone(Arg.Any<string>()))
                .Do(x => content = x.Arg<string>());;
            AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
            {
                { "Foobar", "http://www.google.com" }
            });

            // When
            autoLink.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received().Clone(Arg.Any<string>());
            Assert.AreEqual(output, content);
            stream.Dispose();
        }

        [Test]
        public void AddsLinkWithAlternateQuerySelector()
        {
            // Given
            string input = @"<html>
                    <head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <baz>This is some Foobar text</baz>
                        <p>This is some Foobar text</p>
                    </body>
                </html>";
            string output = @"<html><head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <baz>This is some <a href=""http://www.google.com"">Foobar</a> text</baz>
                        <p>This is some Foobar text</p>
                    
                </body></html>".Replace("\r\n", "\n");
            IDocument document = Substitute.For<IDocument>();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            document.Stream.Returns(stream);
            string content = null;
            document
                .When(x => x.Clone(Arg.Any<string>()))
                .Do(x => content = x.Arg<string>()); ;
            AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
            {
                { "Foobar", "http://www.google.com" }
            }).SetQuerySelector("baz");

            // When
            autoLink.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received().Clone(Arg.Any<string>());
            Assert.AreEqual(output, content);
            stream.Dispose();
        }

        [Test]
        public void AddsLinkWhenContainerHasChildElements()
        {
            // Given
            string input = @"<html>
                    <head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <p>This <i>is</i> some Foobar <b>text</b></p>
                    </body>
                </html>";
            string output = @"<html><head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <p>This <i>is</i> some <a href=""http://www.google.com"">Foobar</a> <b>text</b></p>
                    
                </body></html>".Replace("\r\n", "\n");
            IDocument document = Substitute.For<IDocument>();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            document.Stream.Returns(stream);
            string content = null;
            document
                .When(x => x.Clone(Arg.Any<string>()))
                .Do(x => content = x.Arg<string>()); ;
            AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
            {
                { "Foobar", "http://www.google.com" }
            });

            // When
            autoLink.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received().Clone(Arg.Any<string>());
            Assert.AreEqual(output, content);
            stream.Dispose();
        }

        [Test]
        public void AddsLinkWhenInsideChildElement()
        {
            // Given
            string input = @"<html>
                    <head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <p>This <i>is</i> some <i>Foobar</i> <b>text</b></p>
                    </body>
                </html>";
            string output = @"<html><head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <p>This <i>is</i> some <i><a href=""http://www.google.com"">Foobar</a></i> <b>text</b></p>
                    
                </body></html>".Replace("\r\n", "\n");
            IDocument document = Substitute.For<IDocument>();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            document.Stream.Returns(stream);
            string content = null;
            document
                .When(x => x.Clone(Arg.Any<string>()))
                .Do(x => content = x.Arg<string>()); ;
            AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
            {
                { "Foobar", "http://www.google.com" }
            });

            // When
            autoLink.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received().Clone(Arg.Any<string>());
            Assert.AreEqual(output, content);
            stream.Dispose();
        }

        [Test]
        public void DoesNotReplaceInAttributes()
        {
            // Given
            string input = @"<html>
                    <head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1 title=""Foobar"">Title</h1>
                        <p attr=""Foobar"">This is some Foobar <b ref=""Foobar"">text</b></p>
                    </body>
                </html>";
            string output = @"<html><head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1 title=""Foobar"">Title</h1>
                        <p attr=""Foobar"">This is some <a href=""http://www.google.com"">Foobar</a> <b ref=""Foobar"">text</b></p>
                    
                </body></html>".Replace("\r\n", "\n");
            IDocument document = Substitute.For<IDocument>();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            document.Stream.Returns(stream);
            string content = null;
            document
                .When(x => x.Clone(Arg.Any<string>()))
                .Do(x => content = x.Arg<string>()); ;
            AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
            {
                { "Foobar", "http://www.google.com" }
            });

            // When
            autoLink.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received().Clone(Arg.Any<string>());
            Assert.AreEqual(output, content);
            stream.Dispose();
        }

        [Test]
        public void AddsMultipleLinksInSameElement()
        {
            // Given
            string input = @"<html>
                    <head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <p>This is some <i>Foobar</i> text Foobaz</p>
                    </body>
                </html>";
            string output = @"<html><head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <p>This is some <i><a href=""http://www.google.com"">Foobar</a></i> text <a href=""http://www.bing.com"">Foobaz</a></p>
                    
                </body></html>".Replace("\r\n", "\n");
            IDocument document = Substitute.For<IDocument>();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            document.Stream.Returns(stream);
            string content = null;
            document
                .When(x => x.Clone(Arg.Any<string>()))
                .Do(x => content = x.Arg<string>()); ;
            AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
            {
                { "Foobar", "http://www.google.com" },
                { "Foobaz", "http://www.bing.com" }
            });

            // When
            autoLink.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received().Clone(Arg.Any<string>());
            Assert.AreEqual(output, content);
            stream.Dispose();
        }

        [Test]
        public void AddsMultipleLinksInDifferentElements()
        {
            // Given
            string input = @"<html>
                    <head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Foobaz</h1>
                        <p>This is some <i>Foobar</i> text Foobaz</p>
                        <p>Another Foobaz paragraph</p>
                    </body>
                </html>";
            string output = @"<html><head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Foobaz</h1>
                        <p>This is some <i><a href=""http://www.google.com"">Foobar</a></i> text <a href=""http://www.bing.com"">Foobaz</a></p>
                        <p>Another <a href=""http://www.bing.com"">Foobaz</a> paragraph</p>
                    
                </body></html>".Replace("\r\n", "\n");
            IDocument document = Substitute.For<IDocument>();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            document.Stream.Returns(stream);
            string content = null;
            document
                .When(x => x.Clone(Arg.Any<string>()))
                .Do(x => content = x.Arg<string>()); ;
            AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
            {
                { "Foobar", "http://www.google.com" },
                { "Foobaz", "http://www.bing.com" }
            });

            // When
            autoLink.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received().Clone(Arg.Any<string>());
            Assert.AreEqual(output, content);
            stream.Dispose();
        }

        [Test]
        public void DoesNotAddLinksInExistingLinkElements()
        {
            // Given
            string input = @"<html>
                    <head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <p>This is some <a href=""http://www.yahoo.com"">Foobar</a> text Foobaz</p>
                    </body>
                </html>";
            string output = @"<html><head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <p>This is some <a href=""http://www.yahoo.com"">Foobar</a> text <a href=""http://www.bing.com"">Foobaz</a></p>
                    
                </body></html>".Replace("\r\n", "\n");
            IDocument document = Substitute.For<IDocument>();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            document.Stream.Returns(stream);
            string content = null;
            document
                .When(x => x.Clone(Arg.Any<string>()))
                .Do(x => content = x.Arg<string>()); ;
            AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
            {
                { "Foobar", "http://www.google.com" },
                { "Foobaz", "http://www.bing.com" }
            });

            // When
            autoLink.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received().Clone(Arg.Any<string>());
            Assert.AreEqual(output, content);
            stream.Dispose();
        }

        [Test]
        public void AddsMultipleLinksWhenFirstIsSubstring()
        {
            // Given
            string input = @"<html>
                    <head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <p>This is some <i>Foobar</i> text Foobaz</p>
                    </body>
                </html>";
            string output = @"<html><head>
                        <title>Foobar</title>
                    </head>
                    <body>
                        <h1>Title</h1>
                        <p>This is some <i><a href=""http://www.google.com"">Foo</a>bar</i> text <a href=""http://www.bing.com"">Foobaz</a></p>
                    
                </body></html>".Replace("\r\n", "\n");
            IDocument document = Substitute.For<IDocument>();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            document.Stream.Returns(stream);
            string content = null;
            document
                .When(x => x.Clone(Arg.Any<string>()))
                .Do(x => content = x.Arg<string>()); ;
            AutoLink autoLink = new AutoLink(new Dictionary<string, string>()
            {
                { "Foo", "http://www.google.com" },
                { "Foobaz", "http://www.bing.com" }
            });

            // When
            autoLink.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received().Clone(Arg.Any<string>());
            Assert.AreEqual(output, content);
            stream.Dispose();
        }
    }
}
