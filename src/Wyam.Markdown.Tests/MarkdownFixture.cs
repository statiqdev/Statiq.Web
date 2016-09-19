using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Testing;

namespace Wyam.Markdown.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class MarkdownFixture : BaseFixture
    {
        public class ExecuteTests : MarkdownFixture
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
".Replace(Environment.NewLine, "\n");
                IDocument document = Substitute.For<IDocument>();
                document.Content.Returns(input);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                Markdown markdown = new Markdown();

                // When
                markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, output);
            }

            [Test]
            public void DoesNotRenderSpecialAttributesByDefault()
            {
                // Given
                string input = @"[link](url){#id .class}";
                string output = @"<p><a href=""url"">link</a>{#id .class}</p>
".Replace(Environment.NewLine, "\n");
                IDocument document = Substitute.For<IDocument>();
                document.Content.Returns(input);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                Markdown markdown = new Markdown();

                // When
                markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, output);
            }

            [Test]
            public void DoesRenderSpecialAttributesIfExtensionsActive()
            {
                // Given
                string input = @"[link](url){#id .class}";
                string output = @"<p><a href=""url"" id=""id"" class=""class"">link</a></p>
".Replace(Environment.NewLine, "\n");
                IDocument document = Substitute.For<IDocument>();
                document.Content.Returns(input);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                Markdown markdown = new Markdown().UseExtensions();

                // When
                markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, output);
            }

            [Test]
            public void DoesNotRenderDefinitionListWithoutExtensions()
            {
                // Given
                string input = @"Apple
:   Pomaceous fruit of plants of the genus Malus in 
    the family Rosaceae.";
                string output = @"<p>Apple
:   Pomaceous fruit of plants of the genus Malus in
the family Rosaceae.</p>
".Replace(Environment.NewLine, "\n");
                IDocument document = Substitute.For<IDocument>();
                document.Content.Returns(input);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                Markdown markdown = new Markdown();

                // When
                markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, output);
            }

            [Test]
            public void DoesRenderDefintionListWithSpecificConfiguration()
            {
                // Given
                string input = @"Apple
:   Pomaceous fruit of plants of the genus Malus in 
    the family Rosaceae.";
                string output = @"<dl>
<dt>Apple</dt>
<dd>Pomaceous fruit of plants of the genus Malus in
the family Rosaceae.</dd>
</dl>
".Replace(Environment.NewLine, "\n");
                IDocument document = Substitute.For<IDocument>();
                document.Content.Returns(input);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                Markdown markdown = new Markdown().UseConfiguration("definitionlists");

                // When
                markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, output);
            }

            [Test]
            public void EscapesAtByDefault()
            {
                // Given
                string input = @"Looking @Good, Man!";
                string output = @"<p>Looking &#64;Good, Man!</p>
".Replace(Environment.NewLine, "\n");
                IDocument document = Substitute.For<IDocument>();
                document.Content.Returns(input);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                Markdown markdown = new Markdown();

                // When
                markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, output);
            }

            [Test]
            public void DoesNotEscapeAtIfDisabled()
            {
                // Given
                string input = @"Looking @Good, Man!";
                string output = @"<p>Looking @Good, Man!</p>
".Replace(Environment.NewLine, "\n");
                IDocument document = Substitute.For<IDocument>();
                document.Content.Returns(input);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                Markdown markdown = new Markdown().EscapeAt(false);

                // When
                markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, output);
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
".Replace(Environment.NewLine, "\n");

                IDocument document = Substitute.For<IDocument>();
                document.ContainsKey("meta").Returns(true);
                document.String("meta").Returns(input);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                Markdown markdown = new Markdown("meta");

                // When
                markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(0).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
                context.Received().GetDocument(Arg.Any<IDocument>(), Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new MetadataItems
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
".Replace(Environment.NewLine, "\n");

                IDocument document = Substitute.For<IDocument>();
                document.ContainsKey("meta").Returns(true);
                document.String("meta").Returns(input);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                Markdown markdown = new Markdown("meta", "meta2");

                // When
                markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(0).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
                context.Received().GetDocument(Arg.Any<IDocument>(), Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.SequenceEqual(new MetadataItems
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
                context.Received(0).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received(0).GetDocument(Arg.Any<IDocument>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            }
        }
    }
}