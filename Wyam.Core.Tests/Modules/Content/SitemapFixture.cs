using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Pipelines;
using Wyam.Core.Documents;
using Wyam.Core.Modules.Contents;
using Wyam.Core.Modules.Metadata;
using Wyam.Core.Pipelines;
using ExecutionContext = Wyam.Core.Pipelines.ExecutionContext;

namespace Wyam.Core.Tests.Modules.Content
{
    [TestFixture]
    public class SitemapFixture
    {
        [SetUp]
        public void SetUp()
        {
            if (Directory.Exists(@"TestFiles\Output\"))
            {
                int c = 0;
                while (true)
                {
                    try
                    {
                        Directory.Delete(@"TestFiles\Output\", true);
                        break;
                    }
                    catch (System.IO.IOException)
                    {
                        Thread.Sleep(1000);
                        if (c++ < 4)
                        {
                            continue;
                        }
                        throw;
                    }
                }
            }

            Directory.CreateDirectory("TestFiles/Output");
        }

        [TestCase("http://www.example.org", null, "http://www.example.org/sub/testfile.html")]
        [TestCase(null, "http://www.example.com/{0}", "http://www.example.com/sub/testfile.html")]
        [TestCase("http://www.example.org", "http://www.example.com/{0}", "http://www.example.com/sub/testfile.html")]
        public void SitemapGeneratedWithSitemapItem(string hostname, string formatterString, string expected)
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.OutputFolder = @"TestFiles/output/";

            if (!string.IsNullOrWhiteSpace(hostname))
                engine.Metadata[Keys.Hostname] = hostname;

            Pipeline contentPipeline = new Pipeline("Content", engine, null);
            var doc = new Document(engine, contentPipeline).Clone("Test", new[] { new KeyValuePair<string, object>(Keys.RelativeFilePath, "sub/testfile.html") });
            IDocument[] inputs = { doc };

            IExecutionContext context = new ExecutionContext(engine, contentPipeline);
            Core.Modules.Metadata.Meta m = new Core.Modules.Metadata.Meta(Keys.SitemapItem, (d, c) => new SitemapItem(d[Keys.RelativeFilePath].ToString()));
            var outputs = m.Execute(inputs, context);

            Func<string, string> formatter = null;

            if (!string.IsNullOrWhiteSpace(formatterString))
                formatter = f => string.Format(formatterString, f);

            // When
            Sitemap sitemap = new Sitemap("sitemap.xml", formatter).FromInputDocuments();
            sitemap.Execute(outputs.ToList(), context);

            foreach (IDocument document in inputs.Concat(outputs.ToList()))
            {
                ((IDisposable)document).Dispose();
            }

            // Then
            Assert.IsTrue(File.Exists(@"TestFiles\Output\sitemap.xml"));
            var sitemapContent = File.ReadAllText(@"TestFiles\Output\sitemap.xml");
            Assert.That(sitemapContent, Is.StringContaining($"<loc>{expected}</loc>"));
        }

        [TestCase("http://www.example.org", null, "http://www.example.org/sub/testfile.html")]
        [TestCase(null, "http://www.example.com/{0}", "http://www.example.com/sub/testfile.html")]
        [TestCase("http://www.example.org", "http://www.example.com/{0}", "http://www.example.com/sub/testfile.html")]
        public void SitemapGeneratedWithSitemapItemAsString(string hostname, string formatterString, string expected)
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.OutputFolder = @"TestFiles/output/";

            if( !string.IsNullOrWhiteSpace(hostname) )
                engine.Metadata[Keys.Hostname] = hostname;

            Pipeline contentPipeline = new Pipeline("Content", engine, null);
            var doc = new Document(engine, contentPipeline).Clone("Test", new[] { new KeyValuePair<string, object>(Keys.RelativeFilePath, "sub/testfile.html") });
            IDocument[] inputs = { doc };

            IExecutionContext context = new ExecutionContext(engine, contentPipeline);
            Core.Modules.Metadata.Meta m = new Core.Modules.Metadata.Meta(Keys.SitemapItem, (d, c) => d[Keys.RelativeFilePath].ToString());
            var outputs = m.Execute(inputs, context);

            Func<string, string> formatter = null;

            if (!string.IsNullOrWhiteSpace(formatterString))
                formatter = f => string.Format(formatterString, f);

            // When
            Sitemap sitemap = new Sitemap("sitemap.xml", formatter).FromInputDocuments();
            sitemap.Execute(outputs.ToList(), context);

            foreach (IDocument document in inputs.Concat(outputs.ToList()))
            {
                ((IDisposable)document).Dispose();
            }

            // Then
            Assert.IsTrue(File.Exists(@"TestFiles\Output\sitemap.xml"));
            var sitemapContent = File.ReadAllText(@"TestFiles\Output\sitemap.xml");
            Assert.That(sitemapContent, Is.StringContaining($"<loc>{expected}</loc>"));
        }
    }
}
