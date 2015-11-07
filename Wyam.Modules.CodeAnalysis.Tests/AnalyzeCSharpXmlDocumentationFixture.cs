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
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

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
            Assert.AreEqual("This is a summary.", GetResult(results, "Green")["Summary"]);
            Assert.AreEqual("This is another summary.", GetResult(results, "Red")["Summary"]);
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
            Assert.AreEqual("\n    This is a summary.\n    ", GetResult(results, "Green")["Summary"]);
            Assert.AreEqual("\n    This is\n    another summary.\n    ", GetResult(results, "Red")["Summary"]);
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
            Assert.AreEqual("This is a summary.\nThis is another summary.", GetResult(results, "Green")["Summary"]);
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
            Assert.AreEqual(string.Empty, GetResult(results, "Green")["Summary"]);
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
            Assert.AreEqual("\n    This is <code>some code</code> in a summary.\n    ", GetResult(results, "Green")["Summary"]);
            stream.Dispose();
        }

        [Test]
        public void SummaryWithCElementAndInlineCssClass()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <summary>
                    /// This is <c class=""code"">some code</c> in a summary.
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
            Assert.AreEqual("\n    This is <code class=\"code\">some code</code> in a summary.\n    ", GetResult(results, "Green")["Summary"]);
            stream.Dispose();
        }

        [Test]
        public void SummaryWithCElementAndDeclaredCssClass()
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
            IModule module = new AnalyzeCSharp().WithCssClasses("code", "code");

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual("\n    This is <code class=\"code\">some code</code> in a summary.\n    ", GetResult(results, "Green")["Summary"]);
            stream.Dispose();
        }

        [Test]
        public void SummaryWithCElementAndInlineAndDeclaredCssClasses()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <summary>
                    /// This is <c class=""code"">some code</c> in a summary.
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
            IModule module = new AnalyzeCSharp().WithCssClasses("code", "more-code");

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual("\n    This is <code class=\"code more-code\">some code</code> in a summary.\n    ", GetResult(results, "Green")["Summary"]);
            stream.Dispose();
        }

        [Test]
        public void SummaryWithMultipleCElements()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <summary>
                    /// This is <c>some code</c> in <c>a</c> summary.
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
            Assert.AreEqual("\n    This is <code>some code</code> in <code>a</code> summary.\n    ", GetResult(results, "Green")["Summary"]);
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
            Assert.AreEqual("\n    This is\n    <pre><code>\n    with some code\n    </code></pre>\n    a summary\n    ", GetResult(results, "Green")["Summary"]);
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
                GetResult(results, "Green")["Summary"]);
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
            Assert.AreEqual("FooException",
                GetMember(results, "Green", "Go").List<ReferenceComment>("Exceptions")[0].Name);
            Assert.AreEqual("<a href=\"/Foo/6412642C\">FooException</a>",
                GetMember(results, "Green", "Go").List<ReferenceComment>("Exceptions")[0].Link);
            Assert.AreEqual("Throws when null",
                GetMember(results, "Green", "Go").List<ReferenceComment>("Exceptions")[0].Html);
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
                GetMember(results, "Green", "Go").List<ReferenceComment>("Exceptions")[0].Name);
            Assert.AreEqual("FooException",
                GetMember(results, "Green", "Go").List<ReferenceComment>("Exceptions")[0].Link);
            Assert.AreEqual("Throws when null",
                GetMember(results, "Green", "Go").List<ReferenceComment>("Exceptions")[0].Html);
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
                GetMember(results, "Green", "Go").List<ReferenceComment>("Exceptions")[0].Name);
            Assert.AreEqual("Throws when null",
                GetMember(results, "Green", "Go").List<ReferenceComment>("Exceptions")[0].Html);
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
            Assert.AreEqual(2, GetMember(results, "Green", "Go").List<ReferenceComment>("Exceptions").Count);
            Assert.AreEqual("<a href=\"/Foo/6412642C\">FooException</a>",
                GetMember(results, "Green", "Go").List<ReferenceComment>("Exceptions")[0].Link);
            Assert.AreEqual("FooException",
                GetMember(results, "Green", "Go").List<ReferenceComment>("Exceptions")[0].Name);
            Assert.AreEqual("Throws when null",
                GetMember(results, "Green", "Go").List<ReferenceComment>("Exceptions")[0].Html);
            Assert.AreEqual("BarException",
                GetMember(results, "Green", "Go").List<ReferenceComment>("Exceptions")[1].Link);
            Assert.AreEqual("BarException",
                GetMember(results, "Green", "Go").List<ReferenceComment>("Exceptions")[1].Name);
            Assert.AreEqual("Throws for another reason",
                GetMember(results, "Green", "Go").List<ReferenceComment>("Exceptions")[1].Html);
            stream.Dispose();
        }

        [Test]
        public void SummaryWithBulletListElement()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <summary>
                    /// This is a summary.
                    /// <list type=""bullet"">
                    /// <listheader>
                    /// <term>A</term>
                    /// <description>a</description>
                    /// </listheader>
                    /// <item>
                    /// <term>X</term>
                    /// <description>x</description>
                    /// </item>
                    /// <item>
                    /// <term>Y</term>
                    /// <description>y</description>
                    /// </item>
                    /// </list>
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
            Assert.AreEqual(@"
                This is a summary.
                <ul>
                <li>
                <span class=""term"">A</span>
                <span class=""description"">a</span>
                </li>
                <li>
                <span class=""term"">X</span>
                <span class=""description"">x</span>
                </li>
                <li>
                <span class=""term"">Y</span>
                <span class=""description"">y</span>
                </li>
                </ul>
                ".Replace("\r\n", "\n").Replace("                ", "    "), GetResult(results, "Green")["Summary"]);
            stream.Dispose();
        }

        [Test]
        public void SummaryWithNumberListElement()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <summary>
                    /// This is a summary.
                    /// <list type=""number"">
                    /// <listheader>
                    /// <term>A</term>
                    /// <description>a</description>
                    /// </listheader>
                    /// <item>
                    /// <term>X</term>
                    /// <description>x</description>
                    /// </item>
                    /// <item>
                    /// <term>Y</term>
                    /// <description>y</description>
                    /// </item>
                    /// </list>
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
            Assert.AreEqual(@"
                This is a summary.
                <ol>
                <li>
                <span class=""term"">A</span>
                <span class=""description"">a</span>
                </li>
                <li>
                <span class=""term"">X</span>
                <span class=""description"">x</span>
                </li>
                <li>
                <span class=""term"">Y</span>
                <span class=""description"">y</span>
                </li>
                </ol>
                ".Replace("\r\n", "\n").Replace("                ", "    "), GetResult(results, "Green")["Summary"]);
            stream.Dispose();
        }

        [Test]
        public void SummaryWithTableListElement()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <summary>
                    /// This is a summary.
                    /// <list type=""table"">
                    /// <listheader>
                    /// <term>A</term>
                    /// <term>a</term>
                    /// </listheader>
                    /// <item>
                    /// <term>X</term>
                    /// <term>x</term>
                    /// </item>
                    /// <item>
                    /// <term>Y</term>
                    /// <term>y</term>
                    /// </item>
                    /// </list>
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
            Assert.AreEqual(@"
                This is a summary.
                <table class=""table"">
                <tr>
                <th>A</th>
                <th>a</th>
                </tr>
                <tr>
                <td>X</td>
                <td>x</td>
                </tr>
                <tr>
                <td>Y</td>
                <td>y</td>
                </tr>
                </table>
                ".Replace("\r\n", "\n").Replace("                ", "    "), GetResult(results, "Green")["Summary"]);
            stream.Dispose();
        }

        [Test]
        public void SummaryWithParaElements()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <summary>
                    /// <para>ABC</para>
                    /// <para>XYZ</para>
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
            Assert.AreEqual("\n    <p>ABC</p>\n    <p>XYZ</p>\n    ", GetResult(results, "Green")["Summary"]);
            stream.Dispose();
        }

        [Test]
        public void SummaryWithParaElementsAndNestedCElement()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <summary>
                    /// <para>ABC</para>
                    /// <para>X<c>Y</c>Z</para>
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
            Assert.AreEqual("\n    <p>ABC</p>\n    <p>X<code>Y</code>Z</p>\n    ", GetResult(results, "Green")["Summary"]);
            stream.Dispose();
        }

        [Test]
        public void SummaryWithSeeElement()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <summary>Check <see cref=""Red""/> class</summary>
                    class Green
                    {
                    }

                    class Red
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
            Assert.AreEqual("Check <a href=\"/Foo/414E2165\">Red</a> class", GetResult(results, "Green")["Summary"]);
            stream.Dispose();
        }

        [Test]
        public void SummaryWithSeeElementToMethod()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <summary>Check <see cref=""Red.Blue""/> method</summary>
                    class Green
                    {
                    }

                    class Red
                    {
                        void Blue()
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
            Assert.AreEqual("Check <a href=\"/Foo/414E2165/00F22A50.html\">Blue()</a> method", GetResult(results, "Green")["Summary"]);
            stream.Dispose();
        }

        [Test]
        public void SummaryWithUnknownSeeElement()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <summary>Check <see cref=""Red""/> class</summary>
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
            Assert.AreEqual("Check Red class", GetResult(results, "Green")["Summary"]);
            stream.Dispose();
        }

        [Test]
        public void SummaryWithSeealsoElement()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <summary>Check this out <seealso cref=""Red""/></summary>
                    class Green
                    {
                    }

                    class Red
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
            // <seealso> should be removed from the summary and instead placed in the SeeAlso metadata
            Assert.AreEqual("Check this out ", GetResult(results, "Green")["Summary"]);
            Assert.AreEqual("<a href=\"/Foo/414E2165\">Red</a>", GetResult(results, "Green").Get<IReadOnlyList<string>>("SeeAlso")[0]);
            stream.Dispose();
        }

        [Test]
        public void RootSeealsoElement()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <seealso cref=""Red""/>
                    class Green
                    {
                    }

                    class Red
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
            Assert.AreEqual("<a href=\"/Foo/414E2165\">Red</a>", GetResult(results, "Green").Get<IReadOnlyList<string>>("SeeAlso")[0]);
            stream.Dispose();
        }

        [Test]
        public void OtherCommentWithSeeElement()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <bar>Check <see cref=""Red""/> class</bar>
                    class Green
                    {
                    }

                    class Red
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
            Assert.AreEqual("Check <a href=\"/Foo/414E2165\">Red</a> class", 
                GetResult(results, "Green").List<OtherComment>("BarComments")[0].Html);
            stream.Dispose();
        }

        [Test]
        public void MultipleOtherComments()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <bar>Circle</bar>
                    /// <bar>Square</bar>
                    /// <bar>Rectangle</bar>
                    class Green
                    {
                    }

                    class Red
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
            Assert.AreEqual(3,
                GetResult(results, "Green").List<OtherComment>("BarComments").Count);
            Assert.AreEqual("Circle",
                GetResult(results, "Green").List<OtherComment>("BarComments")[0].Html);
            Assert.AreEqual("Square",
                GetResult(results, "Green").List<OtherComment>("BarComments")[1].Html);
            Assert.AreEqual("Rectangle",
                GetResult(results, "Green").List<OtherComment>("BarComments")[2].Html);
            stream.Dispose();
        }

        [Test]
        public void OtherCommentsWithAttributes()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    /// <bar a='x'>Circle</bar>
                    /// <bar a='y' b='z'>Square</bar>
                    class Green
                    {
                    }

                    class Red
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
            Assert.AreEqual(1,
                GetResult(results, "Green").List<OtherComment>("BarComments")[0].Attributes.Count);
            Assert.AreEqual("x",
                GetResult(results, "Green").List<OtherComment>("BarComments")[0].Attributes["a"]);
            Assert.AreEqual(2,
                GetResult(results, "Green").List<OtherComment>("BarComments")[1].Attributes.Count);
            Assert.AreEqual("y",
                GetResult(results, "Green").List<OtherComment>("BarComments")[1].Attributes["a"]);
            Assert.AreEqual("z",
                GetResult(results, "Green").List<OtherComment>("BarComments")[1].Attributes["b"]);
            stream.Dispose();
        }

        [Test]
        public void NoDocsForImplicitSymbols()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    class Green
                    {
                        /// <summary>This is a summary.</summary>
                        Green() {}
                    }
                }
            ";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp()
                .WhereSymbol(x => x is INamedTypeSymbol);

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.IsFalse(GetResult(results, "Green").Get<IReadOnlyList<IDocument>>("Constructors")[0].ContainsKey("Summary"));
            stream.Dispose();
        }

        [Test]
        public void WithDocsForImplicitSymbols()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    class Green
                    {
                        /// <summary>This is a summary.</summary>
                        Green() {}
                    }
                }
            ";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp()
                .WhereSymbol(x => x is INamedTypeSymbol)
                .WithDocsForImplicitSymbols();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual("This is a summary.", GetResult(results, "Green").Get<IReadOnlyList<IDocument>>("Constructors")[0]["Summary"]);
            stream.Dispose();
        }
    }
}
