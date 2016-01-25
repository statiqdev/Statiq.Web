using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Meta;
using Wyam.Core.Meta;
using Wyam.Testing;

namespace Wyam.Core.Tests.Documents
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class MetadataClassTests : BaseFixture
    {
        public class IndexerTests : MetadataClassTests
        {
            [Test]
            public void MissingKeyThrowsKeyNotFoundException()
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
            public void NullKeyThrowsKeyNotFoundException()
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
            public void ReturnsCorrectResultWithMetadataValue()
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
        }

        public class ContainsKeyMethodTests : MetadataClassTests
        {
            [Test]
            public void ReturnsTrueForValidValue()
            {
                // Given
                InitialMetadata initialMetadata = new InitialMetadata {["A"] = "a"};
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
                InitialMetadata initialMetadata = new InitialMetadata {["A"] = "a"};
                Metadata metadata = new Metadata(initialMetadata);

                // When
                bool contains = metadata.ContainsKey("B");

                // Then
                Assert.IsFalse(contains);
            }
        }

        public class TryGetValueMethodTests : MetadataClassTests
        {
            [Test]
            public void ReturnsTrueForValidValue()
            {
                // Given
                InitialMetadata initialMetadata = new InitialMetadata {["A"] = "a"};
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
                InitialMetadata initialMetadata = new InitialMetadata {["A"] = "a"};
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
        }

        public class CloneMethodTests : MetadataClassTests
        {
            [Test]
            public void CanCloneWithNewValues()
            {
                // Given
                InitialMetadata initialMetadata = new InitialMetadata();
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
                InitialMetadata initialMetadata = new InitialMetadata {["A"] = "a"};
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
                InitialMetadata initialMetadata = new InitialMetadata {["A"] = "a"};
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
                InitialMetadata initialMetadata = new InitialMetadata {["A"] = "a"};
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
                InitialMetadata initialMetadata = new InitialMetadata {["A"] = "a"};
                Metadata metadata = new Metadata(initialMetadata);

                // When
                Metadata clone = metadata.Clone(new Dictionary<string, object> {{"A", "b"}});

                // Then
                Assert.AreEqual("a", metadata["A"]);
                Assert.AreEqual("b", clone["A"]);
            }
        }

        public class GetMethodTests : MetadataClassTests
        {
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
            public void ReturnsCorrectResultWithDerivedMetadataValue()
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
            public void MetadataValueCalledForEachRequest()
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
        }

        public class ListMethodTests : MetadataClassTests
        {
            [Test]
            public void ReturnsCorrectResultForList()
            {
                // Given
                InitialMetadata initialMetadata = new InitialMetadata {["A"] = new List<int> {1, 2, 3}};
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
                InitialMetadata initialMetadata = new InitialMetadata {["A"] = new List<string> {"1", "2", "3"}};
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
                InitialMetadata initialMetadata = new InitialMetadata {["A"] = new List<int> {1, 2, 3}};
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
                InitialMetadata initialMetadata = new InitialMetadata {["A"] = new[] {1, 2, 3}};
                Metadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<int> result = metadata.List<int>("A");

                // Then
                Assert.IsNotNull(result);
                CollectionAssert.AreEqual(result, new[] {1, 2, 3});
            }
        }

        public class LinkMethodTests : MetadataClassTests
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
                InitialMetadata initialMetadata = new InitialMetadata
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

        public class MetadataAsMethodTests : MetadataClassTests
        {
            [Test]
            public void ConvertIntToString()
            {
                // Given
                InitialMetadata initialMetadata = new InitialMetadata();
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
                InitialMetadata initialMetadata = new InitialMetadata();
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
                InitialMetadata initialMetadata = new InitialMetadata();
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
                InitialMetadata initialMetadata = new InitialMetadata();
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
                InitialMetadata initialMetadata = new InitialMetadata();
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
                InitialMetadata initialMetadata = new InitialMetadata();
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
                InitialMetadata initialMetadata = new InitialMetadata();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", "1") });
                IMetadata<int[]> metadataAs = metadata.MetadataAs<int[]>();

                // Then
                Assert.AreEqual("1", metadata["A"]);
                CollectionAssert.AreEqual(new int[] { 1 }, (IEnumerable)metadataAs["A"]);
            }
        }

        public class EnumeratorTests : MetadataClassTests
        {
            [Test]
            public void EnumeratingMetadataValuesReturnsCorrectResults()
            {
                // Given
                InitialMetadata initialMetadata = new InitialMetadata
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
