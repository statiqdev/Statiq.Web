using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Core.Meta;
using Wyam.Testing;

namespace Wyam.Core.Tests.Meta
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class MetadataTests : BaseFixture
    {
        public class IndexerTests : MetadataTests
        {
            [Test]
            public void MissingKeyThrowsKeyNotFoundException()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata();
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
            public void NullKeyThrowsKeyNotFoundException()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata();
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
            public void ReturnsCorrectResultWithMetadataValue()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata
                {
                    ["A"] = new SimpleMetadataValue { Value = "a" }
                };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                object value = metadata["A"];

                // Then
                Assert.AreEqual("a", value);
            }
        }

        public class ContainsKeyMethodTests : MetadataTests
        {
            [Test]
            public void ReturnsTrueForValidValue()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata {["A"] = "a"};
                Metadata metadata = new Metadata(initialMetadata);

                // When
                bool contains = metadata.ContainsKey("A");

                // Then
                Assert.IsTrue(contains);
            }

            [Test]
            public void ReturnsFalseForInvalidValue()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata {["A"] = "a"};
                Metadata metadata = new Metadata(initialMetadata);

                // When
                bool contains = metadata.ContainsKey("B");

                // Then
                Assert.IsFalse(contains);
            }
        }

        public class TryGetValueMethodTests : MetadataTests
        {
            [Test]
            public void ReturnsTrueForValidValue()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata {["A"] = "a"};
                Metadata metadata = new Metadata(initialMetadata);

                // When
                object value;
                bool contains = metadata.TryGetValue("A", out value);

                // Then
                Assert.IsTrue(contains);
                Assert.AreEqual("a", value);
            }

            [Test]
            public void ReturnsFalseForInvalidValue()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata {["A"] = "a"};
                Metadata metadata = new Metadata(initialMetadata);

                // When
                object value;
                bool contains = metadata.TryGetValue("B", out value);

                // Then
                Assert.IsFalse(contains);
                Assert.AreEqual(null, value);
            }

            [Test]
            public void ReturnsCorrectResultWithMetadataValue()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata
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
        }

        public class CloneMethodTests : MetadataTests
        {
            [Test]
            public void CanCloneWithNewValues()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", "a") });

                // Then
                Assert.AreEqual("a", metadata["A"]);
            }

            [Test]
            public void ContainsPreviousValues()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata {["A"] = "a"};
                Metadata metadata = new Metadata(initialMetadata);

                // When
                Metadata clone = metadata.Clone(new Dictionary<string, object> {{"B", "b"}});

                // Then
                Assert.AreEqual("a", clone["A"]);
            }

            [Test]
            public void ClonedMetadataDoesNotContainNewValues()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata {["A"] = "a"};
                Metadata metadata = new Metadata(initialMetadata);

                // When
                Metadata clone = metadata.Clone(new Dictionary<string, object> {{"B", "b"}});

                // Then
                Assert.IsFalse(metadata.ContainsKey("B"));
            }

            [Test]
            public void ContainsNewValues()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata {["A"] = "a"};
                Metadata metadata = new Metadata(initialMetadata);

                // When
                Metadata clone = metadata.Clone(new Dictionary<string, object> {{"B", "b"}});

                // Then
                Assert.AreEqual("b", clone["B"]);
            }

            [Test]
            public void ReplacesValue()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata {["A"] = "a"};
                Metadata metadata = new Metadata(initialMetadata);

                // When
                Metadata clone = metadata.Clone(new Dictionary<string, object> {{"A", "b"}});

                // Then
                Assert.AreEqual("a", metadata["A"]);
                Assert.AreEqual("b", clone["A"]);
            }
        }

        public class GetMethodTests : MetadataTests
        {
            [Test]
            public void GetWithMetadataValueReturnsCorrectResult()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata { ["A"] = "a" };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                object value = metadata.Get("A");

                // Then
                Assert.AreEqual("a", value);
            }

            [Test]
            public void ReturnsCorrectResultWithDerivedMetadataValue()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata
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
            public void MetadataValueCalledForEachRequest()
            {
                // Given
                SimpleMetadataValue metadataValue = new SimpleMetadataValue { Value = "a" };
                SimpleMetadata initialMetadata = new SimpleMetadata { ["A"] = metadataValue };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                object value = metadata.Get("A");
                value = metadata.Get("A");
                value = metadata.Get("A");

                // Then
                Assert.AreEqual("a", value);
                Assert.AreEqual(3, metadataValue.Calls);
            }
        }

        public class ListMethodTests : MetadataTests
        {
            [Test]
            public void ReturnsCorrectResultForList()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata {["A"] = new List<int> {1, 2, 3}};
                Metadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<int> result = metadata.List<int>("A");

                // Then
                Assert.IsNotNull(result);
                CollectionAssert.AreEqual(result, new[] {1, 2, 3});
            }

            [Test]
            public void ReturnsCorrectResultForConvertedStringList()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata {["A"] = new List<string> {"1", "2", "3"}};
                Metadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<int> result = metadata.List<int>("A");

                // Then
                Assert.IsNotNull(result);
                CollectionAssert.AreEqual(result, new[] {1, 2, 3});
            }

            [Test]
            public void ReturnsCorrectResultForConvertedIntList()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata {["A"] = new List<int> {1, 2, 3}};
                Metadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<string> result = metadata.List<string>("A");

                // Then
                Assert.IsNotNull(result);
                CollectionAssert.AreEqual(result, new[] {"1", "2", "3"});
            }

            [Test]
            public void ReturnsCorrectResultForArray()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata {["A"] = new[] {1, 2, 3}};
                Metadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<int> result = metadata.List<int>("A");

                // Then
                Assert.IsNotNull(result);
                CollectionAssert.AreEqual(result, new[] {1, 2, 3});
            }
        }

        public class DocumentsMethodTests : MetadataTests
        {
            [Test]
            public void ReturnsNullWhenKeyNotFound()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<IDocument> result = metadata.Documents("A");

                // Then
                Assert.IsNull(result);
            }

            [Test]
            public void ReturnsEmptyListForListOfInt()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata { ["A"] = new List<int> { 1, 2, 3 } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<IDocument> result = metadata.Documents("A");

                // Then
                Assert.IsNotNull(result);
                CollectionAssert.IsEmpty(result);
            }

            [Test]
            public void ReturnsEmptyListForSingleInt()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata { ["A"] = 1 };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<IDocument> result = metadata.Documents("A");

                // Then
                Assert.IsNotNull(result);
                CollectionAssert.IsEmpty(result);
            }

            [Test]
            public void ReturnsListForList()
            {
                // Given
                IDocument a = Substitute.For<IDocument>();
                IDocument b = Substitute.For<IDocument>();
                IDocument c = Substitute.For<IDocument>();
                SimpleMetadata initialMetadata = new SimpleMetadata { ["A"] = new List<IDocument> { a, b, c } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<IDocument> result = metadata.Documents("A");

                // Then
                Assert.IsNotNull(result);
                CollectionAssert.AreEqual(new [] { a, b, c }, result);
            }

            [Test]
            public void ReturnsListForSingleDocument()
            {
                // Given
                IDocument a = Substitute.For<IDocument>();
                SimpleMetadata initialMetadata = new SimpleMetadata { ["A"] = a };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<IDocument> result = metadata.Documents("A");

                // Then
                Assert.IsNotNull(result);
                CollectionAssert.AreEqual(new[] { a }, result);
            }
        }

        public class LinkMethodTests : MetadataTests
        {
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
            public void ReturnsCorrectResult(string value, bool pretty, string link)
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata
                {
                    ["A"] = new SimpleMetadataValue {Value = value}
                };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                object result = metadata.Link("A", pretty: pretty);

                // Then
                Assert.AreEqual(link, result);
            }
        }

        public class MetadataAsMethodTests : MetadataTests
        {
            [Test]
            public void ConvertIntToString()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", 1) });
                IMetadata<string> metadataAs = metadata.MetadataAs<string>();

                // Then
                Assert.AreEqual(1, metadata["A"]);
                Assert.AreEqual("1", metadataAs["A"]);
            }

            [Test]
            public void ConvertStringToInt()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", "1") });
                IMetadata<int> metadataAs = metadata.MetadataAs<int>();

                // Then
                Assert.AreEqual("1", metadata["A"]);
                Assert.AreEqual(1, metadataAs["A"]);
            }

            [Test]
            public void ConvertIntArrayToStringArray()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", new int[] { 1, 2, 3 }) });
                IMetadata<string[]> metadataAs = metadata.MetadataAs<string[]>();

                // Then
                CollectionAssert.AreEqual(new int[] { 1, 2, 3 }, (IEnumerable)metadata["A"]);
                CollectionAssert.AreEqual(new string[] { "1", "2", "3" }, (IEnumerable)metadataAs["A"]);
            }

            [Test]
            public void ConvertStringArrayToIntArray()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", new string[] { "1", "2", "3" }) });
                IMetadata<int[]> metadataAs = metadata.MetadataAs<int[]>();

                // Then
                CollectionAssert.AreEqual(new string[] { "1", "2", "3" }, (IEnumerable)metadata["A"]);
                CollectionAssert.AreEqual(new int[] { 1, 2, 3 }, (IEnumerable)metadataAs["A"]);
            }

            [Test]
            public void ConvertIntArrayToStringEnumerable()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", new int[] { 1, 2, 3 }) });
                IMetadata<IEnumerable<string>> metadataAs = metadata.MetadataAs<IEnumerable<string>>();

                // Then
                CollectionAssert.AreEqual(new int[] { 1, 2, 3 }, (IEnumerable)metadata["A"]);
                CollectionAssert.AreEqual(new string[] { "1", "2", "3" }, metadataAs["A"]);
            }

            [Test]
            public void ConvertStringEnumerableToIntArray()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", new List<string> { "1", "2", "3" }) });
                IMetadata<int[]> metadataAs = metadata.MetadataAs<int[]>();

                // Then
                CollectionAssert.AreEqual(new string[] { "1", "2", "3" }, (IEnumerable)metadata["A"]);
                CollectionAssert.AreEqual(new int[] { 1, 2, 3 }, (IEnumerable)metadataAs["A"]);
            }

            [Test]
            public void ConvertStringToIntArray()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", "1") });
                IMetadata<int[]> metadataAs = metadata.MetadataAs<int[]>();

                // Then
                Assert.AreEqual("1", metadata["A"]);
                CollectionAssert.AreEqual(new int[] { 1 }, (IEnumerable)metadataAs["A"]);
            }
        }

        public class EnumeratorTests : MetadataTests
        {
            [Test]
            public void EnumeratingMetadataValuesReturnsCorrectResults()
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata
                {
                    ["A"] = new SimpleMetadataValue {Value = "a"},
                    ["B"] = new SimpleMetadataValue {Value = "b"},
                    ["C"] = new SimpleMetadataValue {Value = "c"}
                };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                object[] values = metadata.Select(x => x.Value).ToArray();

                // Then
                CollectionAssert.AreEquivalent(new[] {"a", "b", "c"}, values);
            }
        }

        private class SimpleMetadataValue : IMetadataValue
        {
            public string Value { get; set; }
            public int Calls { get; set; }

            object IMetadataValue.Get(string key, IMetadata metadata)
            {
                Calls++;
                return Value;
            }
        }

        private class DerivedMetadataValue : IMetadataValue
        {
            public string Key { get; set; }

            object IMetadataValue.Get(string key, IMetadata metadata)
            {
                return metadata[Key];
            }
        }
    }
}
