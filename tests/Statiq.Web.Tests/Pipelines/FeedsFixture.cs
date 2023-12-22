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
        public class ExecuteTests : FeedsFixture
        {
            [Test]
            public async Task AllowsStringIds()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(Keys.Host, "statiq.dev");
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
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(Keys.Host, "statiq.dev");
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

            [Test]
            public async Task ToggleFeedWithPath()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(Keys.Host, "statiq.dev");
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/foo.md", "Hi!" },
                    {
                        "/input/feed.yml",
                        "FeedRss: rss/custom-feed.xml"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Web.Pipelines.Feeds)][Phase.Process].ShouldHaveSingleItem();
                document.Destination.ShouldBe("rss/custom-feed.xml");
            }

            [TestCase("a", "b", "c", "a")]
            [TestCase(null, "b", "c", "b")]
            [TestCase(null, null, "c", "c")]
            [TestCase(null, null, null, "Feed")]
            public async Task ShouldSetCorrectFeedTitle(
                string feedTitle, string title, string siteTitle, string expected)
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(Keys.Host, "statiq.dev");
                if (siteTitle is object)
                {
                    bootstrapper.AddSetting(WebKeys.SiteTitle, siteTitle);
                }
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/foo.md", "Hi!" },
                    {
                        "/input/feed.yml",
                        @"FeedItemId: => ""Bar""
FeedRss: true" + (feedTitle is null ? string.Empty : $@"
FeedTitle: {feedTitle}") + (title is null ? string.Empty : $@"
Title: {title}")
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Web.Pipelines.Feeds)][Phase.Process].ShouldHaveSingleItem();
                (await document.GetContentStringAsync()).ShouldContain($"<title>{expected}</title>");
            }

            [TestCase("a", "b", "c", "a")]
            [TestCase(null, "b", "c", "b")]
            [TestCase(null, null, "c", "c")]
            public async Task ShouldSetCorrectFeedDescription(
                string feedDescription, string description, string siteDescription, string expected)
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(Keys.Host, "statiq.dev");
                if (siteDescription is object)
                {
                    bootstrapper.AddSetting(WebKeys.SiteDescription, siteDescription);
                }
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/foo.md", "Hi!" },
                    {
                        "/input/feed.yml",
                        @"FeedItemId: => ""Bar""
FeedRss: true" + (feedDescription is null ? string.Empty : $@"
FeedDescription: {feedDescription}") + (description is null ? string.Empty : $@"
Description: {description}")
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Web.Pipelines.Feeds)][Phase.Process].ShouldHaveSingleItem();
                (await document.GetContentStringAsync()).ShouldContain($"<description>{expected}</description>");
            }
        }
    }
}