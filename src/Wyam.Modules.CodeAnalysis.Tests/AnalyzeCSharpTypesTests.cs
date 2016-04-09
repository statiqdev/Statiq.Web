using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.Modules.CodeAnalysis.Tests
{
    [TestFixture]
    public class AnalyzeCSharpTypesTests : AnalyzeCSharpBaseFixture
    {
        public class ExecuteMethodTests : AnalyzeCSharpTypesTests
        {
            [Test]
            public void ReturnsAllTypes()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                        }

                        class Green
                        {
                            class Red
                            {
                            }
                        }

                        internal struct Yellow
                        {
                        }

                        enum Orange
                        {
                        }
                    }
                ";
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(stream);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.InputFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.GetDocument(Arg.Any<FilePath>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<MetadataItem>)x[2]));
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new[] { string.Empty, "Foo", "Blue", "Green", "Red", "Yellow", "Orange" }, results.Select(x => x["Name"]));
                stream.Dispose();
            }

            [Test]
            public void MemberTypesReturnsNestedTypes()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            public class Blue
                            {
                            }

                            private struct Red
                            {
                            }

                            enum Yellow
                            {
                            }
                        }
                    }
                ";
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(stream);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.InputFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.GetDocument(Arg.Any<FilePath>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<MetadataItem>)x[2]));
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new[] { "Blue", "Red", "Yellow" }, 
                    results.Single(x => x["Name"].Equals("Green")).Get<IEnumerable<IDocument>>("MemberTypes").Select(x => x["Name"]));
                stream.Dispose();
            }

            [Test]
            public void FullNameContainsContainingType()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        public class Green
                        {
                            private class Blue
                            {
                            }
                        }

                        struct Red
                        {
                        }
                    }

                    namespace Foo.Bar
                    {
                        enum Yellow
                        {
                        }
                    }
                ";
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(stream);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.InputFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.GetDocument(Arg.Any<FilePath>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<MetadataItem>)x[2]));
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new[] { string.Empty, "Foo", "Green", "Green.Blue", "Red", "Yellow", "Bar" }, results.Select(x => x["FullName"]));
                stream.Dispose();
            }

            [Test]
            public void DisplayNameContainsContainingType()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        public class Green
                        {
                            private class Blue
                            {
                            }
                        }

                        struct Red
                        {
                        }
                    }

                    namespace Foo.Bar
                    {
                        enum Yellow
                        {
                        }
                    }
                ";
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(stream);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.InputFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.GetDocument(Arg.Any<FilePath>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<MetadataItem>)x[2]));
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new[] { "global", "Foo", "Green", "Green.Blue", "Red", "Yellow", "Foo.Bar" }, results.Select(x => x["DisplayName"]));
                stream.Dispose();
            }

            [Test]
            public void QualifiedNameContainsNamespaceAndContainingType()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        public class Green
                        {
                            private class Blue
                            {
                            }
                        }

                        struct Red
                        {
                        }
                    }

                    namespace Foo.Bar
                    {
                        enum Yellow
                        {
                        }
                    }
                ";
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(stream);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.InputFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.GetDocument(Arg.Any<FilePath>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<MetadataItem>)x[2]));
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new[] { String.Empty, "Foo", "Foo.Green", "Foo.Green.Blue", "Foo.Red", "Foo.Bar.Yellow", "Foo.Bar" }, results.Select(x => x["QualifiedName"]));
                stream.Dispose();
            }

            [Test]
            public void ContainingNamespaceIsCorrect()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            class Blue
                            {
                            }
                        }

                        struct Red
                        {
                        }
                    }

                    namespace Foo.Bar
                    {
                        enum Yellow
                        {
                        }
                    }
                ";
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(stream);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.InputFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.GetDocument(Arg.Any<FilePath>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<MetadataItem>)x[2]));
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.AreEqual("Foo", results.Single(x => x["Name"].Equals("Green")).Get<IDocument>("ContainingNamespace")["Name"]);
                Assert.AreEqual("Foo", results.Single(x => x["Name"].Equals("Blue")).Get<IDocument>("ContainingNamespace")["Name"]);
                Assert.AreEqual("Foo", results.Single(x => x["Name"].Equals("Red")).Get<IDocument>("ContainingNamespace")["Name"]);
                Assert.AreEqual("Bar", results.Single(x => x["Name"].Equals("Yellow")).Get<IDocument>("ContainingNamespace")["Name"]);
                stream.Dispose();
            }

            [Test]
            public void ContainingTypeIsCorrect()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            class Blue
                            {
                            }
                        }

                        struct Red
                        {
                        }
                    }

                    namespace Foo.Bar
                    {
                        enum Yellow
                        {
                        }
                    }
                ";
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(stream);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.InputFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.GetDocument(Arg.Any<FilePath>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<MetadataItem>)x[2]));
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.IsNull(results.Single(x => x["Name"].Equals("Green")).Get<IDocument>("ContainingType"));
                Assert.AreEqual("Green", results.Single(x => x["Name"].Equals("Blue")).Get<IDocument>("ContainingType")["Name"]);
                Assert.IsNull(results.Single(x => x["Name"].Equals("Red")).Get<IDocument>("ContainingType"));
                Assert.IsNull(results.Single(x => x["Name"].Equals("Yellow")).Get<IDocument>("ContainingType"));
                stream.Dispose();
            }

            [Test]
            public void KindIsCorrect()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        public class Green
                        {
                            private class Blue
                            {
                            }
                        }

                        struct Red
                        {
                        }
                    }

                    namespace Foo.Bar
                    {
                        enum Yellow
                        {
                        }
                    }
                ";
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(stream);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.InputFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.GetDocument(Arg.Any<FilePath>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<MetadataItem>)x[2]));
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.AreEqual("NamedType", results.Single(x => x["Name"].Equals("Green"))["Kind"]);
                Assert.AreEqual("NamedType", results.Single(x => x["Name"].Equals("Blue"))["Kind"]);
                Assert.AreEqual("NamedType", results.Single(x => x["Name"].Equals("Red"))["Kind"]);
                Assert.AreEqual("NamedType", results.Single(x => x["Name"].Equals("Yellow"))["Kind"]);
                stream.Dispose();
            }

            [Test]
            public void SpecificKindIsCorrect()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        public class Green
                        {
                            private class Blue
                            {
                            }
                        }

                        struct Red
                        {
                        }
                    }

                    namespace Foo.Bar
                    {
                        enum Yellow
                        {
                        }
                    }
                ";
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(stream);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.InputFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.GetDocument(Arg.Any<FilePath>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<MetadataItem>)x[2]));
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.AreEqual("Class", results.Single(x => x["Name"].Equals("Green"))["SpecificKind"]);
                Assert.AreEqual("Class", results.Single(x => x["Name"].Equals("Blue"))["SpecificKind"]);
                Assert.AreEqual("Struct", results.Single(x => x["Name"].Equals("Red"))["SpecificKind"]);
                Assert.AreEqual("Enum", results.Single(x => x["Name"].Equals("Yellow"))["SpecificKind"]);
                stream.Dispose();
            }

            [Test]
            public void BaseTypeIsCorrect()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        public class Red
                        {
                        }

                        public class Green : Red
                        {
                        }

                        struct Blue
                        {
                        }

                        interface Yellow
                        {
                        }

                        interface Purple : Yellow
                        {
                        }

                        enum Orange
                        {
                        }
                    }
                ";
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(stream);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.InputFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.GetDocument(Arg.Any<FilePath>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<MetadataItem>)x[2]));
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.AreEqual("Object", results.Single(x => x["Name"].Equals("Red")).Get<IDocument>("BaseType")["Name"]);
                Assert.AreEqual("Red", results.Single(x => x["Name"].Equals("Green")).Get<IDocument>("BaseType")["Name"]);
                Assert.AreEqual("ValueType", results.Single(x => x["Name"].Equals("Blue")).Get<IDocument>("BaseType")["Name"]);
                Assert.IsNull(results.Single(x => x["Name"].Equals("Yellow")).Get<IDocument>("BaseType"));
                Assert.IsNull(results.Single(x => x["Name"].Equals("Purple")).Get<IDocument>("BaseType"));
                Assert.AreEqual("Enum", results.Single(x => x["Name"].Equals("Orange")).Get<IDocument>("BaseType")["Name"]);
                stream.Dispose();
            }

            [Test]
            public void MembersReturnsAllMembersExceptConstructors()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                            public Blue()
                            {
                            }

                            void Green()
                            {
                            }

                            int Red { get; }

                            string _yellow;

                            event ChangedEventHandler Changed;
                        }
                    }
                ";
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(stream);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.InputFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.GetDocument(Arg.Any<FilePath>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<MetadataItem>)x[2]));
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new[] { "Green", "Red", "_yellow", "Changed" },
                    GetResult(results, "Blue").Get<IReadOnlyList<IDocument>>("Members").Select(x => x["Name"]));
                stream.Dispose();
            }

            [Test]
            public void ConstructorsIsPopulated()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                            public Blue()
                            {
                            }

                            protected Blue(int x)
                            {
                            }

                            void Green()
                            {
                            }

                            int Red { get; }

                            string _yellow;

                            event ChangedEventHandler Changed;
                        }
                    }
                ";
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(stream);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.InputFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.GetDocument(Arg.Any<FilePath>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<MetadataItem>)x[2]));
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.AreEqual(2, GetResult(results, "Blue").Get<IReadOnlyList<IDocument>>("Constructors").Count);
                stream.Dispose();
            }

            [Test]
            public void WritePathIsCorrect()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        class Red
                        {
                        }

                        enum Green
                        {
                        }

                        namespace Bar
                        {
                            struct Blue
                            {
                            }
                        }
                    }

                    class Yellow
                    {
                    }
                ";
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(stream);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.InputFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.GetDocument(Arg.Any<FilePath>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<MetadataItem>)x[2]));
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new[] { "Foo\\414E2165\\index.html", "Foo.Bar\\92C5B5C5\\index.html", "global\\439037DE\\index.html", "Foo\\53AB53EF\\index.html" },
                    results.Where(x => x["Kind"].Equals("NamedType")).Select(x => x[Keys.WritePath]));
                stream.Dispose();
            }

            [Test]
            public void GetDocumentForExternalBaseType()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        class Red
                        {
                        }
                    }
                ";
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(stream);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.InputFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.GetDocument(Arg.Any<FilePath>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<MetadataItem>)x[2]));
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.AreEqual("Object", GetResult(results, "Red").Get<IDocument>("BaseType")["Name"]);
                stream.Dispose();
            }

            [Test]
            public void GetDocumentsForExternalInterfaces()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        class Red : IBlue, IFoo
                        {
                        }

                        interface IBlue
                        {
                        }
                    }
                ";
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(stream);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.InputFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.GetDocument(Arg.Any<FilePath>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<MetadataItem>)x[2]));
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new[] { string.Empty, "Foo", "Red", "IBlue" }, results.Select(x => x["Name"]));
                CollectionAssert.AreEquivalent(new [] { "IBlue", "IFoo" }, GetResult(results, "Red").Get<IEnumerable<IDocument>>("AllInterfaces").Select(x => x["Name"]));
                stream.Dispose();
            }

            [Test]
            public void GetDerivedTypes()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        class Red : IBlue, IFoo
                        {
                        }

                        class Blue : Red, IBlue
                        {
                        }

                        class Green : Blue
                        {
                        }

                        class Yellow : Blue
                        {
                        }

                        interface IBlue
                        {
                        }
                    }
                ";
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(stream);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.InputFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.GetDocument(Arg.Any<FilePath>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<MetadataItem>)x[2]));
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new[] { "Blue" }, GetResult(results, "Red").Get<IEnumerable<IDocument>>("DerivedTypes").Select(x => x["Name"]));
                CollectionAssert.AreEquivalent(new[] { "Green", "Yellow" }, GetResult(results, "Blue").Get<IEnumerable<IDocument>>("DerivedTypes").Select(x => x["Name"]));
                CollectionAssert.IsEmpty(GetResult(results, "Green").Get<IEnumerable<IDocument>>("DerivedTypes"));
                CollectionAssert.IsEmpty(GetResult(results, "Yellow").Get<IEnumerable<IDocument>>("DerivedTypes"));
                CollectionAssert.IsEmpty(GetResult(results, "IBlue").Get<IEnumerable<IDocument>>("DerivedTypes"));
                stream.Dispose();
            }

            [Test]
            public void GetTypeParams()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        class Red<T>
                        {
                        }

                        interface IBlue<TKey, TValue>
                        {
                        }
                    }
                ";
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(stream);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.InputFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.GetDocument(Arg.Any<FilePath>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<MetadataItem>)x[2]));
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEqual(new[] { "T" }, GetResult(results, "Red").Get<IEnumerable<IDocument>>("TypeParameters").Select(x => x["Name"]));
                CollectionAssert.AreEqual(new[] { "TKey", "TValue" }, GetResult(results, "IBlue").Get<IEnumerable<IDocument>>("TypeParameters").Select(x => x["Name"]));
                stream.Dispose();
            }

            [Test]
            public void TypeParamReferencesClass()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        class Red<T>
                        {
                        }
                    }
                ";
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(stream);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.InputFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.GetDocument(Arg.Any<FilePath>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<MetadataItem>)x[2]));
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.AreEqual("Red", GetResult(results, "Red").Get<IEnumerable<IDocument>>("TypeParameters").First().Get<IDocument>("DeclaringType")["Name"]);
                stream.Dispose();
            }

            [Test]
            public void TypesExcludedByPredicate()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                        }

                        class Green
                        {
                            class Red
                            {
                            }
                        }

                        internal struct Yellow
                        {
                        }

                        enum Orange
                        {
                        }
                    }
                ";
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(stream);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.InputFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.GetDocument(Arg.Any<FilePath>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<MetadataItem>)x[2]));
                IModule module = new AnalyzeCSharp().WhereSymbol(x => x is INamedTypeSymbol && ((INamedTypeSymbol)x).TypeKind == TypeKind.Class);

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new[] { "Blue", "Green", "Red" }, results.Select(x => x["Name"]));
                stream.Dispose();
            }

            [Test]
            public void RestrictedToNamedTypes()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                            public void Foo()
                            {
                            }
                        }

                        public interface Red
                        {
                        }
                    }
                ";
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(stream);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.InputFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.GetDocument(Arg.Any<FilePath>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<MetadataItem>)x[2]));
                IModule module = new AnalyzeCSharp().WithNamedTypes();

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new[] { "Blue", "Red" }, results.Select(x => x["Name"]));
                stream.Dispose();
            }

            [Test]
            public void RestrictedToNamedTypesWithPredicate()
            {
                // Given
                string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                            public void Foo()
                            {
                            }
                        }

                        public interface Red
                        {
                        }
                    }
                ";
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
                IDocument document = Substitute.For<IDocument>();
                document.GetStream().Returns(stream);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.InputFolder.Returns(TestContext.CurrentContext.TestDirectory);
                context.GetDocument(Arg.Any<FilePath>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                    .Returns(x => new TestDocument((IEnumerable<MetadataItem>)x[2]));
                IModule module = new AnalyzeCSharp().WithNamedTypes(x => x.TypeKind == TypeKind.Class);

                // When
                List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(new[] { "Blue" }, results.Select(x => x["Name"]));
                stream.Dispose();
            }
        }
    }
}