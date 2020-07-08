using System;
using System.Collections.Immutable;
using System.Linq;
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
    public class RedirectsFixture : BaseFixture
    {
        public class ExecuteTests : RedirectsFixture
        {
            [Test]
            public async Task GeneratesClientRedirects()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/a/b/c.md",
                        @"RedirectFrom: x/y
---
Foo"
                    },
                    {
                        "/input/d/e.md",
                        "Bar"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Redirects)][Phase.Process].ShouldHaveSingleItem();
                document.Destination.ShouldBe("x/y.html");
                (await document.GetContentStringAsync()).ShouldContain(@"<meta http-equiv=""refresh"" content=""0;url='/a/b/c'"" />");
            }

            [Test]
            public async Task GeneratesNetlifyRedirects()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(WebKeys.NetlifyRedirects, true)
                    .AddSetting(WebKeys.MetaRefreshRedirects, false);
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/a/b/c.md",
                        @"RedirectFrom: x/y
---
Foo"
                    },
                    {
                        "/input/d/e.md",
                        "Bar"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Redirects)][Phase.Process].ShouldHaveSingleItem();
                document.Destination.ShouldBe("_redirects");
                (await document.GetContentStringAsync()).ShouldBe("/x/y /a/b/c");
            }
        }
    }
}
