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
        public void AddsLinkWhenContainerHashChildElements()
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

        // TODO: Test replacement inside child element (such as <b>)
        // TODO: Test no replacement in existing attributes
        // TODO: Test multiple replacements in same element
        // TODO: Test multiple replacements in dfferent elements
        // TODO: Test no replacement in existing link element
    }
}
