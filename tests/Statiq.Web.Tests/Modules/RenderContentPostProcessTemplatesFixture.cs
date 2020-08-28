using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;
using Statiq.Web.Modules;

namespace Statiq.Web.Tests.Modules
{
    [TestFixture]
    public class RenderContentPostProcessTemplatesFixture : BaseFixture
    {
        public class ExecuteTests : RenderContentPostProcessTemplatesFixture
        {
            [Test]
            public async Task MakesRelativeLinksAbsolute()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Services.AddSingleton<IReadOnlyFileSystem>(context.FileSystem);
                context.Settings[WebKeys.MakeLinksAbsolute] = true;
                context.Settings[Keys.Host] = "site.com";
                context.Settings[Keys.LinksUseHttps] = true;
                TestDocument document = new TestDocument(
                    new NormalizedPath("/a/b/c.html"),
                    new NormalizedPath("a/b/c.html"),
                    @"<html>
                        <body>
                            Foo <a href=""../d"">FizzBuzz</a>
                        </body>
                    </html>");
                Templates templates = new Templates();
                RenderContentPostProcessTemplates module = new RenderContentPostProcessTemplates(templates);

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html><head></head><body>
                            Foo <a href=""https://site.com/a/d"">FizzBuzz</a>
                        
                    </body></html>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task MakesRelativeLinksRootRelative()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Services.AddSingleton<IReadOnlyFileSystem>(context.FileSystem);
                context.Settings[WebKeys.MakeLinksRootRelative] = true;
                context.Settings[Keys.Host] = "site.com";
                context.Settings[Keys.LinksUseHttps] = true;
                TestDocument document = new TestDocument(
                    new NormalizedPath("/a/b/c.html"),
                    new NormalizedPath("a/b/c.html"),
                    @"<html>
                        <body>
                            Foo <a href=""../d"">FizzBuzz</a>
                        </body>
                    </html>");
                Templates templates = new Templates();
                RenderContentPostProcessTemplates module = new RenderContentPostProcessTemplates(templates);

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html><head></head><body>
                            Foo <a href=""/a/d"">FizzBuzz</a>
                        
                    </body></html>",
                    StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}
