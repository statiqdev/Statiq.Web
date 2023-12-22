using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.App;
using Statiq.Common;
using Statiq.Testing;
using Statiq.Web.Pipelines;

namespace Statiq.Web.Tests.Pipelines
{
    [TestFixture]
    public class SitemapFixture : BaseFixture
    {
        public class ExecuteTests : SitemapFixture
        {
            [Test]
            public async Task IncludeInSitemapByDefault()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>());

                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/foo.html",
                        ContentFile
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Sitemap)][Phase.Output].ShouldHaveSingleItem();
                string sitemapContent = await document.GetContentStringAsync();
                sitemapContent.ShouldContain("<loc>/foo</loc>");
            }

            [Test]
            public async Task IncludeInSitemapWhenSpecified()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(WebKeys.GenerateSitemap, true);

                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/foo.html",
                        @"IncludeInSitemap: true
---" + ContentFile
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Sitemap)][Phase.Output].ShouldHaveSingleItem();
                string sitemapContent = await document.GetContentStringAsync();
                sitemapContent.ShouldContain("<loc>/foo</loc>");
            }

            [Test]
            public async Task ExcludeFromSitemapWhenSpecified()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(WebKeys.GenerateSitemap, true);

                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/foo.html",
                        @"IncludeInSitemap: false
---" + ContentFile
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Sitemap)][Phase.Output].ShouldHaveSingleItem();
                string sitemapContent = await document.GetContentStringAsync();
                sitemapContent.ShouldNotContain("<loc>/foo</loc>");
            }
        }

        public const string ContentFile = @"
<html>
  <head>
  </head>
  <body>
  </body>
</html>";
    }
}