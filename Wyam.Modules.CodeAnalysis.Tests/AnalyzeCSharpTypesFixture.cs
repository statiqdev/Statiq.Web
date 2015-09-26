using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common;

namespace Wyam.Modules.CodeAnalysis.Tests
{
    [TestFixture]
    public class AnalyzeCSharpTypesFixture
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
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
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
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new[] { "Blue", "Red", "Yellow" }, 
                results.Single(x => x["Name"].Equals("Green")).Get<IEnumerable<IDocument>>("MemberTypes").Select(x => x["Name"]));
            stream.Dispose();
        }

        [Test]
        public void DisplayStringContainsNamespaceAndContainingType()
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
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new[] { "<global namespace>", "Foo", "Foo.Green", "Foo.Green.Blue", "Foo.Red", "Foo.Bar.Yellow", "Foo.Bar" }, results.Select(x => x["DisplayString"]));
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
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
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
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
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
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
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
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
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

                    enum Orange
                    {
                    }
                }
            ";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.IsNull(results.Single(x => x["Name"].Equals("Red")).Get<IDocument>("BaseType"));
            Assert.AreEqual("Red", results.Single(x => x["Name"].Equals("Green")).Get<IDocument>("BaseType")["Name"]);
            Assert.IsNull(results.Single(x => x["Name"].Equals("Blue")).Get<IDocument>("BaseType"));
            Assert.IsNull(results.Single(x => x["Name"].Equals("Yellow")).Get<IDocument>("BaseType"));
            Assert.IsNull(results.Single(x => x["Name"].Equals("Orange")).Get<IDocument>("BaseType"));
            stream.Dispose();
        }
    }
}
