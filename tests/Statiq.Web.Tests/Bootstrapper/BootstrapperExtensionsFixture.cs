using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Statiq.App;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Web.Tests.Bootstrapper
{
    [TestFixture]
    public class BootstrapperExtensionsFixture : BaseFixture
    {
        public class AddWebTests : BootstrapperExtensionsFixture
        {
            [Test]
            public async Task SetsThemePathsFromSettings()
            {
                // Given
                ThemeManager themeManager = null;
                App.Bootstrapper bootstrapper = App.Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(
                        WebKeys.ThemePaths,
                        new NormalizedPath[]
                        {
                            "foo",
                            "bar"
                        })
                    .ConfigureEngine(engine => themeManager = engine.Services.GetRequiredService<ThemeManager>());

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                themeManager.ThemePaths.ShouldBe(new NormalizedPath[] { "foo", "bar" });
            }

            [Test]
            public async Task AddsThemeInputPaths()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddThemePath("foo");

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Engine.FileSystem.InputPaths.ShouldBe(new NormalizedPath[] { "theme/input", "foo/input", "input" });
            }

            [Test]
            public async Task AddsDefaultThemeInputPath()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>());

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Engine.FileSystem.InputPaths.ShouldBe(new NormalizedPath[] { "theme/input", "input" });
            }
        }

        public class AddThemePathTests : BootstrapperExtensionsFixture
        {
            [Test]
            public async Task AddsThemePath()
            {
                // Given
                ThemeManager themeManager = null;
                App.Bootstrapper bootstrapper = App.Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddThemePath("foo")
                    .ConfigureEngine(engine => themeManager = engine.Services.GetRequiredService<ThemeManager>());

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                themeManager.ThemePaths.ShouldBe(new NormalizedPath[] { "theme", "foo" });
            }
        }

        public class SetThemePathTests : BootstrapperExtensionsFixture
        {
            [Test]
            public async Task SetsThemePath()
            {
                // Given
                ThemeManager themeManager = null;
                App.Bootstrapper bootstrapper = App.Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>())
                    .SetThemePath("foo")
                    .ConfigureEngine(engine => themeManager = engine.Services.GetRequiredService<ThemeManager>());

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                themeManager.ThemePaths.ShouldBe(new NormalizedPath[] { "foo" });
            }
        }

        public class AddSettingTests : BootstrapperExtensionsFixture
        {
            [Test]
            public async Task ShouldAddInputPaths()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(WebKeys.InputPaths, new string[] { "foo", "bar" });

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Engine.FileSystem.InputPaths.ShouldBe(
                    new NormalizedPath[] { "theme/input", "foo", "bar" });
            }

            [Test]
            public async Task CommandLineShouldOverrideSettingsInputPaths()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper
                    .Factory
                    .CreateWeb(new string[] { "-i", "fizz", "-i", "buzz" })
                    .AddSetting(WebKeys.InputPaths, new string[] { "foo", "bar" });

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Engine.FileSystem.InputPaths.ShouldBe(
                    new NormalizedPath[] { "theme/input", "buzz", "fizz" });
            }
        }
    }
}