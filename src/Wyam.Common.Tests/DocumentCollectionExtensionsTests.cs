using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Core;
using Wyam.Core.Documents;
using Wyam.Core.Meta;
using Wyam.Core.Pipelines;
using Wyam.Testing;

namespace Wyam.Common.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class DocumentCollectionExtensionsTests : BaseFixture
    {
        public class ToLookupMethodTests : DocumentCollectionExtensionsTests
        {
            [Test]
            public void ReturnsCorrectLookupOfInt()
            {
                // Given
                IDocument a = new Document(
                    new InitialMetadata { { "Numbers", new[] { 1, 2, 3 } } }, "a");
                IDocument b = new Document(
                    new InitialMetadata { { "Numbers", new [] { 2, 3, 4 } } }, "b");
                IDocument c = new Document(
                    new InitialMetadata { { "Numbers", 3 } }, "c");
                IDocument d = new Document(
                    new InitialMetadata { { "Numbers", "4" } }, "d");
                List<IDocument> documents = new List<IDocument>() { a, b, c, d };

                // When
                ILookup<int, IDocument> lookup = documents.ToLookup<int>("Numbers");

                // Then
                Assert.AreEqual(4, lookup.Count);
                CollectionAssert.AreEquivalent(new[] { a }, lookup[1]);
                CollectionAssert.AreEquivalent(new[] { a, b }, lookup[2]);
                CollectionAssert.AreEquivalent(new[] { a, b, c }, lookup[3]);
                CollectionAssert.AreEquivalent(new[] { b, d }, lookup[4]);
            }

            [Test]
            public void ReturnsCorrectLookupOfString()
            {
                // Given
                IDocument a = new Document(
                    new InitialMetadata { { "Numbers", new[] { 1, 2, 3 } } }, "a");
                IDocument b = new Document(
                    new InitialMetadata { { "Numbers", new[] { 2, 3, 4 } } }, "b");
                IDocument c = new Document(
                    new InitialMetadata { { "Numbers", 3 } }, "c");
                IDocument d = new Document(
                    new InitialMetadata { { "Numbers", "4" } }, "d");
                List<IDocument> documents = new List<IDocument>() { a, b, c, d };

                // When
                ILookup<string, IDocument> lookup = documents.ToLookup<string>("Numbers");

                // Then
                Assert.AreEqual(4, lookup.Count);
                CollectionAssert.AreEquivalent(new[] { a }, lookup["1"]);
                CollectionAssert.AreEquivalent(new[] { a, b }, lookup["2"]);
                CollectionAssert.AreEquivalent(new[] { a, b, c }, lookup["3"]);
                CollectionAssert.AreEquivalent(new[] { b, d }, lookup["4"]);
            }

            [Test]
            public void ReturnsCorrectLookupWithValues()
            {
                // Given
                IDocument a = new Document(
                    new InitialMetadata
                    {
                        { "Numbers", new[] { 1, 2, 3 } },
                        { "Colors", "Red" }
                    }, "a");
                IDocument b = new Document(
                    new InitialMetadata
                    {
                        { "Numbers", new[] { 2, 3, 4 } },
                        { "Colors", new [] { "Red", "Blue" } }
                    }, "b");
                IDocument c = new Document(
                    new InitialMetadata
                    {
                        { "Numbers", 3 },
                        { "Colors", "Green" }
                    }, "c");
                IDocument d = new Document(
                    new InitialMetadata
                    {
                        { "Numbers", "4" },
                        { "Colors", new [] { "Green", "Blue" } }
                    }, "d");
                List<IDocument> documents = new List<IDocument>() { a, b, c, d };

                // When
                ILookup<int, string> lookup = documents.ToLookup<int, string>("Numbers", "Colors");

                // Then
                Assert.AreEqual(4, lookup.Count);
                CollectionAssert.AreEquivalent(new[] { "Red" }, lookup[1]);
                CollectionAssert.AreEquivalent(new[] { "Red", "Blue" }, lookup[2]);
                CollectionAssert.AreEquivalent(new[] { "Red", "Blue", "Green" }, lookup[3]);
                CollectionAssert.AreEquivalent(new[] { "Red", "Blue", "Green" }, lookup[4]);
            }
        }
    }
}
