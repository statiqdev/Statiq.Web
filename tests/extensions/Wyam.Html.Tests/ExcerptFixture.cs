using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Html.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ExcerptFixture : BaseFixture
    {
        public class ExecuteTests : ExcerptFixture
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
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                Excerpt excerpt = new Excerpt();

                // When
                IEnumerable<IDocument> results = excerpt.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single()["Excerpt"].ShouldBe("<p>This is some Foobar text</p>");
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
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                Excerpt excerpt = new Excerpt("div");

                // When
                IEnumerable<IDocument> results = excerpt.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single()["Excerpt"].ShouldBe("<div>This is some other text</div>");
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
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                Excerpt excerpt = new Excerpt().WithMetadataKey("Baz");

                // When
                IEnumerable<IDocument> results = excerpt.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single()["Baz"].ShouldBe("<p>This is some Foobar text</p>");
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
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                Excerpt excerpt = new Excerpt().WithOuterHtml(false);

                // When
                IEnumerable<IDocument> results = excerpt.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single()["Excerpt"].ShouldBe("This is some Foobar text");
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
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                Excerpt excerpt = new Excerpt("p");

                // When
                IEnumerable<IDocument> results = excerpt.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

                // Then
                results.Single().ShouldBe(document);
            }

            [Test]
            public void SeparatorInsideParagraph()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <!-- excerpt --> Foobar text</p>
                            <p>This is other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                Excerpt excerpt = new Excerpt();

                // When
                IEnumerable<IDocument> results = excerpt.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single()["Excerpt"].ShouldBe("<p>This is some </p>");
            }

            [Test]
            public void SeparatorBetweenParagraphs()
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
                            <!-- excerpt -->
                            <p>This is some more text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                Excerpt excerpt = new Excerpt();

                // When
                IEnumerable<IDocument> results = excerpt.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single()["Excerpt"].ToString().ShouldBe(
                    @"<p>This is some Foobar text</p>
                            <p>This is some other text</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void SeparatorInsideParagraphWithSiblings()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                            <p>This <b>is</b> some <!-- excerpt --><i>other</i> text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                Excerpt excerpt = new Excerpt();

                // When
                IEnumerable<IDocument> results = excerpt.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single()["Excerpt"].ToString().ShouldBe(
                    @"<p>This is some Foobar text</p>
                            <p>This <b>is</b> some </p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void AlternateSeparatorComment()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <!-- foo --> Foobar text</p>
                            <p>This is other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                Excerpt excerpt = new Excerpt().WithSeparators(new[] { "foo" });

                // When
                IEnumerable<IDocument> results = excerpt.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single()["Excerpt"].ShouldBe("<p>This is some </p>");
            }

            [Test]
            public void MultipleSeparatorComments()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <!-- excerpt --> Foobar text</p>
                            <p>This is <!-- excerpt --> other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                Excerpt excerpt = new Excerpt();

                // When
                IEnumerable<IDocument> results = excerpt.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Single()["Excerpt"].ShouldBe("<p>This is some </p>");
            }
        }
    }
}