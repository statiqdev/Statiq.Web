using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Documents;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Execution;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Core.Tests.Modules.Control
{
    [TestFixture]
    [NonParallelizable]
    public class FrontMatterFixture : BaseFixture
    {
        public class ExecuteTests : FrontMatterFixture
        {
            [Test]
            public void DefaultCtorSplitsAtDashes()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                FrontMatter frontMatter = new FrontMatter(new Execute((x, ctx) =>
                {
                    frontMatterContent = x.Content;
                    return new[] { x };
                }));

                // When
                IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.AreEqual(
                    @"FM1
FM2
", frontMatterContent);
                Assert.AreEqual(
                    @"Content1
Content2", documents.First().Content);
            }

            [Test]
            public void EmptyFirstLineWithDelimiterTreatsAsFrontMatter()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"
---
FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                FrontMatter frontMatter = new FrontMatter(new Execute((x, ctx) =>
                {
                    frontMatterContent = x.Content;
                    return new[] { x };
                }));

                // When
                IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.AreEqual(
                    @"
", frontMatterContent);
                Assert.AreEqual(
                    @"FM1
FM2
---
Content1
Content2", documents.First().Content);
            }

            [Test]
            public void EmptyFirstLineWithoutDelimiterTreatsAsFrontMatter()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"
FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                FrontMatter frontMatter = new FrontMatter(new Execute((x, ctx) =>
                {
                    frontMatterContent = x.Content;
                    return new[] { x };
                }));

                // When
                IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.AreEqual(
                    @"
FM1
FM2
", frontMatterContent);
                Assert.AreEqual(
                    @"Content1
Content2", documents.First().Content);
            }

            [Test]
            public void DashStringDoesNotSplitAtNonmatchingDashes()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
---
Content1
Content2")
                };
                bool executed = false;
                FrontMatter frontMatter = new FrontMatter("-", new Execute((x, ctx) =>
                {
                    executed = true;
                    return new[] { x };
                }));

                // When
                IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.IsFalse(executed);
                Assert.AreEqual(
                    @"FM1
FM2
---
Content1
Content2", documents.First().Content);
            }

            [Test]
            public void MatchingStringSplitsAtCorrectLocation()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
ABC
Content1
Content2")
                };
                string frontMatterContent = null;
                FrontMatter frontMatter = new FrontMatter("ABC", new Execute((x, ctx) =>
                {
                    frontMatterContent = x.Content;
                    return new[] { x };
                }));

                // When
                IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.AreEqual(
                    @"FM1
FM2
", frontMatterContent);
                Assert.AreEqual(
                    @"Content1
Content2", documents.First().Content);
            }

            [Test]
            public void SingleCharWithRepeatedDelimiterSplitsAtCorrectLocation()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
!!!!
Content1
Content2")
                };
                string frontMatterContent = null;
                FrontMatter frontMatter = new FrontMatter('!', new Execute((x, ctx) =>
                {
                    frontMatterContent = x.Content;
                    return new[] { x };
                }));

                // When
                IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.AreEqual(
                    @"FM1
FM2
", frontMatterContent);
                Assert.AreEqual(
                    @"Content1
Content2", documents.First().Content);
            }

            [Test]
            public void SingleCharWithRepeatedDelimiterWithTrailingSpacesSplitsAtCorrectLocation()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
!!!!  
Content1
Content2")
                };
                string frontMatterContent = null;
                FrontMatter frontMatter = new FrontMatter('!', new Execute((x, ctx) =>
                {
                    frontMatterContent = x.Content;
                    return new[] { x };
                }));

                // When
                IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.AreEqual(
                    @"FM1
FM2
", frontMatterContent);
                Assert.AreEqual(
                    @"Content1
Content2", documents.First().Content);
            }

            [Test]
            public void SingleCharWithRepeatedDelimiterWithLeadingSpacesDoesNotSplit()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
  !!!!
Content1
Content2")
                };
                bool executed = false;
                FrontMatter frontMatter = new FrontMatter('!', new Execute((x, ctx) =>
                {
                    executed = true;
                    return new[] { x };
                }));

                // When
                IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.IsFalse(executed);
                Assert.AreEqual(
                    @"FM1
FM2
  !!!!
Content1
Content2", documents.First().Content);
            }

            [Test]
            public void SingleCharWithRepeatedDelimiterWithExtraLinesSplitsAtCorrectLocation()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                     new TestDocument(@"FM1
FM2

!!!!

Content1
Content2")
                };
                string frontMatterContent = null;
                FrontMatter frontMatter = new FrontMatter('!', new Execute((x, ctx) =>
                {
                    frontMatterContent = x.Content;
                    return new[] { x };
                }));

                // When
                IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.AreEqual(
                    @"FM1
FM2

", frontMatterContent);
                Assert.AreEqual(
                    @"
Content1
Content2", documents.First().Content);
            }

            [Test]
            public void SingleCharWithSingleDelimiterSplitsAtCorrectLocation()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
!
Content1
Content2")
                };
                string frontMatterContent = null;
                FrontMatter frontMatter = new FrontMatter('!', new Execute((x, ctx) =>
                {
                    frontMatterContent = x.Content;
                    return new[] { x };
                }));

                // When
                IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.AreEqual(
                    @"FM1
FM2
", frontMatterContent);
                Assert.AreEqual(
                    @"Content1
Content2", documents.First().Content);
            }

            [Test]
            public void MultipleInputDocumentsResultsInMultipleOutputs()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"AA
-
XX"),
                    new TestDocument(@"BB
-
YY")
                };
                string frontMatterContent = string.Empty;
                FrontMatter frontMatter = new FrontMatter(new Execute((x, ctx) =>
                {
                    frontMatterContent += x.Content;
                    return new[] { x };
                }));

                // When
                IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

                // Then
                Assert.AreEqual(2, documents.Count());
                Assert.AreEqual(
                    @"AA
BB
", frontMatterContent);
                Assert.AreEqual("XX", documents.First().Content);
                Assert.AreEqual("YY", documents.Skip(1).First().Content);
            }

            [Test]
            public void DefaultCtorIgnoresDelimiterOnFirstLine()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"---
FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                FrontMatter frontMatter = new FrontMatter(new Execute((x, ctx) =>
                {
                    frontMatterContent = x.Content;
                    return new[] { x };
                }));

                // When
                IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.AreEqual(
                    @"FM1
FM2
", frontMatterContent);
                Assert.AreEqual(
                    @"Content1
Content2", documents.First().Content);
            }

            [Test]
            public void NoIgnoreDelimiterOnFirstLine()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument[] inputs =
                {
                    new TestDocument(@"---
FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                FrontMatter frontMatter = new FrontMatter(new Execute((x, ctx) =>
                {
                    frontMatterContent = x.Content;
                    return new[] { x };
                })).IgnoreDelimiterOnFirstLine(false);

                // When
                IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

                // Then
                Assert.AreEqual(1, documents.Count());
                Assert.AreEqual("\n", frontMatterContent);
                Assert.AreEqual(
                    @"FM1
FM2
---
Content1
Content2", documents.First().Content);
            }
        }
    }
}
