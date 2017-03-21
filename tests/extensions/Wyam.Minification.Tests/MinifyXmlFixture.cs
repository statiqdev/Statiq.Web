using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Minification.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class MinifyXmlFixture : BaseFixture
    {
        public class ExecuteTests : MinifyXmlFixture
        {
            [Test]
            public void Minify()
            {
                // Given
                string input = @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
                        <urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                            <!-- Homepage -->
                            <url>
                                <loc>https://wyam.io/</loc>
                                <changefreq>weekly</changefreq>
                                <priority>0.9</priority>
                            </url>
                            <!-- Content Page -->
                            <url>
                                <loc>https://wyam.io/modules/minifyxml</loc>
                                <changefreq>monthly</changefreq>
                                <priority>0.7</priority>
                            </url>
                        </urlset>";
                string output = @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?><urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9""><url><loc>https://wyam.io/</loc><changefreq>weekly</changefreq><priority>0.9</priority></url><url><loc>https://wyam.io/modules/minifyxml</loc><changefreq>monthly</changefreq><priority>0.7</priority></url></urlset>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                MinifyXml minifyXml = new MinifyXml();

                // When
                IList<IDocument> results = minifyXml.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { output }));
            }
        }
    }
}