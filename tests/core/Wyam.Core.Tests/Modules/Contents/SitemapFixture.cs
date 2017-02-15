using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;
using Wyam.Core.Documents;
using Wyam.Core.Modules.Contents;
using Wyam.Core.Modules.Metadata;
using Wyam.Core.Execution;
using Wyam.Testing;
using ExecutionContext = Wyam.Core.Execution.ExecutionContext;

namespace Wyam.Core.Tests.Modules.Contents
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class SitemapFixture : BaseFixture
    {
        public class ExecuteTests : SitemapFixture
        {
            [TestCase("www.example.org", null, "http://www.example.org/sub/testfile")]
            [TestCase(null, "http://www.example.com", "http://www.example.com")]
            [TestCase("www.example.org", "http://www.example.com/{0}", "http://www.example.com/sub/testfile.html")]
            public void SitemapGeneratedWithSitemapItem(string hostname, string formatterString, string expected)
            {
                // Given
                Engine engine = new Engine();
                if (!string.IsNullOrWhiteSpace(hostname))
                {
                    engine.Settings[Keys.Host] = hostname;
                }
                Pipeline contentPipeline = new Pipeline("Content", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, contentPipeline);

                IDocument doc = context.GetDocument("Test", new[]
                {
                    new KeyValuePair<string, object>(Keys.RelativeFilePath, "sub/testfile.html")
                });
                IDocument[] inputs = {doc};

                Core.Modules.Metadata.Meta m = new Core.Modules.Metadata.Meta(Keys.SitemapItem,
                    (d, c) => new SitemapItem(d[Keys.RelativeFilePath].ToString()));
                var outputs = m.Execute(inputs, context);

                Func<string, string> formatter = null;

                if (!string.IsNullOrWhiteSpace(formatterString))
                    formatter = f => string.Format(formatterString, f);

                // When
                Sitemap sitemap = new Sitemap(formatter);
                List<IDocument> results = sitemap.Execute(outputs.ToList(), context).ToList();

                foreach (IDocument document in inputs.Concat(outputs.ToList()))
                {
                    ((IDisposable) document).Dispose();
                }

                // Then
                Assert.AreEqual(1, results.Count);
                Assert.That(results[0].Content, Does.Contain($"<loc>{expected}</loc>"));
            }

            [TestCase("www.example.org", null, "http://www.example.org/sub/testfile")]
            [TestCase(null, "http://www.example.com", "http://www.example.com")]
            [TestCase("www.example.org", "http://www.example.com/{0}", "http://www.example.com/sub/testfile.html")]
            public void SitemapGeneratedWithSitemapItemAsString(string hostname, string formatterString, string expected)
            {
                // Given
                Engine engine = new Engine();
                if (!string.IsNullOrWhiteSpace(hostname))
                {
                    engine.Settings[Keys.Host] = hostname;
                }
                Pipeline contentPipeline = new Pipeline("Content", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, contentPipeline);

                IDocument doc = context.GetDocument("Test", new[]
                {
                    new KeyValuePair<string, object>(Keys.RelativeFilePath, "sub/testfile.html")
                });
                IDocument[] inputs = {doc};

                Core.Modules.Metadata.Meta m = new Core.Modules.Metadata.Meta(Keys.SitemapItem,
                    (d, c) => d[Keys.RelativeFilePath].ToString());
                var outputs = m.Execute(inputs, context);

                Func<string, string> formatter = null;

                if (!string.IsNullOrWhiteSpace(formatterString))
                    formatter = f => string.Format(formatterString, f);

                // When
                Sitemap sitemap = new Sitemap(formatter);
                List<IDocument> results = sitemap.Execute(outputs.ToList(), context).ToList();

                foreach (IDocument document in inputs.Concat(outputs.ToList()))
                {
                    ((IDisposable) document).Dispose();
                }

                // Then
                Assert.AreEqual(1, results.Count);
                Assert.That(results[0].Content, Does.Contain($"<loc>{expected}</loc>"));
            }
        }
    }
}
