using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Abstractions;

namespace Wyam.Core.Tests
{
    [TestFixture]
    public class DocumentCollectionExtensionsFixture
    {
        [Test]
        public void ToLookupOfIntReturnsCorrectLookup()
        {
            // Given
            Engine engine = new Engine();
            IDocument a = new Document(new Metadata(engine))
                .Clone("a", new[] { new KeyValuePair<string, object>("Numbers", new [] { 1, 2, 3 }) });
            IDocument b = new Document(new Metadata(engine))
                .Clone("b", new[] { new KeyValuePair<string, object>("Numbers", new [] { 2, 3, 4 }) });
            IDocument c = new Document(new Metadata(engine))
                .Clone("c", new[] { new KeyValuePair<string, object>("Numbers", 3) });
            IDocument d = new Document(new Metadata(engine))
                .Clone("d", new[] { new KeyValuePair<string, object>("Numbers", "4") });
            List<IDocument> documents = new List<IDocument>() { a, b, c, d };

            // When
            ILookup<int, IDocument> lookup = documents.ToLookup<int>("Numbers");

            // Then
            Assert.AreEqual(4, lookup.Count);
            CollectionAssert.AreEqual(new[] { a }, lookup[1]);
            CollectionAssert.AreEqual(new[] { a, b }, lookup[2]);
            CollectionAssert.AreEqual(new[] { a, b, c }, lookup[3]);
            CollectionAssert.AreEqual(new[] { b, d }, lookup[4]);
        }

        [Test]
        public void ToLookupOfStringReturnsCorrectLookup()
        {
            // Given
            Engine engine = new Engine();
            IDocument a = new Document(new Metadata(engine))
                .Clone("a", new[] { new KeyValuePair<string, object>("Numbers", new[] { 1, 2, 3 }) });
            IDocument b = new Document(new Metadata(engine))
                .Clone("b", new[] { new KeyValuePair<string, object>("Numbers", new[] { 2, 3, 4 }) });
            IDocument c = new Document(new Metadata(engine))
                .Clone("c", new[] { new KeyValuePair<string, object>("Numbers", 3) });
            IDocument d = new Document(new Metadata(engine))
                .Clone("d", new[] { new KeyValuePair<string, object>("Numbers", "4") });
            List<IDocument> documents = new List<IDocument>() { a, b, c, d };

            // When
            ILookup<string, IDocument> lookup = documents.ToLookup<string>("Numbers");

            // Then
            Assert.AreEqual(4, lookup.Count);
            CollectionAssert.AreEqual(new[] { a }, lookup["1"]);
            CollectionAssert.AreEqual(new[] { a, b }, lookup["2"]);
            CollectionAssert.AreEqual(new[] { a, b, c }, lookup["3"]);
            CollectionAssert.AreEqual(new[] { b, d }, lookup["4"]);
        }
    }
}
