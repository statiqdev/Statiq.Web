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
            Assert.AreEqual("This is a summary.", GetClass(results, "Green")["SummaryHtml"]);
            Assert.AreEqual("This is another summary.", GetClass(results, "Red")["SummaryHtml"]);
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
            Assert.AreEqual("\n    This is a summary.\n    ", GetClass(results, "Green")["SummaryHtml"]);
            Assert.AreEqual("\n    This is\n    another summary.\n    ", GetClass(results, "Red")["SummaryHtml"]);
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
            Assert.AreEqual("This is a summary.\nThis is another summary.", GetClass(results, "Green")["SummaryHtml"]);
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
            Assert.AreEqual(string.Empty, GetClass(results, "Green")["SummaryHtml"]);
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
            Assert.AreEqual("\n    This is <code>some code</code> in a summary.\n    ", GetClass(results, "Green")["SummaryHtml"]);
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
            Assert.AreEqual("\n    This is <code class=\"code\">some code</code> in a summary.\n    ", GetClass(results, "Green")["SummaryHtml"]);
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
            Assert.AreEqual("\n    This is <code class=\"code\">some code</code> in a summary.\n    ", GetClass(results, "Green")["SummaryHtml"]);
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
            Assert.AreEqual("\n    This is <code class=\"code more-code\">some code</code> in a summary.\n    ", GetClass(results, "Green")["SummaryHtml"]);
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
            Assert.AreEqual("\n    This is <code>some code</code> in <code>a</code> summary.\n    ", GetClass(results, "Green")["SummaryHtml"]);
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
            Assert.AreEqual("\n    This is\n    <pre><code>\n    with some code\n    </code></pre>\n    a summary\n    ", GetClass(results, "Green")["SummaryHtml"]);
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
                GetClass(results, "Green")["SummaryHtml"]);
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
            Assert.AreEqual("<a href=\"/Foo/6412642C\">FooException</a>",
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
            Assert.AreEqual("<a href=\"/Foo/6412642C\">FooException</a>",
                GetMember(results, "Green", "Go").Get<IReadOnlyList<KeyValuePair<string, string>>>("ExceptionHtml")[0].Key);
            Assert.AreEqual("Throws when null",
                GetMember(results, "Green", "Go").Get<IReadOnlyList<KeyValuePair<string, string>>>("ExceptionHtml")[0].Value);
            Assert.AreEqual("BarException",
                GetMember(results, "Green", "Go").Get<IReadOnlyList<KeyValuePair<string, string>>>("ExceptionHtml")[1].Key);
            Assert.AreEqual("Throws for another reason",
                GetMember(results, "Green", "Go").Get<IReadOnlyList<KeyValuePair<string, string>>>("ExceptionHtml")[1].Value);
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
                ".Replace("\r\n", "\n").Replace("                ", "    "), GetClass(results, "Green")["SummaryHtml"]);
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
                ".Replace("\r\n", "\n").Replace("                ", "    "), GetClass(results, "Green")["SummaryHtml"]);
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
                ".Replace("\r\n", "\n").Replace("                ", "    "), GetClass(results, "Green")["SummaryHtml"]);
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
            Assert.AreEqual("\n    <p>ABC</p>\n    <p>XYZ</p>\n    ", GetClass(results, "Green")["SummaryHtml"]);
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
            Assert.AreEqual("\n    <p>ABC</p>\n    <p>X<code>Y</code>Z</p>\n    ", GetClass(results, "Green")["SummaryHtml"]);
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
            Assert.AreEqual("Check <a href=\"/Foo/414E2165\">Red</a> class", GetClass(results, "Green")["SummaryHtml"]);
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
            Assert.AreEqual("Check <a href=\"/Foo/414E2165/00F22A50.html\">Blue()</a> method", GetClass(results, "Green")["SummaryHtml"]);
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
            Assert.AreEqual("Check Red class", GetClass(results, "Green")["SummaryHtml"]);
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
            Assert.AreEqual("Check this out ", GetClass(results, "Green")["SummaryHtml"]);
            Assert.AreEqual("<a href=\"/Foo/414E2165\">Red</a>", GetClass(results, "Green").Get<IReadOnlyList<string>>("SeeAlsoHtml")[0]);
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
            Assert.AreEqual("<a href=\"/Foo/414E2165\">Red</a>", GetClass(results, "Green").Get<IReadOnlyList<string>>("SeeAlsoHtml")[0]);
            stream.Dispose();
        }

        // TODO: Remark content with a <see> where the cref points to a member (method or property) of the described class
        // TODO: Remark content with a <see> where the cref points to a member (method or property) of a different class
    }
}
