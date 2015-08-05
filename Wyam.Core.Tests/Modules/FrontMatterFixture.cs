using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Core.Modules;
using Wyam.Abstractions;

namespace Wyam.Core.Tests.Modules
{
    [TestFixture]
    public class FrontMatterFixture
    {
        [Test]
        public void DefaultCtorSplitsAtDashes()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            IDocument[] inputs = { new Document(metadata, pipeline).Clone(@"FM1
FM2
---
Content1
Content2") };
            string frontMatterContent = null;
            FrontMatter frontMatter = new FrontMatter(new Execute(x =>
            {
                frontMatterContent = x.Content;
                return new [] {x};
            }));

            // When
            IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

            // Then
            Assert.AreEqual(1, documents.Count());
            Assert.AreEqual(@"FM1
FM2
", frontMatterContent);
            Assert.AreEqual(@"Content1
Content2", documents.First().Content);
        }

        [Test]
        public void DashStringDoesNotSplitAtNonmatchingDashes()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            IDocument[] inputs = { new Document(metadata, pipeline).Clone(@"FM1
FM2
---
Content1
Content2") };
            bool executed = false;
            FrontMatter frontMatter = new FrontMatter("-", new Execute(x =>
            {
                executed = true;
                return new[] { x };
            }));

            // When
            IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

            // Then
            Assert.AreEqual(1, documents.Count());
            Assert.IsFalse(executed);
            Assert.AreEqual(@"FM1
FM2
---
Content1
Content2", documents.First().Content);
        }

        [Test]
        public void MatchingStringSplitsAtCorrectLocation()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            IDocument[] inputs = { new Document(metadata, pipeline).Clone(@"FM1
FM2
ABC
Content1
Content2") };
            string frontMatterContent = null;
            FrontMatter frontMatter = new FrontMatter("ABC", new Execute(x =>
            {
                frontMatterContent = x.Content;
                return new[] { x };
            }));

            // When
            IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

            // Then
            Assert.AreEqual(1, documents.Count());
            Assert.AreEqual(@"FM1
FM2
", frontMatterContent);
            Assert.AreEqual(@"Content1
Content2", documents.First().Content);
        }

        [Test]
        public void SingleCharWithRepeatedDelimiterSplitsAtCorrectLocation()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            IDocument[] inputs = { new Document(metadata, pipeline).Clone(@"FM1
FM2
!!!!
Content1
Content2") };
            string frontMatterContent = null;
            FrontMatter frontMatter = new FrontMatter('!', new Execute(x =>
            {
                frontMatterContent = x.Content;
                return new[] { x };
            }));

            // When
            IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

            // Then
            Assert.AreEqual(1, documents.Count());
            Assert.AreEqual(@"FM1
FM2
", frontMatterContent);
            Assert.AreEqual(@"Content1
Content2", documents.First().Content);
        }

        [Test]
        public void SingleCharWithRepeatedDelimiterWithTrailingSpacesSplitsAtCorrectLocation()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            IDocument[] inputs = { new Document(metadata, pipeline).Clone(@"FM1
FM2
!!!!  
Content1
Content2") };
            string frontMatterContent = null;
            FrontMatter frontMatter = new FrontMatter('!', new Execute(x =>
            {
                frontMatterContent = x.Content;
                return new[] { x };
            }));

            // When
            IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

            // Then
            Assert.AreEqual(1, documents.Count());
            Assert.AreEqual(@"FM1
FM2
", frontMatterContent);
            Assert.AreEqual(@"Content1
Content2", documents.First().Content);
        }

        [Test]
        public void SingleCharWithRepeatedDelimiterWithLeadingSpacesDoesNotSplit()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            IDocument[] inputs = { new Document(metadata, pipeline).Clone(@"FM1
FM2
  !!!!
Content1
Content2") };
            bool executed = false;
            FrontMatter frontMatter = new FrontMatter('!', new Execute(x =>
            {
                executed = true;
                return new[] { x };
            }));

            // When
            IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

            // Then
            Assert.AreEqual(1, documents.Count());
            Assert.IsFalse(executed);
            Assert.AreEqual(@"FM1
FM2
  !!!!
Content1
Content2", documents.First().Content);
        }

        [Test]
        public void SingleCharWithRepeatedDelimiterWithExtraLinesSplitsAtCorrectLocation()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            IDocument[] inputs = { new Document(metadata, pipeline).Clone(@"FM1
FM2

!!!!

Content1
Content2") };
            string frontMatterContent = null;
            FrontMatter frontMatter = new FrontMatter('!', new Execute(x =>
            {
                frontMatterContent = x.Content;
                return new[] { x };
            }));

            // When
            IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

            // Then
            Assert.AreEqual(1, documents.Count());
            Assert.AreEqual(@"FM1
FM2

", frontMatterContent);
            Assert.AreEqual(@"
Content1
Content2", documents.First().Content);
        }

        [Test]
        public void SingleCharWithSingleDelimiterSplitsAtCorrectLocation()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            IDocument[] inputs = { new Document(metadata, pipeline).Clone(@"FM1
FM2
!
Content1
Content2") };
            string frontMatterContent = null;
            FrontMatter frontMatter = new FrontMatter('!', new Execute(x =>
            {
                frontMatterContent = x.Content;
                return new[] { x };
            }));

            // When
            IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

            // Then
            Assert.AreEqual(1, documents.Count());
            Assert.AreEqual(@"FM1
FM2
", frontMatterContent);
            Assert.AreEqual(@"Content1
Content2", documents.First().Content);
        }

        [Test]
        public void MultipleInputDocumentsResultsInMultipleOutputs()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            IDocument[] inputs = { new Document(metadata, pipeline).Clone(@"AA
-
XX"), new Document(metadata, pipeline).Clone(@"BB
-
YY") };
            string frontMatterContent = string.Empty;
            FrontMatter frontMatter = new FrontMatter(new Execute(x =>
            {
                frontMatterContent += x.Content;
                return new[] { x };
            }));

            // When
            IEnumerable<IDocument> documents = frontMatter.Execute(inputs, context);

            // Then
            Assert.AreEqual(2, documents.Count());
            Assert.AreEqual(@"AA
BB
", frontMatterContent);
            Assert.AreEqual(@"XX", documents.First().Content);
            Assert.AreEqual(@"YY", documents.Skip(1).First().Content);
        }
    }
}
