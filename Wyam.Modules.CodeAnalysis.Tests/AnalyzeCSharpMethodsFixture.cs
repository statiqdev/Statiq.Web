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
    public class AnalyzeCSharpMethodsFixture : AnalyzeCSharpFixtureBase
    {
        [Test]
        public void ClassMembersContainsMethods()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    public class Blue
                    {
                        void Green()
                        {
                        }

                        void Red()
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
            CollectionAssert.AreEquivalent(new[] { "Green", "Red" }, 
                GetResult(results, "Blue").Get<IReadOnlyList<IDocument>>("Members").Select(x => x["Name"]));
            stream.Dispose();
        }

        [Test]
        public void ContainingTypeIsCorrect()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    public class Blue
                    {
                        void Green()
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
            Assert.AreEqual("Blue", GetMember(results, "Blue", "Green").Get<IDocument>("ContainingType")["Name"]);
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
                        void X()
                        {
                        }
                    }
                }

                class Yellow
                {
                    void Y<T>()
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
            CollectionAssert.AreEquivalent(new[] { "439037DE\\66F23CDD.html", "Foo\\414E2165\\A94FD382.html" },
                results.Where(x => x["Kind"].Equals("Method")).Select(x => x["WritePath"]));
            stream.Dispose();
        }

        [Test]
        public void DisplayNameIsCorrect()
        {
            // Given
            string code = @"
                class Yellow
                {
                    public X()
                    {
                    }

                    void Y<T>(T a, int b)
                    {
                    }

                    Z(bool a)
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
            Assert.AreEqual("X()", GetMember(results, "Yellow", "X")["DisplayName"]);
            Assert.AreEqual("Y<T>(T, int)", GetMember(results, "Yellow", "Y")["DisplayName"]);
            Assert.AreEqual("Z(bool)", GetMember(results, "Yellow", "Z")["DisplayName"]);
            stream.Dispose();
        }

        [Test]
        public void ReturnTypeIsCorrect()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    public class Blue
                    {
                        int Green()
                        {
                            return 0;
                        }

                        Red GetRed()
                        {
                            return new Red();
                        }

                        TFoo Bar<TFoo>()
                        {
                            return default(TFoo);
                        }
                    }

                    public class Red
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
            Assert.AreEqual("int", GetMember(results, "Blue", "Green").Get<IDocument>("ReturnType")["DisplayName"]);
            Assert.AreEqual("Red", GetMember(results, "Blue", "GetRed").Get<IDocument>("ReturnType")["DisplayName"]);
            Assert.AreEqual("TFoo", GetMember(results, "Blue", "Bar").Get<IDocument>("ReturnType")["DisplayName"]);
            stream.Dispose();
        }
    }
}
