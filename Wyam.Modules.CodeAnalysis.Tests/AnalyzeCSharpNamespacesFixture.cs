using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
            List<IDocument> results = test.Module.Execute(new[] { test.Document }, test.Context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new [] { string.Empty, "Foo", "Bar" }, results.Select(x => x["Name"]));
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
            List<IDocument> results = test.Module.Execute(new[] { test.Document }, test.Context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new[] { string.Empty, "Foo", "Baz", "Bar" }, results.Select(x => x["Name"]));
            CollectionAssert.AreEquivalent(new [] { "Foo", "Bar" }, 
                results.Single(x => x["Name"].Equals(string.Empty)).Get<IEnumerable<IDocument>>("MemberNamespaces").Select(x => x["Name"]));
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
            List<IDocument> results = test.Module.Execute(new[] { test.Document }, test.Context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new[] { string.Empty, "Foo", "Baz", "Bar" }, results.Select(x => x["Name"]));
            CollectionAssert.AreEquivalent(new[] { "Baz", "Bar" },
                results.Single(x => x["Name"].Equals("Foo")).Get<IEnumerable<IDocument>>("MemberNamespaces").Select(x => x["Name"]));
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
            List<IDocument> results = test.Module.Execute(new[] { test.Document }, test.Context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new[] { "<global namespace>", "Foo", "Foo.Bar" }, results.Select(x => x["DisplayString"]));
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
            List<IDocument> results = test.Module.Execute(new[] { test.Document }, test.Context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new[] { "Namespace", "Namespace", "Namespace" }, results.Select(x => x["Kind"]));
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
            List<IDocument> results = test.Module.Execute(new[] { test.Document }, test.Context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual("Foo", results.Single(x => x["Name"].Equals("Bar")).Get<IDocument>("ContainingNamespace")["Name"]);
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
            List<IDocument> results = test.Module.Execute(new[] { test.Document }, test.Context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new [] { "Red" }, 
                results.Single(x => x["Name"].Equals("Foo")).Get<IEnumerable<IDocument>>("MemberTypes").Select(x => x["Name"]));
            CollectionAssert.AreEquivalent(new[] { "Blue", "Green" },
                results.Single(x => x["Name"].Equals("Bar")).Get<IEnumerable<IDocument>>("MemberTypes").Select(x => x["Name"]));
            test.Stream.Dispose();
        }

        [Test]
        public void NamespacesDoNotContainNestedTypes()
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
            List<IDocument> results = test.Module.Execute(new[] { test.Document }, test.Context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new[] { "Blue" }, 
                results.Single(x => x["Name"].Equals("Foo")).Get<IEnumerable<IDocument>>("MemberTypes").Select(x => x["Name"]));
            test.Stream.Dispose();
        }

        // Document property only implements GetEnumerator() and Get(string, object)
        private class TestObjects
        {
            public IDocument Document { get; } = Substitute.For<IDocument>();
            public IExecutionContext Context { get; } = Substitute.For<IExecutionContext>();
            public MemoryStream Stream { get; }
            public IModule Module { get; } = new AnalyzeCSharp();

            public TestObjects(string code)
            {
                Stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                Document.GetStream().Returns(Stream);
                Context.GetNewDocument(Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[0]));
            }
        }
    }

}
