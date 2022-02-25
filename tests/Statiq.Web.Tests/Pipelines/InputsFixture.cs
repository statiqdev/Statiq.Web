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
    public class InputsFixture : BaseFixture
    {
        public class ExecuteTests : InputsFixture
        {
            [TestCase("/input/foo.statiq", "foo")]
            [TestCase("/input/foo.txt.statiq", "foo.txt")]
            public async Task ShouldRemoveStatiqFileExtension(string path, string destinationPath)
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { path, string.Empty }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Inputs)][Phase.Process].ShouldHaveSingleItem();
                document.Destination.ShouldBe(destinationPath);
            }

            [Test]
            public async Task ShouldExcludeUnderscoreFiles()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/foo", string.Empty },
                    { "/input/_foo", string.Empty }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Inputs)][Phase.Process].ShouldHaveSingleItem();
                document.Destination.ShouldBe("foo");
            }

            [Test]
            public async Task ShouldNotExcludeUnderscoreStatiqFiles()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/foo", string.Empty },
                    { "/input/_foo.statiq", string.Empty },
                    { "/input/_foo.txt.statiq", string.Empty }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Outputs[nameof(Inputs)][Phase.Process]
                    .Select(x => x.Destination.FullPath)
                    .ShouldBe(new[] { "foo", "_foo", "_foo.txt" }, true);
            }

            [Test]
            public async Task ShouldChangeDestinationOfStatiqFile()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/_foo.txt.statiq",
                        @"DestinationPath: bar.baz
---
Fizz buzz"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Assets)][Phase.Output].ShouldHaveSingleItem();
                document.Destination.ShouldBe("bar.baz");
            }

            [Test]
            public async Task ShouldProcessFrontMatterForStatiqFile()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/foo.txt.statiq",
                        @"Fizz: Buzz
---
Fizz buzz"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Inputs)][Phase.Process].ShouldHaveSingleItem();
                document.Destination.ShouldBe("foo.txt");
                document.GetString("Fizz").ShouldBe("Buzz");
            }

            [Test]
            public async Task ShouldSkipFrontMatterDelimiterInStatiqBodyFileWithEmptyFrontMatter()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/foo.txt.statiq",
                        @"
---
Fizz: Buzz
---
Fizz buzz"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Inputs)][Phase.Process].ShouldHaveSingleItem();
                document.Destination.ShouldBe("foo.txt");
                document.GetString("Fizz").ShouldBeNull();
            }
        }
    }
}