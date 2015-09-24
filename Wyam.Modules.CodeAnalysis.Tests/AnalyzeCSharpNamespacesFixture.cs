using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;
using Wyam.Common;

namespace Wyam.Modules.CodeAnalysis.Tests
{
    [TestFixture]
    public class AnalyzeCSharpNamespacesFixture
    {
        [Test]
        public void GetsTopLevelNamespaces()
        {
            // Given
            string code = @"
                namespace Foo
                {
                }

                namespace Bar
                {
                }
            ";
            TestObjects test = new TestObjects(code);

            // When
            test.Module.Execute(new[] { test.Document }, test.Context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new [] { string.Empty, "Foo", "Bar" }, test.Metadata.Select(x => x["Name"]));
            test.Stream.Dispose();
        }

        [Test]
        public void TopLevelNamespaceContainsDirectlyNestedNamespaces()
        {
            // Given
            string code = @"
                namespace Foo
                {
                }

                namespace Foo.Baz
                {
                }

                namespace Bar
                {
                }
            ";
            TestObjects test = new TestObjects(code);

            // When
            test.Module.Execute(new[] { test.Document }, test.Context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new[] { string.Empty, "Foo", "Baz", "Bar" }, test.Metadata.Select(x => x["Name"]));
            CollectionAssert.AreEquivalent(new [] { "Foo", "Bar" },
                test.GetChildMetadata<string>(x => (string)x["Name"] == string.Empty, "NestedNamespaces", "Name"));
            test.Stream.Dispose();
        }

        [Test]
        public void NestedNamespaceContainsDirectlyNestedNamespaces()
        {
            // Given
            string code = @"
                namespace Foo
                {
                }

                namespace Foo.Baz
                {
                }

                namespace Foo.Bar
                {
                }
            ";
            TestObjects test = new TestObjects(code);

            // When
            test.Module.Execute(new[] { test.Document }, test.Context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new[] { string.Empty, "Foo", "Baz", "Bar" }, test.Metadata.Select(x => x["Name"]));
            CollectionAssert.AreEquivalent(new[] { "Baz", "Bar" },
                test.GetChildMetadata<string>(x => (string)x["Name"] == "Foo", "NestedNamespaces", "Name"));
            test.Stream.Dispose();
        }

        [Test]
        public void NamespaceDisplayStringContainsFullHierarchy()
        {
            // Given
            string code = @"
                namespace Foo
                {
                }

                namespace Foo.Bar
                {
                }
            ";
            TestObjects test = new TestObjects(code);

            // When
            test.Module.Execute(new[] { test.Document }, test.Context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new[] { "<global namespace>", "Foo", "Foo.Bar" }, test.Metadata.Select(x => x["DisplayString"]));
            test.Stream.Dispose();
        }

        [Test]
        public void NamespaceKindIsNamespace()
        {
            // Given
            string code = @"
                namespace Foo
                {
                }

                namespace Foo.Bar
                {
                }
            ";
            TestObjects test = new TestObjects(code);

            // When
            test.Module.Execute(new[] { test.Document }, test.Context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new[] { "Namespace", "Namespace", "Namespace" }, test.Metadata.Select(x => x["Kind"]));
            test.Stream.Dispose();
        }

        [Test]
        public void NestedNamespacesReferenceParents()
        {
            // Given
            string code = @"
                namespace Foo
                {
                }

                namespace Foo.Bar
                {
                }
            ";
            TestObjects test = new TestObjects(code);

            // When
            test.Module.Execute(new[] { test.Document }, test.Context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual("Foo", test.GetMetadata<IDocument>(x => (string)x["Name"] == "Bar", "ParentNamespace").Get("Name"));
            test.Stream.Dispose();
        }

        [Test]
        public void NamespacesContainTypes()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    class Red
                    {
                    }
                }

                namespace Foo.Bar
                {
                    class Blue
                    {
                    }

                    class Green
                    {
                    }
                }
            ";
            TestObjects test = new TestObjects(code);

            // When
            test.Module.Execute(new[] { test.Document }, test.Context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new [] { "Red" }, 
                test.GetMetadata<IEnumerable<IDocument>>(x => (string)x["Name"] == "Foo", "Types").Select(x => x.Get("Name")));
            CollectionAssert.AreEquivalent(new[] { "Blue", "Green" },
                test.GetMetadata<IEnumerable<IDocument>>(x => (string)x["Name"] == "Bar", "Types").Select(x => x.Get("Name")));
            test.Stream.Dispose();
        }

        [Test]
        public void NamespacesContainNestedTypes()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    class Blue
                    {
                        class Green
                        {
                        }
                    }
                }
            ";
            TestObjects test = new TestObjects(code);

            // When
            test.Module.Execute(new[] { test.Document }, test.Context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new[] { "Blue", "Green" },
                test.GetMetadata<IEnumerable<IDocument>>(x => (string)x["Name"] == "Foo", "Types").Select(x => x.Get("Name")));
            test.Stream.Dispose();
        }

        // Document property only implements GetEnumerator() and Get(string, object)
        private class TestObjects
        {
            public IDocument Document { get; } = Substitute.For<IDocument>();
            public IExecutionContext Context { get; } = Substitute.For<IExecutionContext>();
            public MemoryStream Stream { get; }
            public List<Dictionary<string, object>> Metadata { get; } = new List<Dictionary<string, object>>();
            public IModule Module { get; } = new AnalyzeCSharp();

            public TestObjects(string code)
            {
                Stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                Document.GetStream().Returns(Stream);
                Context.GetNewDocument(Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x =>
                    {
                        IDocument newDocument = Substitute.For<IDocument>();
                        newDocument.GetEnumerator().Returns(((IEnumerable<KeyValuePair<string, object>>)x[0]).GetEnumerator());
                        newDocument.Get(Arg.Any<string>(), Arg.Any<object>()).Returns(y =>
                        {
                            KeyValuePair<string, object> kvp = ((IEnumerable<KeyValuePair<string, object>>)x[0]).FirstOrDefault(z => z.Key == (string)y[0]);
                            return kvp.Equals(default(KeyValuePair<string, object>)) ? y[1] : kvp.Value;
                        });
                        Metadata.Add(((IEnumerable<KeyValuePair<string, object>>)x[0]).ToDictionary(y => y.Key, y => y.Value));
                        return newDocument;
                    });
            }

            // Gets a specific metadata value from the metadata collection
            public T GetMetadata<T>(Func<Dictionary<string, object>, bool> metadataSelector, string metadataKey)
            {
                return (T)Metadata.Single(metadataSelector)[metadataKey];
            }

            // Gets metadata Name values for children given a parent IDocument and a metadata key containing an IEnumerable<IDocument>
            public IEnumerable<T> GetChildMetadata<T>(Func<Dictionary<string, object>, bool> parentSelector, string documentsKey, string childKey)
            {
                return GetMetadata<IEnumerable<IDocument>>(parentSelector, documentsKey)
                    .Select(x => x.Get(childKey))
                    .Cast<T>();
            }
        }
    }

}
