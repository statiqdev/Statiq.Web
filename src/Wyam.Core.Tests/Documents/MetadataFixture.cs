using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Tracing;
using Wyam.Core;
using Wyam.Core.Documents;
using Wyam.Core.Meta;
using Wyam.Testing;

namespace Wyam.Core.Tests.Documents
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class MetadataFixture : TraceListenerFixture
    {
        [Test]
        public void CanCloneWithNewValues()
        {
            // Given
            InitialMetadata initialMetadata = new InitialMetadata();
            Metadata metadata = new Metadata(initialMetadata);

            // When
            metadata = metadata.Clone(new [] { new KeyValuePair<string, object>("A", "a") });

            // Then
            Assert.AreEqual("a", metadata["A"]);
        }

        [Test]
        public void IndexerMissingKeyThrowsKeyNotFoundException()
        {
            // Given
            InitialMetadata initialMetadata = new InitialMetadata();
            Metadata metadata = new Metadata(initialMetadata);

            // When
            TestDelegate test = () =>
            {
                object value = metadata["A"];
            };

            // Then
            Assert.Throws<KeyNotFoundException>(test);
        }

        [Test]
        public void IndexerNullKeyThrowsKeyNotFoundException()
        {
            // Given
            InitialMetadata initialMetadata = new InitialMetadata();
            Metadata metadata = new Metadata(initialMetadata);

            // When
            TestDelegate test = () =>
            {
                object value = metadata[null];
            };

            // Then
            Assert.Throws<ArgumentNullException>(test);
        }
        
        [Test]
        public void ContainsKeyReturnsTrueForValidValue()
        {
            // Given
            InitialMetadata initialMetadata = new InitialMetadata { ["A"] = "a" };
            Metadata metadata = new Metadata(initialMetadata);

            // When
            bool contains = metadata.ContainsKey("A");

            // Then
            Assert.IsTrue(contains);
        }

        [Test]
        public void ContainsReturnsFalseForInvalidValue()
        {
            // Given
            InitialMetadata initialMetadata = new InitialMetadata { ["A"] = "a" };
            Metadata metadata = new Metadata(initialMetadata);

            // When
            bool contains = metadata.ContainsKey("B");

            // Then
            Assert.IsFalse(contains);
        }

        [Test]
        public void TryGetValueReturnsTrueForValidValue()
        {
            // Given
            InitialMetadata initialMetadata = new InitialMetadata { ["A"] = "a" };
            Metadata metadata = new Metadata(initialMetadata);

            // When
            object value;
            bool contains = metadata.TryGetValue("A", out value);

            // Then
            Assert.IsTrue(contains);
            Assert.AreEqual("a", value);
        }        

        [Test]
        public void TryGetValueReturnsFalseForInvalidValue()
        {
            // Given
            InitialMetadata initialMetadata = new InitialMetadata { ["A"] = "a" };
            Metadata metadata = new Metadata(initialMetadata);

            // When
            object value;
            bool contains = metadata.TryGetValue("B", out value);

            // Then
            Assert.IsFalse(contains);
            Assert.AreEqual(null, value);
        }

        [Test]
        public void CloneContainsPreviousValues()
        {
            // Given
            InitialMetadata initialMetadata = new InitialMetadata { ["A"] = "a" };
            Metadata metadata = new Metadata(initialMetadata);

            // When
            Metadata clone = metadata.Clone(new Dictionary<string, object> { { "B", "b" } });

            // Then
            Assert.AreEqual("a", clone["A"]);
        }

        [Test]
        public void ClonedMetadataDoesNotContainNewValues()
        {
            // Given
            InitialMetadata initialMetadata = new InitialMetadata { ["A"] = "a" };
            Metadata metadata = new Metadata(initialMetadata);

            // When
            Metadata clone = metadata.Clone(new Dictionary<string, object> { { "B", "b" } });

            // Then
            Assert.IsFalse(metadata.ContainsKey("B"));
        }

        [Test]
        public void CloneContainsNewValues()
        {
            // Given
            InitialMetadata initialMetadata = new InitialMetadata { ["A"] = "a" };
            Metadata metadata = new Metadata(initialMetadata);

            // When
            Metadata clone = metadata.Clone(new Dictionary<string, object> { { "B", "b" } });

            // Then
            Assert.AreEqual("b", clone["B"]);
        }

        [Test]
        public void CloneReplacesValue()
        {
            // Given
            InitialMetadata initialMetadata = new InitialMetadata { ["A"] = "a" };
            Metadata metadata = new Metadata(initialMetadata);

            // When
            Metadata clone = metadata.Clone(new Dictionary<string, object> { { "A", "b" } });

            // Then
            Assert.AreEqual("a", metadata["A"]);
            Assert.AreEqual("b", clone["A"]);
        }

        [Test]
        public void GetWithMetadataValueReturnsCorrectResult()
        {
            // Given
            InitialMetadata initialMetadata = new InitialMetadata { ["A"] = "a" };
            Metadata metadata = new Metadata(initialMetadata);

            // When
            object value = metadata.Get("A");

            // Then
            Assert.AreEqual("a", value);
        }

        [Test]
        public void ListReturnsCorrectResultForList()
        {
            // Given
            InitialMetadata initialMetadata = new InitialMetadata { ["A"] = new List<int> { 1, 2 ,3 } };
            Metadata metadata = new Metadata(initialMetadata);

            // When
            IReadOnlyList<int> result = metadata.List<int>("A");

            // Then
            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(result, new [] { 1, 2, 3 });
        }

        [Test]
        public void ListReturnsCorrectResultForConvertedStringList()
        {
            // Given
            InitialMetadata initialMetadata = new InitialMetadata { ["A"] = new List<string> { "1", "2", "3" } };
            Metadata metadata = new Metadata(initialMetadata);

            // When
            IReadOnlyList<int> result = metadata.List<int>("A");

            // Then
            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(result, new[] { 1, 2, 3 });
        }

        [Test]
        public void ListReturnsCorrectResultForConvertedIntList()
        {
            // Given
            InitialMetadata initialMetadata = new InitialMetadata { ["A"] = new List<int> { 1, 2, 3 } };
            Metadata metadata = new Metadata(initialMetadata);

            // When
            IReadOnlyList<string> result = metadata.List<string>("A");

            // Then
            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(result, new[] { "1", "2", "3" });
        }

        [Test]
        public void ListReturnsCorrectResultForArray()
        {
            // Given
            InitialMetadata initialMetadata = new InitialMetadata { ["A"] = new [] { 1, 2, 3 } };
            Metadata metadata = new Metadata(initialMetadata);

            // When
            IReadOnlyList<int> result = metadata.List<int>("A");

            // Then
            Assert.IsNotNull(result);
            CollectionAssert.AreEqual(result, new[] { 1, 2, 3 });
        }

        [TestCase("foo/bar", false, "/foo/bar")]
        [TestCase("foo/bar", true, "/foo/bar")]
        [TestCase("/foo/bar", false, "/foo/bar")]
        [TestCase("/foo/bar", true, "/foo/bar")]
        [TestCase("/foo/bar/index.html", false, "/foo/bar/index.html")]
        [TestCase("/foo/bar/index.html", true, "/foo/bar")]
        [TestCase("/foo/bar/index.htm", false, "/foo/bar/index.htm")]
        [TestCase("/foo/bar/index.htm", true, "/foo/bar")]
        [TestCase("/foo/bar/baz.html", false, "/foo/bar/baz.html")]
        [TestCase("/foo/bar/baz.html", true, "/foo/bar/baz.html")]
        [TestCase(null, false, "#")]
        [TestCase(null, true, "#")]
        [TestCase("", false, "#")]
        [TestCase("", true, "#")]
        [TestCase(" ", false, "#")]
        [TestCase(" ", true, "#")]
        [TestCase("/index.html", false, "/index.html")]
        [TestCase("/index.html", true, "/")]
        [TestCase("index.html", false, "/index.html")]
        [TestCase("index.html", true, "/")]
        [TestCase("/foo.html", false, "/foo.html")]
        [TestCase("/foo.html", true, "/foo.html")]
        [TestCase("foo.html", false, "/foo.html")]
        [TestCase("foo.html", true, "/foo.html")]
        public void LinkReturnsCorrectResult(string value, bool pretty, string link)
        {
            // Given
            InitialMetadata initialMetadata = new InitialMetadata
            {
                ["A"] = new SimpleMetadataValue { Value = value }
            };
            Metadata metadata = new Metadata(initialMetadata);

            // When
            object result = metadata.Link("A", pretty: pretty);

            // Then
            Assert.AreEqual(link, result);
        }

        [Test]
        public void IndexerWithMetadataValueReturnsCorrectResult()
        {
            // Given
            InitialMetadata initialMetadata = new InitialMetadata
            {
                ["A"] = new SimpleMetadataValue { Value = "a" }
            };
            Metadata metadata = new Metadata(initialMetadata);

            // When
            object value = metadata["A"];

            // Then
            Assert.AreEqual("a", value);
        }

        [Test]
        public void TryGetValueWithMetadataValueReturnsCorrectResult()
        {
            // Given
            InitialMetadata initialMetadata = new InitialMetadata
            {
                ["A"] = new SimpleMetadataValue { Value = "a" }
            };
            Metadata metadata = new Metadata(initialMetadata);

            // When
            object value;
            bool contains = metadata.TryGetValue("A", out value);

            // Then
            Assert.IsTrue(contains);
            Assert.AreEqual("a", value);
        }

        [Test]
        public void GetWithDerivedMetadataValueReturnsCorrectResult()
        {
            // Given
            InitialMetadata initialMetadata = new InitialMetadata
            {
                ["A"] = new DerivedMetadataValue { Key = "X" },
                ["X"] = "x"
            };
            Metadata metadata = new Metadata(initialMetadata);

            // When
            object value = metadata.Get("A");

            // Then
            Assert.AreEqual("x", value);
        }

        [Test]
        public void GetWithMetadataValueCalledForEachRequest()
        {
            // Given
            SimpleMetadataValue metadataValue = new SimpleMetadataValue { Value = "a" };
            InitialMetadata initialMetadata = new InitialMetadata { ["A"] = metadataValue };
            Metadata metadata = new Metadata(initialMetadata);

            // When
            object value = metadata.Get("A");
            value = metadata.Get("A");
            value = metadata.Get("A");

            // Then
            Assert.AreEqual("a", value);
            Assert.AreEqual(3, metadataValue.Calls);
        }

        [Test]
        public void EnumeratingMetadataValuesReturnsCorrectResults()
        {
            // Given
            InitialMetadata initialMetadata = new InitialMetadata
            {
                ["A"] = new SimpleMetadataValue { Value = "a" },
                ["B"] = new SimpleMetadataValue { Value = "b" },
                ["C"] = new SimpleMetadataValue { Value = "c" }
            };
            Metadata metadata = new Metadata(initialMetadata);

            // When
            object[] values = metadata.Select(x => x.Value).ToArray();

            // Then
            CollectionAssert.AreEquivalent(new [] { "a", "b", "c" }, values);
        }

        private class SimpleMetadataValue : IMetadataValue
        {
            public string Value { get; set; }
            public int Calls { get; set; }

            public object Get(string key, IMetadata metadata)
            {
                Calls++;
                return Value;
            }
        }

        private class DerivedMetadataValue : IMetadataValue
        {
            public string Key { get; set; }

            public object Get(string key, IMetadata metadata)
            {
                return metadata[Key];
            }
        }
    }
}
