using System;
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
    public class SearchIndexFixture : BaseFixture
    {
        public class ExecuteTests : SearchIndexFixture
        {
            [Test]
            public async Task ShouldNotGenerateSearchIndexByDefault()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(WebKeys.GenerateSearchIndex, false);
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/a/a.html",
                        @"Title: Foo
---
Fizz"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Outputs[nameof(SearchIndex)][Phase.Output].ShouldBeEmpty();
            }

            [Test]
            public async Task ShouldGenerateScriptAtDefaultPath()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(WebKeys.GenerateSearchIndex, true);
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/a/a.html",
                        @"Title: Foo
---
Fizz"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Outputs[nameof(SearchIndex)][Phase.Output]
                    .Select(x => x.Destination.FullPath)
                    .ShouldBe(
                        new[]
                        {
                            "search.js",
                            "search.results.gz",
                            "search.index.gz"
                        },
                        true);
            }

            [Test]
            public async Task ShouldGenerateScriptAtAlternatePath()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(WebKeys.GenerateSearchIndex, true)
                    .AddSetting(WebKeys.SearchScriptPath, "bar.js");
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/a/a.html",
                        @"Title: Foo
---
Fizz"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Outputs[nameof(SearchIndex)][Phase.Output]
                    .Select(x => x.Destination.FullPath)
                    .ShouldBe(
                        new[]
                        {
                            "bar.js",
                            "bar.results.gz",
                            "bar.index.gz"
                        },
                        true);
            }

            [Test]
            public async Task ShouldGenerateIndexAtAlternatePath()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(WebKeys.GenerateSearchIndex, true)
                    .AddSetting(WebKeys.SearchIndexPath, "bar.index.gz");
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/a/a.html",
                        @"Title: Foo
---
Fizz"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Outputs[nameof(SearchIndex)][Phase.Output]
                    .Select(x => x.Destination.FullPath)
                    .ShouldBe(
                        new[]
                        {
                            "search.js",
                            "search.results.gz",
                            "bar.index.gz"
                        },
                        true);
            }

            [Test]
            public async Task ShouldGenerateResultsAtAlternatePath()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(WebKeys.GenerateSearchIndex, true)
                    .AddSetting(WebKeys.SearchResultsPath, "bar.results.gz");
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/a/a.html",
                        @"Title: Foo
---
Fizz"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Outputs[nameof(SearchIndex)][Phase.Output]
                    .Select(x => x.Destination.FullPath)
                    .ShouldBe(
                        new[]
                        {
                            "search.js",
                            "bar.results.gz",
                            "search.index.gz"
                        },
                        true);
            }

            [Test]
            public async Task ShouldNotZipIndexFile()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(WebKeys.GenerateSearchIndex, true)
                    .AddSetting(WebKeys.ZipSearchIndexFile, false);
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/a/a.html",
                        @"Title: Foo
---
Fizz"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Outputs[nameof(SearchIndex)][Phase.Output]
                    .Select(x => x.Destination.FullPath)
                    .ShouldBe(
                        new[]
                        {
                            "search.js",
                            "search.results.gz",
                            "search.index.json"
                        },
                        true);
                IDocument document = result.Outputs[nameof(SearchIndex)][Phase.Output]
                    .Single(x => x.Destination.FullPath == "search.index.json");
                (await document.GetContentStringAsync()).ShouldContain(@"""fields"":[""title"",""content""]");
            }

            [Test]
            public async Task ShouldNotZipResultsFile()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(WebKeys.GenerateSearchIndex, true)
                    .AddSetting(WebKeys.ZipSearchResultsFile, false);
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/a/a.html",
                        @"Title: Foo
---
Fizz"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Outputs[nameof(SearchIndex)][Phase.Output]
                    .Select(x => x.Destination.FullPath)
                    .ShouldBe(
                        new[]
                        {
                            "search.js",
                            "search.results.json",
                            "search.index.gz"
                        },
                        true);
                IDocument document = result.Outputs[nameof(SearchIndex)][Phase.Output]
                    .Single(x => x.Destination.FullPath == "search.results.json");
                (await document.GetContentStringAsync()).ShouldContain(@"{""-224211022"":{""link"":""/a/a"",""title"":""Foo""}}");
            }
        }
    }
}