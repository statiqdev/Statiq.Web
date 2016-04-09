using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Testing;

namespace Wyam.Modules.Minification.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class XmlMinifyTests : BaseFixture
    {
        public class ExecuteMethodTests : XmlMinifyTests
        {
            [Test]
            public void Minify()
            {
                // Given
                string input = @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
                        <urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                            <!-- Homepage -->
                            <url>
                                <loc>http://wyam.io/</loc>
                                <changefreq>weekly</changefreq>
                                <priority>0.9</priority>
                            </url>
                            <!-- Content Page -->
                            <url>
                                <loc>http://wyam.io/modules/xmlminify</loc>
                                <changefreq>monthly</changefreq>
                                <priority>0.7</priority>
                            </url>
                        </urlset>";
                string output = @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?><urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9""><url><loc>http://wyam.io/</loc><changefreq>weekly</changefreq><priority>0.9</priority></url><url><loc>http://wyam.io/modules/xmlminify</loc><changefreq>monthly</changefreq><priority>0.7</priority></url></urlset>";

                IExecutionContext context = Substitute.For<IExecutionContext>();
                IDocument document = Substitute.For<IDocument>();
                document.Content.Returns(input);

                XmlMinify xmlMinify = new XmlMinify();

                // When
                xmlMinify.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, output);
            }
        }
    }
}