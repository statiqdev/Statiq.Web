using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.App;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Web.Tests.Themes
{
    [TestFixture]
    public class ThemeManagerFixture : BaseFixture
    {
        public class AddPathsFromSettingsTests : ThemeManagerFixture
        {
            [Test]
            public async Task ShouldAddThemePathsBeforeDefaultInputPath()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Engine.FileSystem.InputPaths.ShouldBe(new NormalizedPath[]
                {
                    "theme/input",
                    "input"
                });
            }

            [Test]
            public async Task ShouldAddThemePathsBeforeCliInputPaths()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(new[] { "-i", "foo" });

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Engine.FileSystem.InputPaths.ShouldBe(new NormalizedPath[]
                {
                    "theme/input",
                    "foo"
                });
            }

            [Test]
            public async Task ShouldAddThemePathsBeforeBootstrapperInputPaths()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddInputPath("foo");

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Engine.FileSystem.InputPaths.ShouldBe(new NormalizedPath[]
                {
                    "theme/input",
                    "input",
                    "foo"
                });
            }

            [Test]
            public async Task ShouldAddThemePathsInCorrectOrder()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddThemePath("foo");

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Engine.FileSystem.InputPaths.ShouldBe(new NormalizedPath[]
                {
                    "theme/input",
                    "foo/input",
                    "input"
                });
            }

            [Test]
            public async Task ShouldSetThemePath()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .SetThemePath("foo");

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Engine.FileSystem.InputPaths.ShouldBe(new NormalizedPath[]
                {
                    "foo/input",
                    "input"
                });
            }

            [Test]
            public async Task ShouldAddThemePathsBeforeSettingsInputPaths()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(WebKeys.InputPaths, new[] { "foo" });

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Engine.FileSystem.InputPaths.ShouldBe(new NormalizedPath[]
                {
                    "theme/input",
                    "foo"
                });
            }
        }
    }
}