using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;
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
                const string input = @"Line 1
*Line 2*
# Line 3";
                const string output = @"<p>Line 1
<em>Line 2</em></p>
<h1>Line 3</h1>
";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown();

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void CanUseExternalExtensionDirectly()
            {
                TestMarkdownExtension extension = new TestMarkdownExtension();
                Markdown markdown = new Markdown().UseExtension(extension);

                // When
                markdown.Execute(new[] { new TestDocument(string.Empty) }, new TestExecutionContext()).ToList();  // Make sure to materialize the result list

                // Then
                extension.ReceivedSetup.ShouldBeTrue();
            }

            [Test]
            public void CanUseExternalExtension()
            {
                const string input = "![Alt text](/path/to/img.jpg)";
                const string output = @"<p><img src=""/path/to/img.jpg"" class=""ui spaced image"" alt=""Alt text"" /></p>
";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Type[] o = { typeof(TestMarkdownExtension) };
                IEnumerable<Type> cast = o as IEnumerable<Type>;
                Markdown markdown = new Markdown().UseExtensions(cast);

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void CanUseMultipleExternalExtensions()
            {
                const string input = "![Alt text](/path/to/img.jpg)";
                const string output = @"<p><img src=""/path/to/img.jpg"" class=""ui spaced image second"" alt=""Alt text"" /></p>
";

                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Type[] o =
                {
                    typeof(TestMarkdownExtension),
                    typeof(AlternateTestMarkdownExtension)
                };
                IEnumerable<Type> cast = o;
                Markdown markdown = new Markdown().UseExtensions(cast);

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void DoesNotRenderSpecialAttributesByDefault()
            {
                // Given
                const string input = "[link](url){#id .class}";
                const string output = @"<p><a href=""url"">link</a>{#id .class}</p>
";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown();

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void DoesRenderSpecialAttributesIfExtensionsActive()
            {
                // Given
                const string input = "[link](url){#id .class}";
                const string output = @"<p><a href=""url"" id=""id"" class=""class"">link</a></p>
";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown().UseExtensions();

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void DoesNotRenderDefinitionListWithoutExtensions()
            {
                // Given
                const string input = @"Apple
:   Pomaceous fruit of plants of the genus Malus in 
    the family Rosaceae.";
                const string output = @"<p>Apple
:   Pomaceous fruit of plants of the genus Malus in
the family Rosaceae.</p>
";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown();

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void DoesRenderDefintionListWithSpecificConfiguration()
            {
                // Given
                const string input = @"Apple
:   Pomaceous fruit of plants of the genus Malus in 
    the family Rosaceae.";
                const string output = @"<dl>
<dt>Apple</dt>
<dd>Pomaceous fruit of plants of the genus Malus in
the family Rosaceae.</dd>
</dl>
";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown().UseConfiguration("definitionlists");

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void EscapesAtByDefault()
            {
                // Given
                const string input = "Looking @Good, Man!";
                const string output = @"<p>Looking &#64;Good, Man!</p>
";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown();

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void UnescapesDoubleAt()
            {
                // Given
                const string input = @"Looking @Good, \\@Man!";
                const string output = @"<p>Looking &#64;Good, @Man!</p>
";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown();

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void DoesNotEscapeAtIfDisabled()
            {
                // Given
                const string input = "Looking @Good, Man!";
                const string output = @"<p>Looking @Good, Man!</p>
";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown().EscapeAt(false);

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void RendersMarkdownFromMetadata()
            {
                // Given
                const string input = @"Line 1
*Line 2*
# Line 3";
                const string output = @"<p>Line 1
<em>Line 2</em></p>
<h1>Line 3</h1>
";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "meta", input }
                });
                Markdown markdown = new Markdown("meta");

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().String("meta").ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void RendersMarkdownFromMetadataToNewKey()
            {
                // Given
                const string input = @"Line 1
*Line 2*
# Line 3";
                const string output = @"<p>Line 1
<em>Line 2</em></p>
<h1>Line 3</h1>
";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "meta", input }
                });
                Markdown markdown = new Markdown("meta", "meta2");

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().String("meta2").ShouldBe(output, StringCompareShould.IgnoreLineEndings);
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
                results.ShouldBe(new[] { document });
            }

            [Test]
            public void UsePrependLinkRootSetting()
            {
                // Given
                const string input = "This is a [link](/link.html)";
                string output = @"<p>This is a <a href=""/virtual-dir/link.html"">link</a></p>" + Environment.NewLine;
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.LinkRoot] = "/virtual-dir";
                TestDocument document = new TestDocument(input);
                Markdown markdown = new Markdown().PrependLinkRoot(true);

                // When
                IList<IDocument> results = markdown.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}