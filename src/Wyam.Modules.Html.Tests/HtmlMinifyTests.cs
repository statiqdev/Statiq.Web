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
using Wyam.Common.Pipelines;
using Wyam.Testing;

namespace Wyam.Modules.Html.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class HtmlMinifyTests : BaseFixture
    {
        public class ExecuteMethodTests : HtmlMinifyTests
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
                            <h1>Title</h1>
                            <p>This is<br />some text</p>
                        </body>
                    </html>";
                string output = @"<html><head><title>Title</title></head><body><h1>Title</h1><p>This is<br>some text</p></body></html>";

                IExecutionContext context = Substitute.For<IExecutionContext>();
                IDocument document = Substitute.For<IDocument>();
                document.Content.Returns(input);

                HtmlMinify htmlMinify = new HtmlMinify().RemoveOptionalEndTags(false);

                // When
                htmlMinify.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, output);
            }
        }
    }
}