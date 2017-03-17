using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Core.Documents;
using Wyam.Core.Meta;
using Wyam.Testing;

namespace Wyam.Common.Tests.Documents
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ToLookupExtensionsFixture : BaseFixture
    {
        public class ToLookupManyTests : ToLookupExtensionsFixture
        {
            [Test]
            public void ReturnsCorrectLookupOfInt()
            {
                // Given
                IDocument a = new Document(
                    new MetadataDictionary { { "Numbers", new[] { 1, 2, 3 } } },
                    new MemoryStream(Encoding.UTF8.GetBytes("a")));
                IDocument b = new Document(
                    new MetadataDictionary { { "Numbers", new[] { 2, 3, 4 } } },
                    new MemoryStream(Encoding.UTF8.GetBytes("b")));
                IDocument c = new Document(
                    new MetadataDictionary { { "Numbers", 3 } },
                    new MemoryStream(Encoding.UTF8.GetBytes("c")));
                IDocument d = new Document(
                    new MetadataDictionary { { "Numbers", "4" } },
                    new MemoryStream(Encoding.UTF8.GetBytes("d")));
                List<IDocument> documents = new List<IDocument>() { a, b, c, d };

                // When
                ILookup<int, IDocument> lookup = documents.ToLookupMany<int>("Numbers");

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
                    new MetadataDictionary { { "Numbers", new[] { 1, 2, 3 } } },
                    new MemoryStream(Encoding.UTF8.GetBytes("a")));
                IDocument b = new Document(
                    new MetadataDictionary { { "Numbers", new[] { 2, 3, 4 } } },
                    new MemoryStream(Encoding.UTF8.GetBytes("b")));
                IDocument c = new Document(
                    new MetadataDictionary { { "Numbers", 3 } },
                    new MemoryStream(Encoding.UTF8.GetBytes("c")));
                IDocument d = new Document(
                    new MetadataDictionary { { "Numbers", "4" } },
                    new MemoryStream(Encoding.UTF8.GetBytes("d")));
                List<IDocument> documents = new List<IDocument>() { a, b, c, d };

                // When
                ILookup<string, IDocument> lookup = documents.ToLookupMany<string>("Numbers");

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
                    new MetadataDictionary
                    {
                        { "Numbers", new[] { 1, 2, 3 } },
                        { "Colors", "Red" }
                    },
                    new MemoryStream(Encoding.UTF8.GetBytes("a")));
                IDocument b = new Document(
                    new MetadataDictionary
                    {
                        { "Numbers", new[] { 2, 3, 4 } },
                        { "Colors", new [] { "Red", "Blue" } }
                    },
                    new MemoryStream(Encoding.UTF8.GetBytes("b")));
                IDocument c = new Document(
                    new MetadataDictionary
                    {
                        { "Numbers", 3 },
                        { "Colors", "Green" }
                    },
                    new MemoryStream(Encoding.UTF8.GetBytes("c")));
                IDocument d = new Document(
                    new MetadataDictionary
                    {
                        { "Numbers", "4" },
                        { "Colors", new [] { "Green", "Blue" } }
                    },
                    new MemoryStream(Encoding.UTF8.GetBytes("d")));
                List<IDocument> documents = new List<IDocument>() { a, b, c, d };

                // When
                ILookup<int, string> lookup = documents.ToLookupMany<int, string>("Numbers", "Colors");

                // Then
                Assert.AreEqual(4, lookup.Count);
                CollectionAssert.AreEquivalent(new[] { "Red" }, lookup[1]);
                CollectionAssert.AreEquivalent(new[] { "Red", "Red" }, lookup[2]);
                CollectionAssert.AreEquivalent(new[] { "Red", "Red", "Green" }, lookup[3]);
                CollectionAssert.AreEquivalent(new[] { "Red", "Green" }, lookup[4]);
            }
        }

        public class ToLookupManyToManyTests : ToLookupExtensionsFixture
        {
            [Test]
            public void ReturnsCorrectLookupWithValues()
            {
                // Given
                IDocument a = new Document(
                    new MetadataDictionary
                    {
                        { "Numbers", new[] { 1, 2, 3 } },
                        { "Colors", "Red" }
                    },
                    new MemoryStream(Encoding.UTF8.GetBytes("a")));
                IDocument b = new Document(
                    new MetadataDictionary
                    {
                        { "Numbers", new[] { 2, 3, 4 } },
                        { "Colors", new [] { "Red", "Blue" } }
                    },
                    new MemoryStream(Encoding.UTF8.GetBytes("b")));
                IDocument c = new Document(
                    new MetadataDictionary
                    {
                        { "Numbers", 3 },
                        { "Colors", "Green" }
                    },
                    new MemoryStream(Encoding.UTF8.GetBytes("c")));
                IDocument d = new Document(
                    new MetadataDictionary
                    {
                        { "Numbers", "4" },
                        { "Colors", new [] { "Green", "Blue" } }
                    },
                    new MemoryStream(Encoding.UTF8.GetBytes("d")));
                List<IDocument> documents = new List<IDocument>() { a, b, c, d };

                // When
                ILookup<int, string> lookup = documents.ToLookupManyToMany<int, string>("Numbers", "Colors");

                // Then
                Assert.AreEqual(4, lookup.Count);
                CollectionAssert.AreEquivalent(new[] { "Red" }, lookup[1]);
                CollectionAssert.AreEquivalent(new[] { "Red", "Red", "Blue" }, lookup[2]);
                CollectionAssert.AreEquivalent(new[] { "Red", "Red", "Blue", "Green" }, lookup[3]);
                CollectionAssert.AreEquivalent(new[] { "Red", "Blue", "Green", "Blue" }, lookup[4]);
            }
        }
    }
}
