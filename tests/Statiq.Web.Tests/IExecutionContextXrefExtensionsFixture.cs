using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;
using Statiq.Web.Pipelines;

namespace Statiq.Web.Tests
{
    [TestFixture]
    public class IExecutionContextXrefExtensionsFixture : BaseFixture
    {
        public class TryGetXrefDocumentTests : IExecutionContextXrefExtensionsFixture
        {
            [Test]
            public void FindsXref()
            {
                // Given
                TestDocument a = new TestDocument
                {
                    { WebKeys.Xref, "12" }
                };
                TestDocument b = new TestDocument
                {
                    { WebKeys.Xref, "34" }
                };
                TestDocument c = new TestDocument
                {
                    { WebKeys.Xref, "56" }
                };
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(WebKeys.XrefPipelines, new string[] { nameof(Content) });
                context.Pipelines.Add(nameof(Content)); // Pipeline must exist for xref
                context.Outputs.Dictionary.Add(
                    nameof(Content),
                    new IDocument[] { a, b, c }.ToImmutableArray());

                // When
                bool success = context.TryGetXrefDocument("34", out IDocument result, out string error);

                // Then
                success.ShouldBeTrue();
                result.ShouldBe(b);
                error.ShouldBeNull();
            }

            [Test]
            public void FindsXrefInChildDocuments()
            {
                // Given
                TestDocument a = new TestDocument
                {
                    { WebKeys.Xref, "12" }
                };
                TestDocument child = new TestDocument
                {
                    { WebKeys.Xref, "78" }
                };
                TestDocument b = new TestDocument
                {
                    { WebKeys.Xref, "34" },
                    { Keys.Children, new IDocument[] { child } }
                };
                TestDocument c = new TestDocument
                {
                    { WebKeys.Xref, "56" }
                };
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(WebKeys.XrefPipelines, new string[] { nameof(Content) });
                context.Pipelines.Add(nameof(Content)); // Pipeline must exist for xref
                context.Outputs.Dictionary.Add(
                    nameof(Content),
                    new IDocument[] { a, b, c }.ToImmutableArray());

                // When
                bool success = context.TryGetXrefDocument("78", out IDocument result, out string error);

                // Then
                success.ShouldBeTrue();
                result.ShouldBe(child);
                error.ShouldBeNull();
            }

            [Test]
            public void FindXrefIsCaseInsensitive()
            {
                // Given
                TestDocument a = new TestDocument
                {
                    { WebKeys.Xref, "fooBAR" }
                };
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(WebKeys.XrefPipelines, new string[] { nameof(Content) });
                context.Pipelines.Add(nameof(Content)); // Pipeline must exist for xref
                context.Outputs.Dictionary.Add(
                    nameof(Content),
                    new IDocument[] { a }.ToImmutableArray());

                // When
                bool success = context.TryGetXrefDocument("FOObar", out IDocument result, out string error);

                // Then
                success.ShouldBeTrue();
                result.ShouldBe(a);
                error.ShouldBeNull();
            }

            [Test]
            public void ReturnsFalseForNoMatch()
            {
                // Given
                TestDocument a = new TestDocument
                {
                    { WebKeys.Xref, "fooBAR" }
                };
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(WebKeys.XrefPipelines, new string[] { nameof(Content) });
                context.Pipelines.Add(nameof(Content)); // Pipeline must exist for xref
                context.Outputs.Dictionary.Add(
                    nameof(Content),
                    new IDocument[] { a }.ToImmutableArray());

                // When
                bool success = context.TryGetXrefDocument("fizzbuzz", out IDocument result, out string error);

                // Then
                success.ShouldBeFalse();
                result.ShouldBeNull();
                error.ShouldNotBeNullOrWhiteSpace();
            }

            [Test]
            public void NullXrefDoesNotThrow()
            {
                // Given
                TestDocument a = new TestDocument
                {
                    { WebKeys.Xref, null }
                };
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(WebKeys.XrefPipelines, new string[] { nameof(Content) });
                context.Pipelines.Add(nameof(Content)); // Pipeline must exist for xref
                context.Outputs.Dictionary.Add(
                    nameof(Content),
                    new IDocument[] { a }.ToImmutableArray());

                // When
                bool success = context.TryGetXrefDocument("fizzbuzz", out IDocument result, out string error);

                // Then
                success.ShouldBeFalse();
                result.ShouldBeNull();
                error.ShouldNotBeNullOrWhiteSpace();
            }

            [Test]
            public void ReturnsFalseForAmbiguousMatches()
            {
                // Given
                TestDocument a = new TestDocument
                {
                    { WebKeys.Xref, "12" }
                };
                TestDocument b = new TestDocument
                {
                    { WebKeys.Xref, "34" }
                };
                TestDocument c = new TestDocument
                {
                    { WebKeys.Xref, "12" }
                };
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(WebKeys.XrefPipelines, new string[] { nameof(Content) });
                context.Pipelines.Add(nameof(Content)); // Pipeline must exist for xref
                context.Outputs.Dictionary.Add(
                    nameof(Content),
                    new IDocument[] { a, b, c }.ToImmutableArray());

                // When
                bool success = context.TryGetXrefDocument("12", out IDocument result, out string error);

                // Then
                success.ShouldBeFalse();
                result.ShouldBeNull();
                error.ShouldNotBeNullOrWhiteSpace();
            }
        }

        public class GetXrefDocumentTests : IExecutionContextXrefExtensionsFixture
        {
            [Test]
            public void ThrowsForNoMatch()
            {
                // Given
                TestDocument a = new TestDocument
                {
                    { WebKeys.Xref, "fooBAR" }
                };
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(WebKeys.XrefPipelines, new string[] { nameof(Content) });
                context.Pipelines.Add(nameof(Content)); // Pipeline must exist for xref
                context.Outputs.Dictionary.Add(
                    nameof(Content),
                    new IDocument[] { a }.ToImmutableArray());

                // When, Then
                Should.Throw<ExecutionException>(() => context.GetXrefDocument("fizzbuzz"));
            }
        }

        public class TryGetXrefLinkTests : IExecutionContextXrefExtensionsFixture
        {
            [Test]
            public void ReturnsFalseForNoMatch()
            {
                // Given
                TestDocument a = new TestDocument(new NormalizedPath("/a/b/c.html"), new NormalizedPath("a/b/c.html"))
                {
                    { WebKeys.Xref, "fooBAR" }
                };
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(WebKeys.XrefPipelines, new string[] { nameof(Content) });
                context.Pipelines.Add(nameof(Content)); // Pipeline must exist for xref
                context.Outputs.Dictionary.Add(
                    nameof(Content),
                    new IDocument[] { a }.ToImmutableArray());

                // When
                bool success = context.TryGetXrefLink("fizzbuzz", out string link, out string error);

                // Then
                success.ShouldBeFalse();
                link.ShouldBeNull();
                error.ShouldNotBeNullOrWhiteSpace();
            }

            [Test]
            public void ReturnsFalseForNoLink()
            {
                // Given
                TestDocument a = new TestDocument()
                {
                    { WebKeys.Xref, "fooBAR" }
                };
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(WebKeys.XrefPipelines, new string[] { nameof(Content) });
                context.Pipelines.Add(nameof(Content)); // Pipeline must exist for xref
                context.Outputs.Dictionary.Add(
                    nameof(Content),
                    new IDocument[] { a }.ToImmutableArray());

                // When
                bool success = context.TryGetXrefLink("fizzbuzz", out string link, out string error);

                // Then
                success.ShouldBeFalse();
                link.ShouldBeNull();
                error.ShouldNotBeNullOrWhiteSpace();
            }

            [Test]
            public void ReturnsLink()
            {
                // Given
                TestDocument a = new TestDocument(new NormalizedPath("/a/b/c.html"), new NormalizedPath("a/b/c.html"))
                {
                    { WebKeys.Xref, "fooBAR" }
                };
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(WebKeys.XrefPipelines, new string[] { nameof(Content) });
                context.Pipelines.Add(nameof(Content)); // Pipeline must exist for xref
                context.Outputs.Dictionary.Add(
                    nameof(Content),
                    new IDocument[] { a }.ToImmutableArray());

                // When
                bool success = context.TryGetXrefLink("FOObar", out string link, out string error);

                // Then
                success.ShouldBeTrue();
                link.ShouldBe("/a/b/c.html");
                error.ShouldBeNull();
            }
        }

        public class GetXrefLinkTests : IExecutionContextXrefExtensionsFixture
        {
            [Test]
            public void ThrowsForNoMatch()
            {
                // Given
                TestDocument a = new TestDocument(new NormalizedPath("/a/b/c.html"), new NormalizedPath("a/b/c.html"))
                {
                    { WebKeys.Xref, "fooBAR" }
                };
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(WebKeys.XrefPipelines, new string[] { nameof(Content) });
                context.Pipelines.Add(nameof(Content)); // Pipeline must exist for xref
                context.Outputs.Dictionary.Add(
                    nameof(Content),
                    new IDocument[] { a }.ToImmutableArray());

                // When, Then
                Should.Throw<ExecutionException>(() => context.GetXrefLink("fizzbuzz"));
            }

            [Test]
            public void ThrowsForNoLink()
            {
                // Given
                TestDocument a = new TestDocument()
                {
                    { WebKeys.Xref, "fooBAR" }
                };
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(WebKeys.XrefPipelines, new string[] { nameof(Content) });
                context.Pipelines.Add(nameof(Content)); // Pipeline must exist for xref
                context.Outputs.Dictionary.Add(
                    nameof(Content),
                    new IDocument[] { a }.ToImmutableArray());

                // When, Then
                Should.Throw<ExecutionException>(() => context.GetXrefLink("fizzbuzz"));
            }
        }
    }
}