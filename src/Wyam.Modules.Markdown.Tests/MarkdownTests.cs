using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NSubstitute;
using System.IO;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Pipelines;
using Wyam.Testing;

namespace Wyam.Modules.Markdown.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class MarkdownTests : BaseFixture
    {
        public class ExecuteMethodTests : MarkdownTests
        {
            [Test]
            public void RendersMarkdown()
            {
                // Given
                string input = @"Line 1
*Line 2*
# Line 3";
                string output = @"<p>Line 1
<em>Line 2</em></p>
<h1>Line 3</h1>

";
                IDocument document = Substitute.For<IDocument>();
                document.Content.Returns(input);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                Markdown markdown = new Markdown();

                // When
                markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                document.Received(1).Clone(Arg.Any<string>());
                document.Received().Clone(output);
            }

            [Test]
            public void EscapesAtByDefault()
            {
                // Given
                string input = @"Looking @Good, Man!";
                string output = @"<p>Looking &#64;Good, Man!</p>

";
                IDocument document = Substitute.For<IDocument>();
                document.Content.Returns(input);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                Markdown markdown = new Markdown();

                // When
                markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                document.Received(1).Clone(Arg.Any<string>());
                document.Received().Clone(output);
            }

            [Test]
            public void DoesNotEscapeAtIfDisabled()
            {
                // Given
                string input = @"Looking @Good, Man!";
                string output = @"<p>Looking @Good, Man!</p>

";
                IDocument document = Substitute.For<IDocument>();
                document.Content.Returns(input);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                Markdown markdown = new Markdown().EscapeAt(false);

                // When
                markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                document.Received(1).Clone(Arg.Any<string>());
                document.Received().Clone(output);
            }

            [Test]
            public void RendersMarkdownFromMetadata()
            {
                // Given
                string input = @"Line 1
*Line 2*
# Line 3";
                string output = @"<p>Line 1
<em>Line 2</em></p>
<h1>Line 3</h1>

";

                IDocument document = Substitute.For<IDocument>();
                document.ContainsKey("meta").Returns(true);
                document.String("meta").Returns(input);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                Markdown markdown = new Markdown("meta");

                // When
                markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                document.Received(0).Clone(Arg.Any<string>());
                document.Received(1).Clone(Arg.Any<MetadataItems>());
                document.Received().Clone(Arg.Is<MetadataItems>(x => x.SequenceEqual(new MetadataItems
                {
                    { "meta", output }
                })));
            }

            [Test]
            public void RendersMarkdownFromMetadataToNewKey()
            {
                // Given
                string input = @"Line 1
*Line 2*
# Line 3";
                string output = @"<p>Line 1
<em>Line 2</em></p>
<h1>Line 3</h1>

";

                IDocument document = Substitute.For<IDocument>();
                document.ContainsKey("meta").Returns(true);
                document.String("meta").Returns(input);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                Markdown markdown = new Markdown("meta", "meta2");

                // When
                markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                document.Received(0).Clone(Arg.Any<string>());
                document.Received(1).Clone(Arg.Any<MetadataItems>());
                document.Received().Clone(Arg.Is<MetadataItems>(x => x.SequenceEqual(new MetadataItems
                {
                    { "meta2", output }
                })));
            }

            [Test]
            public void DoesNothingIfMetadataKeyDoesNotExist()
            {
                // Given
                IDocument document = Substitute.For<IDocument>();
                document.ContainsKey("meta").Returns(false);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                Markdown markdown = new Markdown("meta");

                // When
                markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                document.Received(0).Clone(Arg.Any<string>());
                document.Received(0).Clone(Arg.Any<MetadataItems>());
            }
        }
    }
}