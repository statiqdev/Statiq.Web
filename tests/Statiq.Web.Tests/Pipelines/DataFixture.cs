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
            public async Task DoesNotClearDataContentIfNotSet()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>());
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
                (await document.GetContentStringAsync()).ShouldBe("{ \"Foo\": \"Bar\" }");
            }

            [Test]
            public async Task ClearsDataContentIfSet()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(WebKeys.ClearDataContent, true);
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
                (await document.GetContentStringAsync()).ShouldBeEmpty();
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
            public async Task IncludesDocumentsFromDependencies()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(WebKeys.InputFiles, "x/**/*");
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
            public async Task AddsDirectoryMetadata()
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
            public async Task DoesNotApplyDirectoryMetadataIfSettingIsFalse()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(WebKeys.ApplyDirectoryMetadata, false);
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
                document.ContainsKey("Fizz").ShouldBeFalse();
            }

            [Test]
            public async Task DoesNotAddNonRecursiveDirectoryMetadata()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/a/b/c.json", "{ \"Foo\": \"Bar\" }" },
                    { "/input/a/_directory.json", "{ \"Fizz\": \"Buzz\", \"Recursive\": \"false\" }" }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Data)][Phase.Process].ShouldHaveSingleItem();
                document["Foo"].ShouldBe("Bar");
                document.ContainsKey("Fizz").ShouldBeFalse();
            }

            [Test]
            public async Task AddsLocalRecursiveDirectoryMetadata()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/a/b/c.json", "{ \"Foo\": \"Bar\" }" },
                    { "/input/a/b/_directory.json", "{ \"Fizz\": \"Buzz\", \"Recursive\": \"false\" }" }
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
            public async Task LocalMetadataOverridesDirectoryMetadata()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/a/b/c.json", "{ \"Foo\": \"Bar\" }" },
                    { "/input/a/_directory.json", "{ \"Foo\": \"Buzz\" }" }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Data)][Phase.Process].ShouldHaveSingleItem();
                document["Foo"].ShouldBe("Bar");
            }

            [Test]
            public async Task CloserMetadataOverridesDirectoryMetadata()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/a/b/c.json", "{ }" },
                    { "/input/a/_directory.json", "{ \"Foo\": \"A\" }" },
                    { "/input/a/b/_directory.json", "{ \"Foo\": \"B\" }" }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Data)][Phase.Process].ShouldHaveSingleItem();
                document["Foo"].ShouldBe("B");
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

            [Test]
            public async Task ParsesJsonWithFrontMatter()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/a/b/c.json",
                        @"Fizz: Buzz
---
{ ""Foo"": ""Bar"" }"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Data)][Phase.Process].ShouldHaveSingleItem();
                document["Fizz"].ShouldBe("Buzz");
                document["Foo"].ShouldBe("Bar");
            }

            [Test]
            public async Task ParsesYamlWithFrontMatter()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/a/b/c.yaml",
                        @"Fizz: Buzz
---
Foo: Bar"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Data)][Phase.Process].ShouldHaveSingleItem();
                document["Fizz"].ShouldBe("Buzz");
                document["Foo"].ShouldBe("Bar");
            }

            [Test]
            public async Task FrontMatterOverridesDirectoryMetadata()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/a/b/c.json",
                        @"Fizz: Buzz
---
{ ""Foo"": ""Bar"" }"
                    },
                    { "/input/a/_directory.json", "{ \"Fizz\": \"Bazz\", \"Blue\": \"Green\" }" }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Data)][Phase.Process].ShouldHaveSingleItem();
                document["Fizz"].ShouldBe("Buzz");
                document["Blue"].ShouldBe("Green");
                document["Foo"].ShouldBe("Bar");
            }

            [Test]
            public async Task ProcessesJsonSidecarFile()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/a/b/c.json", "{ \"Foo\": \"Bar\" }" },
                    { "/input/a/b/_c.json", "{ \"Fizz\": \"Buzz\" }" }
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
            public async Task ContentOverridesSidecarFile()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/a/b/c.json", "{ \"Foo\": \"Bar\" }" },
                    { "/input/a/b/_c.json", "{ \"Foo\": \"Buzz\" }" }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Data)][Phase.Process].ShouldHaveSingleItem();
                document.Destination.ShouldBe("a/b/c.json");
                document["Foo"].ShouldBe("Bar");
            }

            [Test]
            public async Task EnumeratesValues()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/a/b/c.json",
                        @"Enumerate:
  - Apple
  - Orange
---
{ ""Foo"": ""Bar"" }"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Outputs[nameof(Data)][Phase.Process].Select(x => x["Current"]).ShouldBe(new[] { "Apple", "Orange" }, true);
            }

            [Test]
            public async Task JsonScriptReturn()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/a/b/c.json",
                        @"
Script: true
---
int a = 1;
int b = 2;
return $""{{ \""Foo\"": \""{ a + b }\"" }}"";"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Data)][Phase.Process].ShouldHaveSingleItem();
                document["Foo"].ShouldBe("3");
            }
        }
    }
}
