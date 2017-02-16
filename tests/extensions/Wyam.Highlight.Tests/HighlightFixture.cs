using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.JavaScript;
using TestExecutionContext = Wyam.Testing.Execution.TestExecutionContext;

namespace Wyam.Highlight.Tests
{
    [TestFixture]
    public class HighlightFixture : BaseFixture
    {
        [Test]
        public void CanHighlightCSharp()
        {
            string input = @"
<html>
<head>
    <title>Foobar</title>
</head>
<body>
    <h1>Title</h1>
    <p>This is some Foobar text</p>
    <pre><code class=""language-csharp"">
    class Program
    {
        static void Main(string[] args)
        {
            var invoices = new List&lt;Invoice&gt; { new Invoice { InvoiceId = 0 } };
            var oneTimeCharges = new List&lt;OneTimeCharge&gt; { new OneTimeCharge { Invoice = 0, OneTimeChargeId = 0 } };
            var otcCharges = invoices.Join(oneTimeCharges, inv =&gt; inv.InvoiceId, otc =&gt; otc.Invoice, (inv, otc) =&gt; inv.InvoiceId);
            Console.WriteLine(otcCharges.Count());
        }        
    }

    public class OneTimeCharge
    {
        public int OneTimeChargeId { get; set; }
        public int? Invoice { get; set; }
    }

    public class Invoice
    {
        public int InvoiceId { get; set; }
    }
    </code></pre>
</body>
</html>";


            IDocument document = new TestDocument(input);
            IExecutionContext context = new TestExecutionContext()
            {
                JsEngineFunc = () => new TestJsEngine()
            };

            Highlight highlight = new Highlight();

            // When
            List<IDocument> results = highlight.Execute(new[] { document }, context).ToList();

            // Then
            Assert.IsTrue(results[0].Content.Contains("language-csharp hljs"));
        }

        [Test]
        public void CanHighlightHtml()
        {
            string input = @"
<html>
<head>
    <title>Foobar</title>
</head>
<body>
    <h1>Title</h1>
    <p>This is some Foobar text</p>
    <pre><code class=""language-html"">
    <html>
    <head>
    <title>Hi Mom!</title>
    </head>
    <body>
        <p>Hello, world! Pretty me up!
    </body>
    </html>
    </code></pre>
</body>
</html>";


            IDocument document = new TestDocument(input);
            IExecutionContext context = new TestExecutionContext()
            {
                JsEngineFunc = () => new TestJsEngine()
            };

            Highlight highlight = new Highlight();

            // When
            List<IDocument> results = highlight.Execute(new[] { document }, context).ToList();

            // Then
            Assert.IsTrue(results[0].Content.Contains("language-html hljs"));
        }

        [Test]
        public void CanHighlightAfterRazor()
        {
            // if we execute razor before this, the code block will be escaped.
            string input = @"
<html>
<head>
    <title>Foobar</title>
</head>
<body>
    <h1>Title</h1>
    <p>This is some Foobar text</p>
    <pre><code class=""language-html"">
    &lt;strong class=&quot;super-strong&quot;&gt;this is strong text&lt;/strong&gt;
    </code></pre>
</body>
</html>";


            IDocument document = new TestDocument(input);
            IExecutionContext context = new TestExecutionContext()
            {
                JsEngineFunc = () => new TestJsEngine()
            };

            Highlight highlight = new Highlight();
            
            // When
            List<IDocument> results = highlight.Execute(new[] { document }, context).ToList();

            // Then
            Assert.IsTrue(results[0].Content.Contains("language-html hljs"));
        }
        
        [Test]
        [Ignore("fails due to Jint issues with regex")]
        public void CanHighlightAutoCodeBlocks()
        {
            string input = @"
<html>
<head>
    <title>Foobar</title>
</head>
<body>
    <h1>Title</h1>
    <p>This is some Foobar text</p>
    <pre><code>
        if (foo == bar)
        {
            DoTheFooBar();
        }
    </code></pre>
</body>
</html>";


            IDocument document = new TestDocument(input);
            IExecutionContext context = new TestExecutionContext()
            {
                JsEngineFunc = () => new TestJsEngine()
            };
            
            Highlight highlight = new Highlight();

            // When
            List<IDocument> results = highlight.Execute(new[] { document }, context).ToList();

            // Then
            Assert.IsTrue(results[0].Content.Contains("hljs"));
        }


        [Test]
        public void HighlightFailsForMissingLanguage()
        {
            string input = @"
<html>
<head>
    <title>Foobar</title>
</head>
<body>
    <h1>Title</h1>
    <p>This is some Foobar text</p>
    <pre><code class=""language-zorg"">
    <html>
    <head>
    <title>Hi Mom!</title>
    </head>
    <body>
        <p>Hello, world! Pretty me up!
    </body>
    </html>
    </code></pre>
</body>
</html>";


            IDocument document = new TestDocument(input);
            IExecutionContext context = new TestExecutionContext()
            {
                JsEngineFunc = () => new TestJsEngine()
            };

            Highlight highlight = new Highlight();

            Assert.Throws<AggregateException>(() =>
            {
                List<IDocument> results = highlight.Execute(new[] {document}, context).ToList();
                Assert.IsNull(results, "Should never get here due to exception");
            });
        }

        [Test]
        public void HighlightSucceedsForMissingLanguageWhenConfiguredNotToWarn()
        {
            string input = @"
<html>
<head>
    <title>Foobar</title>
</head>
<body>
    <h1>Title</h1>
    <p>This is some Foobar text</p>
    <pre><code class=""language-zorg"">
    <html>
    <head>
    <title>Hi Mom!</title>
    </head>
    <body>
        <p>Hello, world! Pretty me up!
    </body>
    </html>
    </code></pre>
</body>
</html>";


            IDocument document = new TestDocument(input);
            IExecutionContext context = new TestExecutionContext()
            {
                JsEngineFunc = () => new TestJsEngine()
            };

            Highlight highlight = new Highlight()
                .WithMissingLanguageWarning(false);

            List<IDocument> results = highlight.Execute(new[] { document }, context).ToList();
            CollectionAssert.IsNotEmpty(results);
        }
    }
}