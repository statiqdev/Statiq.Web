using System;
using System.Collections.Generic;
using System.Linq;

using Markdig;

using NSubstitute;

using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

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
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown();

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output }));
            }

            [Test]
            public void CanUseExternalExtensionDirectly()
            {
                IMarkdownExtension mockExtension = Substitute.For<IMarkdownExtension>();
                Markdown markdown = new Markdown().UseExtension(mockExtension);

                // When
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                markdown.Execute(new[] { new TestDocument(string.Empty) }, new TestExecutionContext()).ToList();  // Make sure to materialize the result list

                // Then
                // Setup will always be called during markdown pipeline setup.
                mockExtension.Received().Setup(Arg.Any<MarkdownPipelineBuilder>());
            }

            [Test]
            public void CanUseExternalExtension()
            {
                string input = @"![Alt text](/path/to/img.jpg)";
                string output = @"<p><img src=""/path/to/img.jpg"" class=""ui spaced image"" alt=""Alt text"" /></p>
".Replace(Environment.NewLine, "\n");
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Type[] o = { typeof(ExternalMarkdownExtension) };
                IEnumerable<Type> cast = o as IEnumerable<Type>;
                Markdown markdown = new Markdown().UseExtensions(cast);

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output }));
            }

            [Test]
            public void CanUseMultipleExternalExtensions()
            {
                const string input = @"![Alt text](/path/to/img.jpg)";
                const string output = @"<p><img src=""/path/to/img.jpg"" class=""ui spaced image second"" alt=""Alt text"" /></p>";

                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Type[] o =
                {
                    typeof(ExternalMarkdownExtension),
                    typeof(SecondExternalMarkdownExtension)
                };
                IEnumerable<Type> cast = o;
                Markdown markdown = new Markdown().UseExtensions(cast);

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();

                // Then
                Assert.That(results.Select(x => x.Content.Trim()), Is.EquivalentTo(new[] { output }));
            }

            [Test]
            public void DoesNotRenderSpecialAttributesByDefault()
            {
                // Given
                string input = @"[link](url){#id .class}";
                string output = @"<p><a href=""url"">link</a>{#id .class}</p>
".Replace(Environment.NewLine, "\n");
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown();

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output }));
            }

            [Test]
            public void DoesRenderSpecialAttributesIfExtensionsActive()
            {
                // Given
                string input = @"[link](url){#id .class}";
                string output = @"<p><a href=""url"" id=""id"" class=""class"">link</a></p>
".Replace(Environment.NewLine, "\n");
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown().UseExtensions();

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output }));
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
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown();

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output }));
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
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown().UseConfiguration("definitionlists");

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output }));
            }

            [Test]
            public void EscapesAtByDefault()
            {
                // Given
                string input = @"Looking @Good, Man!";
                string output = @"<p>Looking &#64;Good, Man!</p>
".Replace(Environment.NewLine, "\n");
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown();

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output }));
            }

            [Test]
            public void UnescapesDoubleAt()
            {
                // Given
                string input = @"Looking @Good, \\@Man!";
                string output = @"<p>Looking &#64;Good, @Man!</p>
".Replace(Environment.NewLine, "\n");
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown();

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output }));
            }

            [Test]
            public void DoesNotEscapeAtIfDisabled()
            {
                // Given
                string input = @"Looking @Good, Man!";
                string output = @"<p>Looking @Good, Man!</p>
".Replace(Environment.NewLine, "\n");
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown().EscapeAt(false);

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output }));
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

                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "meta", input }
                });
                Markdown markdown = new Markdown("meta");

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.String("meta")), Is.EquivalentTo(new[] { output }));
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

                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "meta", input }
                });
                Markdown markdown = new Markdown("meta", "meta2");

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.String("meta2")), Is.EquivalentTo(new[] { output }));
            }

            [Test]
            public void DoesNothingIfMetadataKeyDoesNotExist()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                Markdown markdown = new Markdown("meta");

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results, Is.EquivalentTo(new[] { document }));
            }
        }
    }
}