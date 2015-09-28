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
    public class AnalyzeCSharpXmlDocumentationFixture
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
            Assert.AreEqual("<div class=\"doc-summary\">This is a summary.</div>", results.Single(x => x["Name"].Equals("Green"))["SummaryHtml"]);
            Assert.AreEqual("<div class=\"doc-summary\">This is another summary.</div>", results.Single(x => x["Name"].Equals("Red"))["SummaryHtml"]);
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
            Assert.AreEqual("<div class=\"doc-summary\">\n    This is a summary.\n    </div>", results.Single(x => x["Name"].Equals("Green"))["SummaryHtml"]);
            Assert.AreEqual("<div class=\"doc-summary\">\n    This is\n    another summary.\n    </div>", results.Single(x => x["Name"].Equals("Red"))["SummaryHtml"]);
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
            Assert.AreEqual("<div class=\"doc-summary\">This is a summary.</div>\n<div class=\"doc-summary\">This is another summary.</div>", results.Single(x => x["Name"].Equals("Green"))["SummaryHtml"]);
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
            Assert.AreEqual(string.Empty, results.Single(x => x["Name"].Equals("Green"))["SummaryHtml"]);
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
            Assert.AreEqual("<div class=\"doc-summary\">\n    This is <code>some code</code> in a summary.\n    </div>", results.Single(x => x["Name"].Equals("Green"))["SummaryHtml"]);
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
            Assert.AreEqual("<div class=\"doc-summary\">\n    This is\n    <pre><code>\n    with some code\n    </code></pre>\n    a summary\n    </div>", results.Single(x => x["Name"].Equals("Green"))["SummaryHtml"]);
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
            Assert.AreEqual("<div class=\"doc-summary\">\n    This is <code>some code</code> and\n    <pre><code>\n    with some code\n    </code></pre>\n    a summary\n    </div>", results.Single(x => x["Name"].Equals("Green"))["SummaryHtml"]);
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
            string test = results.Single(x => x["Name"].Equals("Green")).Get<IEnumerable<IDocument>>("Members").Single(x => x["Name"].Equals("Go"))["ExceptionHtml"].ToString();
            Assert.AreEqual("", results.Single(x => x["Name"].Equals("Green")).Get<IEnumerable<IDocument>>("Members").Single(x => x["Name"].Equals("Go"))["ExceptionHtml"]);
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
            Assert.AreEqual("", results.Single(x => x["Name"].Equals("Green")).Get<IEnumerable<IDocument>>("Members").Single(x => x["Name"].Equals("Go"))["SummaryHtml"]);
            stream.Dispose();
        }

        // TODO: ExceptionWithoutCrefAttribute
        // TODO: MultipleExceptions
    }
}
