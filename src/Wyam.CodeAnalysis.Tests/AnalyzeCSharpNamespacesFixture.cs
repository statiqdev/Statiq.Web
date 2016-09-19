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
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.CodeAnalysis.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class AnalyzeCSharpNamespacesFixture : AnalyzeCSharpBaseFixture
    {
        public class ExecuteTests : AnalyzeCSharpNamespacesFixture
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
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new [] { string.Empty, "Foo", "Bar" }, results.Select(x => x["Name"]));
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
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new[] { string.Empty, "Foo", "Baz", "Bar" }, results.Select(x => x["Name"]));
                CollectionAssert.AreEquivalent(new [] { "Foo", "Bar" }, 
                    results.Single(x => x["Name"].Equals(string.Empty)).Get<IEnumerable<IDocument>>("MemberNamespaces").Select(x => x["Name"]));
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
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new[] { string.Empty, "Foo", "Baz", "Bar" }, results.Select(x => x["Name"]));
                CollectionAssert.AreEquivalent(new[] { "Baz", "Bar" },
                    results.Single(x => x["Name"].Equals("Foo")).Get<IEnumerable<IDocument>>("MemberNamespaces").Select(x => x["Name"]));
            }

            [Test]
            public void FullNameDoesNotContainFullHierarchy()
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
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new[] { string.Empty, "Foo", "Bar" }, results.Select(x => x["FullName"]));
            }

            [Test]
            public void QualifiedNameContainsFullHierarchy()
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
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new[] { string.Empty, "Foo", "Foo.Bar" }, results.Select(x => x["QualifiedName"]));
            }

            [Test]
            public void DisplayNameContainsFullHierarchy()
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
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new[] { "global", "Foo", "Foo.Bar" }, results.Select(x => x["DisplayName"]));
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
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new[] { "Namespace", "Namespace", "Namespace" }, results.Select(x => x["Kind"]));
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
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.AreEqual("Foo", results.Single(x => x["Name"].Equals("Bar")).Get<IDocument>("ContainingNamespace")["Name"]);
                Assert.AreEqual(string.Empty, results.Single(x => x["Name"].Equals("Foo")).Get<IDocument>("ContainingNamespace")["Name"]);
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
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new [] { "Red" }, 
                    results.Single(x => x["Name"].Equals("Foo")).Get<IEnumerable<IDocument>>("MemberTypes").Select(x => x["Name"]));
                CollectionAssert.AreEquivalent(new[] { "Blue", "Green" },
                    results.Single(x => x["Name"].Equals("Bar")).Get<IEnumerable<IDocument>>("MemberTypes").Select(x => x["Name"]));
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
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document },context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new[] { "Blue" }, 
                    results.Single(x => x["Name"].Equals("Foo")).Get<IEnumerable<IDocument>>("MemberTypes").Select(x => x["Name"]));
            }

            [Test]
            public void WritePathIsCorrect()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        namespace Bar
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new[] { "global/index.html", "Foo/index.html", "Foo.Bar/index.html" },
                    results.Where(x => x["Kind"].Equals("Namespace")).Select(x => ((FilePath)x[Keys.WritePath]).FullPath));
            }
        }
    }
}
