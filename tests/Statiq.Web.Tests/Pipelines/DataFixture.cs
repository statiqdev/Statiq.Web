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
    public class DataFixture : BaseFixture
    {
        public class ExecuteTests : DataFixture
        {
            [Test]
            public async Task ParsesJson()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/a/b/c.json", "{ \"Foo\": \"Bar\" }" }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Data)][Phase.Process].ShouldHaveSingleItem();
                document["Foo"].ShouldBe("Bar");
            }

            [Test]
            public async Task ParsesYaml()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/a/b/c.yaml", "Foo: Bar" }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Data)][Phase.Process].ShouldHaveSingleItem();
                document["Foo"].ShouldBe("Bar");
            }

            [Test]
            public async Task HonorsDataFilesSetting()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                bootstrapper.AddSetting(WebKeys.DataFiles, "x/**/*.json");
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/a/b/c.json", "{ \"Foo\": \"Bar\" }" },
                    { "/input/x/y/z.json", "{ \"Foo\": \"Buz\" }" }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Data)][Phase.Process].ShouldHaveSingleItem();
                document["Foo"].ShouldBe("Buz");
            }

            [Test]
            public async Task SupportsMultipleDataFilesPatterns()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                bootstrapper.AddSetting(WebKeys.DataFiles, new[] { "a/**/*.json", "x/**/*.json" });
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/a/b/c.json", "{ \"Foo\": \"Bar\" }" },
                    { "/input/x/y/z.json", "{ \"Foo\": \"Buz\" }" }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Outputs[nameof(Data)][Phase.Process].Select(x => x["Foo"]).ShouldBe(new[] { "Bar", "Buz" }, true);
            }

            [Test]
            public async Task IncludesDocumentsFromDependencies()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                bootstrapper.AddSetting(WebKeys.DataFiles, "x/**/*.json");
                bootstrapper.BuildPipeline("Test", builder => builder
                    .WithInputReadFiles("a/**/*.json")
                    .AsDependencyOf(nameof(Data)));
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/a/b/c.json", "{ \"Foo\": \"Bar\" }" },
                    { "/input/x/y/z.json", "{ \"Foo\": \"Buz\" }" }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Outputs[nameof(Data)][Phase.Process].Select(x => x["Foo"]).ShouldBe(new[] { "Bar", "Buz" }, true);
            }

            [Test]
            public async Task AddsDiectoryMetadata()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/a/b/c.json", "{ \"Foo\": \"Bar\" }" },
                    { "/input/a/_directory.json", "{ \"Fizz\": \"Buzz\" }" }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Data)][Phase.Process].ShouldHaveSingleItem();
                document["Foo"].ShouldBe("Bar");
                document["Fizz"].ShouldBe("Buzz");
            }

            [Test]
            public async Task FiltersExcludedFiles()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/a/b/c.json", "{ \"Foo\": \"Bar\" }" },
                    { "/input/x/y/z.json", "{ \"Excluded\": \"true\" }" }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Data)][Phase.Process].ShouldHaveSingleItem();
                document["Foo"].ShouldBe("Bar");
            }

            [Test]
            public async Task DoesNotReadUnderscoreFilesByDefault()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/a/b/c.json", "{ \"Foo\": \"Bar\" }" },
                    { "/input/x/y/_z.json", "{ \"Fizz\": \"Buzz\" }" }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Data)][Phase.Process].ShouldHaveSingleItem();
                document["Foo"].ShouldBe("Bar");
            }
        }
    }
}
