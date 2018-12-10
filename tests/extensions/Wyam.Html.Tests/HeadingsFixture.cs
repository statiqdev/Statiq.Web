using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
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
    public class HeadingsFixture : BaseFixture
    {
        public class ExecuteTests : HeadingsFixture
        {
            [Test]
            public void SetsHeadingContent()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h1>Bar</h1>
                        </body>
                    </html>";
                IDocument document = new TestDocument
                {
                    Content = input
                };
                IExecutionContext context = new TestExecutionContext();
                Headings headings = new Headings();

                // When
                List<IDocument> results = headings.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEqual(
                    new[] { "Foo", "Bar" },
                    results[0].DocumentList(HtmlKeys.Headings).Select(x => x.Content).ToArray());
            }

            [Test]
            public void SetsHeadingMetadata()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h1>Bar</h1>
                        </body>
                    </html>";
                IDocument document = new TestDocument
                {
                    Content = input
                };
                IExecutionContext context = new TestExecutionContext();
                Headings headings = new Headings().WithHeadingKey("HContent");

                // When
                List<IDocument> results = headings.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEqual(
                    new[] { "Foo", "Bar" },
                    results[0].DocumentList(HtmlKeys.Headings).Select(x => x.String("HContent")).ToArray());
            }

            [Test]
            public void DoesNotSetHeadingMetadataIfNull()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h1>Bar</h1>
                        </body>
                    </html>";
                IDocument document = new TestDocument
                {
                    Content = input
                };
                IExecutionContext context = new TestExecutionContext();
                Headings headings = new Headings();

                // When
                List<IDocument> results = headings.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then

                CollectionAssert.AreEqual(
                    new string[] { null, null },
                    results[0].DocumentList(HtmlKeys.Headings).Select(x => x.String("HContent")).ToArray());
            }

            [Test]
            public void OnlyGetsFirstLevelByDefault()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h2>Baz</h2>
                            <h1>Bar</h1>
                        </body>
                    </html>";
                IDocument document = new TestDocument
                {
                    Content = input
                };
                IExecutionContext context = new TestExecutionContext();
                Headings headings = new Headings();

                // When
                List<IDocument> results = headings.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEqual(
                    new[] { "Foo", "Bar" },
                    results[0].DocumentList(HtmlKeys.Headings).Select(x => x.Content).ToArray());
            }

            [Test]
            public void GetsDeeperLevels()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h2>Baz</h2>
                            <h1>Bar</h1>
                        </body>
                    </html>";
                IDocument document = new TestDocument
                {
                    Content = input
                };
                IExecutionContext context = new TestExecutionContext();
                Headings headings = new Headings(3);

                // When
                List<IDocument> results = headings.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEqual(
                    new[] { "Foo", "Baz", "Bar" },
                    results[0].DocumentList(HtmlKeys.Headings).Select(x => x.Content).ToArray());
            }

            [Test]
            public void Nesting()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h2>Baz</h2>
                            <h2>Boz</h2>
                            <h1>Bar</h1>
                            <h2>Boo</h2>
                        </body>
                    </html>";
                IDocument document = new TestDocument
                {
                    Content = input
                };
                IExecutionContext context = new TestExecutionContext();
                Headings headings = new Headings(3).WithNesting();

                // When
                List<IDocument> results = headings.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEqual(
                    new[] { "Foo", "Bar" },
                    results[0].DocumentList(HtmlKeys.Headings).Select(x => x.Content).ToArray());
                CollectionAssert.AreEqual(
                    new[] { "Baz", "Boz" },
                    results[0].DocumentList(HtmlKeys.Headings)[0].DocumentList(Keys.Children).Select(x => x.Content).ToArray());
                CollectionAssert.AreEqual(
                    new[] { "Boo" },
                    results[0].DocumentList(HtmlKeys.Headings)[1].DocumentList(Keys.Children).Select(x => x.Content).ToArray());
            }

            [Test]
            public void SetsChildrenWhenNotNesting()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h2>Baz</h2>
                            <h2>Boz</h2>
                            <h1>Bar</h1>
                            <h2>Boo</h2>
                        </body>
                    </html>";
                IDocument document = new TestDocument
                {
                    Content = input
                };
                IExecutionContext context = new TestExecutionContext();
                Headings headings = new Headings(3);

                // When
                List<IDocument> results = headings.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEqual(
                    new[] { "Foo", "Baz", "Boz", "Bar", "Boo" },
                    results[0].DocumentList(HtmlKeys.Headings).Select(x => x.Content).ToArray());
                CollectionAssert.AreEqual(
                    new[] { "Baz", "Boz" },
                    results[0].DocumentList(HtmlKeys.Headings)[0].DocumentList(Keys.Children).Select(x => x.Content).ToArray());
                CollectionAssert.AreEqual(
                    new[] { "Boo" },
                    results[0].DocumentList(HtmlKeys.Headings)[3].DocumentList(Keys.Children).Select(x => x.Content).ToArray());
            }

            [Test]
            public void SetsHeadingIdAttribute()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h1 id=""bar"">Bar</h1>
                        </body>
                    </html>";
                IDocument document = new TestDocument
                {
                    Content = input
                };
                IExecutionContext context = new TestExecutionContext();
                Headings headings = new Headings();

                // When
                List<IDocument> results = headings.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEqual(
                    new[] { null, "bar" },
                    results[0].DocumentList(HtmlKeys.Headings).Select(x => x.String(HtmlKeys.Id)).ToArray());
            }
        }
    }
}