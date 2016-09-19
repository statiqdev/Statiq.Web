using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Testing;

namespace Wyam.Minification.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class MinifyHtmlFixture : BaseFixture
    {
        public class ExecuteTests : MinifyHtmlFixture
        {
            [Test]
            public void Minify()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Title</title>
                        </head>
                        <body>
                            <!-- FOO -->
                            <h1>Title</h1>
                            <p>This is<br />some text</p>
                        </body>
                    </html>";
                string output = @"<html><head><title>Title</title><body><h1>Title</h1><p>This is<br>some text";

                IExecutionContext context = Substitute.For<IExecutionContext>();
                IDocument document = Substitute.For<IDocument>();
                document.Content.Returns(input);

                MinifyHtml minifyHtml = new MinifyHtml();

                // When
                minifyHtml.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, output);
            }

            [Test]
            public void MinifyWithCustomSettings()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Title</title>
                        </head>
                        <body>
                            <!-- FOO -->
                            <h1>Title</h1>
                            <p>This is<br />some text</p>
                        </body>
                    </html>";
                string output = @"<html><head><title>Title</title></head><body><!-- FOO --><h1>Title</h1><p>This is<br>some text</p></body></html>";

                IExecutionContext context = Substitute.For<IExecutionContext>();
                IDocument document = Substitute.For<IDocument>();
                document.Content.Returns(input);

                MinifyHtml minifyHtml = new MinifyHtml()
                    .WithSettings(settings =>
                    {
                        settings.RemoveOptionalEndTags = false;
                        settings.RemoveHtmlComments = false;
                    });

                // When
                minifyHtml.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, output);
            }
        }
    }
}