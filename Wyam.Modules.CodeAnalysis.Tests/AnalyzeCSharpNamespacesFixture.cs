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
                GetChildMetadata<string>(test.Metadata, x => (string)x["Name"] == string.Empty, "NestedNamespaces", "Name"));
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
                GetChildMetadata<string>(test.Metadata, x => (string)x["Name"] == "Foo", "NestedNamespaces", "Name"));
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
                        Metadata.Add(((IEnumerable<KeyValuePair<string, object>>)x[0]).ToDictionary(y => y.Key, y => y.Value));
                        return newDocument;
                    });
            }
        }

        // Gets metadata Name values for children given a parent IDocument and a metadata key containing an IEnumerable<IDocument>
        private IEnumerable<T> GetChildMetadata<T>(List<Dictionary<string, object>> metadata,
            Func<Dictionary<string, object>, bool> parentSelector, string documentsKey, string childKey)
        {
            return ((IEnumerable<IDocument>)(metadata.Single(parentSelector)[documentsKey]))
                .Select(x => x.First(y => y.Key == childKey).Value)
                .Cast<T>();
        } 
    }

}
