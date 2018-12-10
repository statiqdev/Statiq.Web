using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Html.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class HtmlQueryFixture : BaseFixture
    {
        public class ExecuteTests : HtmlQueryFixture
        {
            [Test]
            public void GetOuterHtml()
            {
                // Given
                const string input = @"<html>
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
                HtmlQuery query = new HtmlQuery("p")
                    .GetOuterHtml("Key");

                // When
                IList<IDocument> results = query.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Select(x => x["Key"].ToString()).ShouldBe(new[]
                {
                    "<p>This is some Foobar text</p>",
                    "<p>This is some other text</p>"
                });
            }

            [Test]
            public void GetOuterHtmlWithAttributes()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p foo=""bar"">This is some Foobar text</p>
                            <p foo=""baz"" foo=""bat"" a=""A"">This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                HtmlQuery query = new HtmlQuery("p")
                    .GetOuterHtml("Key");

                // When
                IList<IDocument> results = query.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Select(x => x["Key"].ToString()).ShouldBe(new[]
                {
                    @"<p foo=""bar"">This is some Foobar text</p>",
                    @"<p foo=""baz"" a=""A"">This is some other text</p>"
                });
            }

            [Test]
            public void GetOuterHtmlForFirst()
            {
                // Given
                const string input = @"<html>
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
                HtmlQuery query = new HtmlQuery("p")
                    .GetOuterHtml("Key")
                    .First();

                // When
                IList<IDocument> results = query.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Select(x => x["Key"].ToString()).ShouldBe(new[]
                {
                    "<p>This is some Foobar text</p>"
                });
            }

            [Test]
            public void GetInnerHtml()
            {
                // Given
                const string input = @"<html>
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
                HtmlQuery query = new HtmlQuery("p")
                    .GetInnerHtml("Key");

                // When
                IList<IDocument> results = query.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Select(x => x["Key"].ToString()).ShouldBe(new[]
                {
                    "This is some Foobar text",
                    "This is some other text"
                });
            }

            [Test]
            public void GetInnerHtmlAndOuterHtml()
            {
                // Given
                const string input = @"<html>
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
                HtmlQuery query = new HtmlQuery("p")
                    .GetInnerHtml("InnerHtmlKey")
                    .GetOuterHtml("OuterHtmlKey");

                // When
                IList<IDocument> results = query.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Select(x => x["InnerHtmlKey"].ToString()).ShouldBe(new[]
                {
                    "This is some Foobar text",
                    "This is some other text"
                });
                results.Select(x => x["OuterHtmlKey"].ToString()).ShouldBe(new[]
                {
                    "<p>This is some Foobar text</p>",
                    "<p>This is some other text</p>"
                });
            }

            [Test]
            public void SetOuterHtmlContent()
            {
                // Given
                const string input = @"<html>
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
                HtmlQuery query = new HtmlQuery("p")
                    .SetContent();

                // When
                List<IDocument> results = query.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Select(x => x.Content).ShouldBe(new[]
                {
                    "<p>This is some Foobar text</p>",
                    "<p>This is some other text</p>"
                });
            }

            [Test]
            public void SetInnerHtmlContent()
            {
                // Given
                const string input = @"<html>
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
                HtmlQuery query = new HtmlQuery("p")
                    .SetContent(false);

                // When
                List<IDocument> results = query.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Select(x => x.Content).ShouldBe(new[]
                {
                    "This is some Foobar text",
                    "This is some other text"
                });
            }

            [Test]
            public void SetOuterHtmlContentWithMetadata()
            {
                // Given
                const string input = @"<html>
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
                HtmlQuery query = new HtmlQuery("p")
                    .SetContent()
                    .GetInnerHtml("InnerHtmlKey")
                    .GetOuterHtml("OuterHtmlKey");

                // When
                List<IDocument> results = query.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Select(x => x.Content).ShouldBe(new[]
                {
                    "<p>This is some Foobar text</p>",
                    "<p>This is some other text</p>"
                });
                results.Select(x => x.String("InnerHtmlKey")).ShouldBe(new[]
                {
                    "This is some Foobar text",
                    "This is some other text"
                });
                results.Select(x => x.String("OuterHtmlKey")).ShouldBe(new[]
                {
                    "<p>This is some Foobar text</p>",
                    "<p>This is some other text</p>"
                });
            }

            [Test]
            public void GetTextContent()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <b>Foobar</b> text</p>
                            <p>This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                HtmlQuery query = new HtmlQuery("p")
                    .GetTextContent("TextContentKey");

                // When
                IList<IDocument> results = query.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Select(x => x.String("TextContentKey")).ShouldBe(new[]
                {
                    "This is some Foobar text",
                    "This is some other text"
                });
            }

            [Test]
            public void GetAttributeValue()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p foo=""bar"">This is some <b>Foobar</b> text</p>
                            <p foo=""baz"">This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                HtmlQuery query = new HtmlQuery("p")
                    .GetAttributeValue("foo", "Foo");

                // When
                IList<IDocument> results = query.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Select(x => x.String("Foo")).ShouldBe(new[]
                {
                    "bar",
                    "baz"
                });
            }

            [Test]
            public void GetAttributeValueWithImplicitKey()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p foo=""bar"">This is some <b>Foobar</b> text</p>
                            <p foo=""baz"">This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                HtmlQuery query = new HtmlQuery("p")
                    .GetAttributeValue("foo");

                // When
                IList<IDocument> results = query.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Select(x => x.String("foo")).ShouldBe(new[]
                {
                    "bar",
                    "baz"
                });
            }

            [Test]
            public void GetAttributeValueWithMoreThanOneMatch()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p foo=""bar"" foo=""bat"">This is some <b>Foobar</b> text</p>
                            <p foo=""baz"">This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                HtmlQuery query = new HtmlQuery("p")
                    .GetAttributeValue("foo");

                // When
                IList<IDocument> results = query.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results.Select(x => x.String("foo")).ShouldBe(new[]
                {
                    "bar",
                    "baz"
                });
            }

            [Test]
            public void GetAttributeValues()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p foo=""bar"" foo=""bat"" a=""A"" b=""B"">This is some <b>Foobar</b> text</p>
                            <p foo=""baz"" x=""X"">This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                HtmlQuery query = new HtmlQuery("p")
                    .GetAttributeValues();

                // When
                IList<IDocument> results = query.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results
                    .Select(x => x.OrderBy(y => y.Key, StringComparer.OrdinalIgnoreCase))
                    .Cast<IEnumerable<KeyValuePair<string, object>>>()
                    .ShouldBe(new[]
                    {
                        new List<KeyValuePair<string, object>>
                        {
                            new KeyValuePair<string, object>("a", "A"),
                            new KeyValuePair<string, object>("b", "B"),
                            new KeyValuePair<string, object>("foo", "bar")
                        },
                        new List<KeyValuePair<string, object>>
                        {
                            new KeyValuePair<string, object>("foo", "baz"),
                            new KeyValuePair<string, object>("x", "X")
                        }
                    });
            }

            [Test]
            public void GetAll()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p foo=""bar"" foo=""bat"" a=""A"" b=""B"">This is some <b>Foobar</b> text</p>
                            <p foo=""baz"" x=""X"">This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                HtmlQuery query = new HtmlQuery("p")
                    .GetAll();

                // When
                IList<IDocument> results = query.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                results
                    .Select(x => x.OrderBy(y => y.Key, StringComparer.OrdinalIgnoreCase))
                    .Cast<IEnumerable<KeyValuePair<string, object>>>()
                    .ShouldBe(new[]
                    {
                        new List<KeyValuePair<string, object>>
                        {
                            new KeyValuePair<string, object>("a", "A"),
                            new KeyValuePair<string, object>("b", "B"),
                            new KeyValuePair<string, object>("foo", "bar"),
                            new KeyValuePair<string, object>("InnerHtml", "This is some <b>Foobar</b> text"),
                            new KeyValuePair<string, object>("OuterHtml", @"<p foo=""bar"" a=""A"" b=""B"">This is some <b>Foobar</b> text</p>"),
                            new KeyValuePair<string, object>("TextContent", "This is some Foobar text")
                        },
                        new List<KeyValuePair<string, object>>
                        {
                            new KeyValuePair<string, object>("foo", "baz"),
                            new KeyValuePair<string, object>("InnerHtml", "This is some other text"),
                            new KeyValuePair<string, object>("OuterHtml", @"<p foo=""baz"" x=""X"">This is some other text</p>"),
                            new KeyValuePair<string, object>("TextContent", "This is some other text"),
                            new KeyValuePair<string, object>("x", "X")
                        }
                    });
            }
        }
    }
}