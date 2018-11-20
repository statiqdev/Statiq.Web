using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Core.Documents;
using Wyam.Core.Meta;
using Wyam.Testing;
using Wyam.Testing.Execution;

namespace Wyam.Core.Tests.Documents
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class CustomDocumentFactoryFixture : BaseFixture
    {
        public class GetDocumentTests : CustomDocumentFactoryFixture
        {
            [Test]
            public void GetsInitialDocumentWithInitialMetadata()
            {
                // Given
                MetadataDictionary initialMetadata = new MetadataDictionary();
                initialMetadata.Add("Foo", "Bar");
                DocumentFactory documentFactory = new DocumentFactory(initialMetadata);
                CustomDocumentFactory<TestDocument> customDocumentFactory = new CustomDocumentFactory<TestDocument>(documentFactory);
                TestExecutionContext context = new TestExecutionContext();

                // When
                IDocument resultDocument = customDocumentFactory.GetDocument(context);

                // Then
                Assert.IsInstanceOf<TestDocument>(resultDocument);
                CollectionAssert.AreEqual(
                    new Dictionary<string, object>
                    {
                        { "Foo", "Bar" }
                    }, resultDocument);
            }

            [Test]
            public void ThrowsWhenCloneReturnsNullDocument()
            {
                // Given
                MetadataDictionary initialMetadata = new MetadataDictionary();
                DocumentFactory documentFactory = new DocumentFactory(initialMetadata);
                CustomDocumentFactory<TestDocument> customDocumentFactory = new CustomDocumentFactory<TestDocument>(documentFactory);
                TestExecutionContext context = new TestExecutionContext();
                CloneReturnsNullDocument document = new CloneReturnsNullDocument();

                // When, Then
                Assert.Throws<Exception>(() => customDocumentFactory.GetDocument(context, document, new Dictionary<string, object>()));
            }

            [Test]
            public void ThrowsWhenCloneReturnsSameDocument()
            {
                // Given
                MetadataDictionary initialMetadata = new MetadataDictionary();
                DocumentFactory documentFactory = new DocumentFactory(initialMetadata);
                CustomDocumentFactory<TestDocument> customDocumentFactory = new CustomDocumentFactory<TestDocument>(documentFactory);
                TestExecutionContext context = new TestExecutionContext();
                CloneReturnsSameDocument document = new CloneReturnsSameDocument();

                // When, Then
                Assert.Throws<Exception>(() => customDocumentFactory.GetDocument(context, document, new Dictionary<string, object>()));
            }

            [Test]
            public void CloneResultsInClonedDocument()
            {
                // Given
                MetadataDictionary initialMetadata = new MetadataDictionary();
                initialMetadata.Add("Foo", "Bar");
                DocumentFactory documentFactory = new DocumentFactory(initialMetadata);
                CustomDocumentFactory<TestDocument> customDocumentFactory = new CustomDocumentFactory<TestDocument>(documentFactory);
                TestExecutionContext context = new TestExecutionContext();
                CustomDocument sourceDocument = (CustomDocument)customDocumentFactory.GetDocument(context);

                // When
                IDocument resultDocument = customDocumentFactory.GetDocument(
                    context,
                    sourceDocument,
                    new Dictionary<string, object>
                    {
                        { "Baz", "Bat" }
                    });

                // Then
                CollectionAssert.AreEquivalent(
                    new Dictionary<string, object>
                    {
                        { "Foo", "Bar" }
                    },
                    sourceDocument);
                CollectionAssert.AreEquivalent(
                    new Dictionary<string, object>
                    {
                        { "Foo", "Bar" },
                        { "Baz", "Bat" }
                    },
                    resultDocument);
            }
        }

        private class TestDocument : CustomDocument
        {
            public string Title { get; set; }

            protected internal override CustomDocument Clone()
            {
                return new TestDocument
                {
                    Title = Title
                };
            }
        }

        private class CloneReturnsNullDocument : CustomDocument
        {
            protected internal override CustomDocument Clone()
            {
                return null;
            }
        }

        private class CloneReturnsSameDocument : CustomDocument
        {
            protected internal override CustomDocument Clone()
            {
                return this;
            }
        }
    }
}
