using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Wyam.Common.Documents;
using Wyam.Common.IO;
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
            public void GlobalMetadataRetainedInNextIterations()
            {
                // TODO add complex object (ex. List<string>) test
                SimpleMetadata meta = new SimpleMetadata();

                meta.Add("hello", "world");
                meta.Add("obj", 123);

                {
                    ISimpleMetadata cloned = meta.Clone();
                    Assert.IsTrue(((string)cloned["hello"]).Equals("world", StringComparison.Ordinal));
                    Assert.AreEqual((int)cloned["obj"], 123);
                    cloned.Clear();

                    Assert.IsTrue(cloned.Count == 0);
                }
                {
                    ISimpleMetadata cloned = meta.Clone();
                    Assert.IsTrue(cloned.Count == 2);
                    Assert.IsTrue(((string)cloned["hello"]).Equals("world", StringComparison.Ordinal));
                    Assert.AreEqual((int)cloned["obj"], 123);
                }
            }

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

        public class StringMethodTests : MetadataTests
        {
            [TestCase("/a/b/c.txt", "/a/b/c.txt")]
            [TestCase("a/b/c.txt", "a/b/c.txt")]
            [TestCase("Foo::/a/b/c.txt", "/a/b/c.txt")]
            public void ReturnsCorrectStringForFilePath(string path, string expected)
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", new FilePath(path)) });
                object result = metadata.String("A");

                // Then
                Assert.IsInstanceOf<string>(result);
                Assert.AreEqual(expected, result);
            }

            [TestCase("/a/b/c", "/a/b/c")]
            [TestCase("a/b/c", "a/b/c")]
            [TestCase("Foo::/a/b/c", "/a/b/c")]
            public void ReturnsCorrectStringForDirectoryPath(string path, string expected)
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", new DirectoryPath(path)) });
                object result = metadata.String("A");

                // Then
                Assert.IsInstanceOf<string>(result);
                Assert.AreEqual(expected, result);
            }
        }

        public class FilePathMethodTests : MetadataTests
        {
            [TestCase("/a/b/c.txt", "/a/b/c.txt")]
            [TestCase("a/b/c.txt", "a/b/c.txt")]
            [TestCase("Foo::/a/b/c.txt", "/a/b/c.txt")]
            public void ReturnsCorrectFilePathForFilePath(string path, string expected)
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", new FilePath(path)) });
                object result = metadata.FilePath("A");

                // Then
                Assert.IsInstanceOf<FilePath>(result);
                Assert.AreEqual(expected, ((FilePath)result).FullPath);
            }
            
            [TestCase("/a/b/c.txt", "/a/b/c.txt")]
            [TestCase("a/b/c.txt", "a/b/c.txt")]
            [TestCase("Foo::/a/b/c.txt", "/a/b/c.txt")]
            [TestCase("Foo::a/b/c.txt", null)]
            [TestCase(null, null)]
            public void ReturnsCorrectFilePathForString(string path, string expected)
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", path) });
                object result = metadata.FilePath("A");

                // Then
                if (expected == null)
                {
                    Assert.IsNull(result);
                }
                else
                {
                    Assert.IsInstanceOf<FilePath>(result);
                    Assert.AreEqual(expected, ((FilePath)result).FullPath);
                }
            }
        }

        public class DirectoryPathMethodTests : MetadataTests
        {
            [TestCase("/a/b/c", "/a/b/c")]
            [TestCase("a/b/c", "a/b/c")]
            [TestCase("Foo::/a/b/c", "/a/b/c")]
            public void ReturnsCorrectDirectoryPathForDirectoryPath(string path, string expected)
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", new DirectoryPath(path)) });
                object result = metadata.DirectoryPath("A");

                // Then
                Assert.IsInstanceOf<DirectoryPath>(result);
                Assert.AreEqual(expected, ((DirectoryPath)result).FullPath);
            }

            [TestCase("/a/b/c", "/a/b/c")]
            [TestCase("a/b/c", "a/b/c")]
            [TestCase("Foo::/a/b/c", "/a/b/c")]
            [TestCase("Foo::a/b/c", null)]
            [TestCase(null, null)]
            public void ReturnsCorrectDirectoryPathForString(string path, string expected)
            {
                // Given
                SimpleMetadata initialMetadata = new SimpleMetadata();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", path) });
                object result = metadata.DirectoryPath("A");

                // Then
                if (expected == null)
                {
                    Assert.IsNull(result);
                }
                else
                {
                    Assert.IsInstanceOf<DirectoryPath>(result);
                    Assert.AreEqual(expected, ((DirectoryPath)result).FullPath);
                }
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
            public object Value { get; set; }
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
