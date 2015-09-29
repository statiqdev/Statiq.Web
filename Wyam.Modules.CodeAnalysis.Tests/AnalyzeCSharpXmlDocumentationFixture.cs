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
    public class AnalyzeCSharpXmlDocumentationFixture : AnalyzeCSharpFixtureBase
    {
        [Test]
        public void SingleLineSummary()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <summary>This is a summary.</summary>
                    class Green
                    {
                    }

                    /// <summary>This is another summary.</summary>
                    struct Red
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
            Assert.AreEqual("This is a summary.", GetClass(results, "Green").Get<IReadOnlyList<string>>("SummaryHtml")[0]);
            Assert.AreEqual("This is another summary.", GetClass(results, "Red").Get<IReadOnlyList<string>>("SummaryHtml")[0]);
            stream.Dispose();
        }

        [Test]
        public void MultiLineSummary()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <summary>
                    /// This is a summary.
                    /// </summary>
                    class Green
                    {
                    }

                    /// <summary>
                    /// This is
                    /// another summary.
                    /// </summary>
                    struct Red
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
            Assert.AreEqual("\n    This is a summary.\n    ", GetClass(results, "Green").Get<IReadOnlyList<string>>("SummaryHtml")[0]);
            Assert.AreEqual("\n    This is\n    another summary.\n    ", GetClass(results, "Red").Get<IReadOnlyList<string>>("SummaryHtml")[0]);
            stream.Dispose();
        }

        [Test]
        public void MultipleSummaryElements()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <summary>This is a summary.</summary>
                    /// <summary>This is another summary.</summary>
                    class Green
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
            Assert.AreEqual(2, GetClass(results, "Green").Get<IReadOnlyList<string>>("SummaryHtml").Count);
            Assert.AreEqual("This is a summary.", GetClass(results, "Green").Get<IReadOnlyList<string>>("SummaryHtml")[0]);
            Assert.AreEqual("This is another summary.", GetClass(results, "Green").Get<IReadOnlyList<string>>("SummaryHtml")[1]);
            stream.Dispose();
        }

        [Test]
        public void NoSummary()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    class Green
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
            CollectionAssert.IsEmpty(GetClass(results, "Green").Get<IReadOnlyList<string>>("SummaryHtml"));
            stream.Dispose();
        }

        [Test]
        public void SummaryWithCElement()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <summary>
                    /// This is <c>some code</c> in a summary.
                    /// </summary>
                    class Green
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
            Assert.AreEqual("\n    This is <code>some code</code> in a summary.\n    ", 
                GetClass(results, "Green").Get<IReadOnlyList<string>>("SummaryHtml")[0]);
            stream.Dispose();
        }

        [Test]
        public void SummaryWithCodeElement()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <summary>
                    /// This is
                    /// <code>
                    /// with some code
                    /// </code>
                    /// a summary
                    /// </summary>
                    class Green
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
            Assert.AreEqual("\n    This is\n    <pre><code>\n    with some code\n    </code></pre>\n    a summary\n    ", 
                GetClass(results, "Green").Get<IReadOnlyList<string>>("SummaryHtml")[0]);
            stream.Dispose();
        }

        [Test]
        public void SummaryWithCodeElementAndCElement()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <summary>
                    /// This is <c>some code</c> and
                    /// <code>
                    /// with some code
                    /// </code>
                    /// a summary
                    /// </summary>
                    class Green
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
            Assert.AreEqual("\n    This is <code>some code</code> and\n    <pre><code>\n    with some code\n    </code></pre>\n    a summary\n    ",
                GetClass(results, "Green").Get<IReadOnlyList<string>>("SummaryHtml")[0]);
            stream.Dispose();
        }
        
        [Test]
        public void MethodWithExceptionElement()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    class Green
                    {
                        /// <exception cref=""FooException"">Throws when null</exception>
                        void Go()
                        {
                        }
                    }

                    class FooException : Exception
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
            Assert.AreEqual("<a href=\"/Foo/6412642C.html\">FooException</a>",
                GetMember(results, "Green", "Go").Get<IReadOnlyList<KeyValuePair<string, string>>>("ExceptionHtml")[0].Key);
            Assert.AreEqual("Throws when null",
                GetMember(results, "Green", "Go").Get<IReadOnlyList<KeyValuePair<string, string>>>("ExceptionHtml")[0].Value);
            stream.Dispose();
        }

        [Test]
        public void MethodWithUnknownExceptionElement()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    class Green
                    {
                        /// <exception cref=""FooException"">Throws when null</exception>
                        void Go()
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
            Assert.AreEqual("FooException",
                GetMember(results, "Green", "Go").Get<IReadOnlyList<KeyValuePair<string, string>>>("ExceptionHtml")[0].Key);
            Assert.AreEqual("Throws when null",
                GetMember(results, "Green", "Go").Get<IReadOnlyList<KeyValuePair<string, string>>>("ExceptionHtml")[0].Value);
            stream.Dispose();
        }

        [Test]
        public void ExceptionElementWithoutCref()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    class Green
                    {
                        /// <exception>Throws when null</exception>
                        void Go()
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
            Assert.AreEqual(string.Empty,
                GetMember(results, "Green", "Go").Get<IReadOnlyList<KeyValuePair<string, string>>>("ExceptionHtml")[0].Key);
            Assert.AreEqual("Throws when null",
                GetMember(results, "Green", "Go").Get<IReadOnlyList<KeyValuePair<string, string>>>("ExceptionHtml")[0].Value);
            stream.Dispose();
        }

        [Test]
        public void MultipleExceptionElements()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    class Green
                    {
                        /// <exception cref=""FooException"">Throws when null</exception>
                        /// <exception cref=""BarException"">Throws for another reason</exception>
                        void Go()
                        {
                        }
                    }

                    class FooException : Exception
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
            Assert.AreEqual(2, GetMember(results, "Green", "Go").Get<IReadOnlyList<KeyValuePair<string, string>>>("ExceptionHtml").Count);
            Assert.AreEqual("<a href=\"/Foo/6412642C.html\">FooException</a>",
                GetMember(results, "Green", "Go").Get<IReadOnlyList<KeyValuePair<string, string>>>("ExceptionHtml")[0].Key);
            Assert.AreEqual("Throws when null",
                GetMember(results, "Green", "Go").Get<IReadOnlyList<KeyValuePair<string, string>>>("ExceptionHtml")[0].Value);
            Assert.AreEqual("BarException",
                GetMember(results, "Green", "Go").Get<IReadOnlyList<KeyValuePair<string, string>>>("ExceptionHtml")[1].Key);
            Assert.AreEqual("Throws for another reason",
                GetMember(results, "Green", "Go").Get<IReadOnlyList<KeyValuePair<string, string>>>("ExceptionHtml")[1].Value);
            stream.Dispose();
        }
    }
}
