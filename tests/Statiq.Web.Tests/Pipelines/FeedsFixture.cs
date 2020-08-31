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
    public class FeedsFixture : BaseFixture
    {
        public class ExecuteTests : DataFixture
        {
            [Test]
            public async Task AllowsStringIds()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/foo.md", "Hi!" },
                    {
                        "/input/feed.yml",
                        @"FeedItemId: => ""Bar""
FeedRss: true"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Web.Pipelines.Feeds)][Phase.Process].ShouldHaveSingleItem();
                (await document.GetContentStringAsync()).ShouldContain(@"<guid isPermaLink=""false"">Bar</guid>");
            }

            [Test]
            public async Task ExcludesAssetContentType()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/foo.md", "Hi!" },
                    {
                        "/input/feed.yml",
                        @"ContentType: Asset
FeedItemId: => ""Bar""
FeedRss: true"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Outputs[nameof(Web.Pipelines.Feeds)][Phase.Process].ShouldBeEmpty();
            }
        }
    }
}
