using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;
using Statiq.Web.Modules;
using Statiq.Web.Pipelines;

namespace Statiq.Web.Tests.Modules
{
    [TestFixture]
    public class ResolveXrefsFixture : BaseFixture
    {
        public class ExecuteTests : ResolveXrefsFixture
        {
            [Test]
            public async Task ResolvesMatchingXref()
            {
                // Given
                TestDocument target = new TestDocument(new NormalizedPath("/a/b/c.html"), new NormalizedPath("a/b/c.html"))
                {
                    { WebKeys.Xref, "fizzbuzz" }
                };
                TestExecutionContext context = new TestExecutionContext();
                context.Outputs.Dictionary.Add(
                    nameof(Content),
                    new IDocument[] { target }.ToImmutableArray());
                TestDocument document = new TestDocument(
                    @"<html>
                        <body>
                            Foo <a href=""xref:fizzbuzz"">FizzBuzz</a>
                        </body>
                    </html>");
                ResolveXrefs module = new ResolveXrefs();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html><head></head><body>
                            Foo <a href=""/a/b/c.html"">FizzBuzz</a>
                        
                    </body></html>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ReturnsInputDocumentForNoXref()
            {
                // Given
                TestDocument target = new TestDocument(new NormalizedPath("/a/b/c.html"), new NormalizedPath("a/b/c.html"))
                {
                    { WebKeys.Xref, "fizzbuzz" }
                };
                TestExecutionContext context = new TestExecutionContext();
                context.Outputs.Dictionary.Add(
                    nameof(Content),
                    new IDocument[] { target }.ToImmutableArray());
                TestDocument document = new TestDocument(
                    @"<html>
                        <body>
                            Foo
                        </body>
                    </html>");
                ResolveXrefs module = new ResolveXrefs();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.ShouldBe(document);
            }

            [Test]
            public async Task PreservesFragment()
            {
                // Given
                TestDocument target = new TestDocument(new NormalizedPath("/a/b/c.html"), new NormalizedPath("a/b/c.html"))
                {
                    { WebKeys.Xref, "fizzbuzz" }
                };
                TestExecutionContext context = new TestExecutionContext();
                context.Outputs.Dictionary.Add(
                    nameof(Content),
                    new IDocument[] { target }.ToImmutableArray());
                TestDocument document = new TestDocument(
                    @"<html>
                        <body>
                            Foo <a href=""xref:fizzbuzz#foobar"">FizzBuzz</a>
                        </body>
                    </html>");
                ResolveXrefs module = new ResolveXrefs();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html><head></head><body>
                            Foo <a href=""/a/b/c.html#foobar"">FizzBuzz</a>
                        
                    </body></html>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task PreservesQuery()
            {
                // Given
                TestDocument target = new TestDocument(new NormalizedPath("/a/b/c.html"), new NormalizedPath("a/b/c.html"))
                {
                    { WebKeys.Xref, "fizzbuzz" }
                };
                TestExecutionContext context = new TestExecutionContext();
                context.Outputs.Dictionary.Add(
                    nameof(Content),
                    new IDocument[] { target }.ToImmutableArray());
                TestDocument document = new TestDocument(
                    @"<html>
                        <body>
                            Foo <a href=""xref:fizzbuzz?color=blue"">FizzBuzz</a>
                        </body>
                    </html>");
                ResolveXrefs module = new ResolveXrefs();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html><head></head><body>
                            Foo <a href=""/a/b/c.html?color=blue"">FizzBuzz</a>
                        
                    </body></html>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task PreservesQueryAndFragment()
            {
                // Given
                TestDocument target = new TestDocument(new NormalizedPath("/a/b/c.html"), new NormalizedPath("a/b/c.html"))
                {
                    { WebKeys.Xref, "fizzbuzz" }
                };
                TestExecutionContext context = new TestExecutionContext();
                context.Outputs.Dictionary.Add(
                    nameof(Content),
                    new IDocument[] { target }.ToImmutableArray());
                TestDocument document = new TestDocument(
                    @"<html>
                        <body>
                            Foo <a href=""xref:fizzbuzz?color=blue#foobar"">FizzBuzz</a>
                        </body>
                    </html>");
                ResolveXrefs module = new ResolveXrefs();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html><head></head><body>
                            Foo <a href=""/a/b/c.html?color=blue#foobar"">FizzBuzz</a>
                        
                    </body></html>",
                    StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}
