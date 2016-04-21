using Cake.Core;
using Cake.Testing;
using Cake.Testing.Fixtures;
using NUnit.Framework;

namespace Cake.Wyam.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class WyamRunnerTests
    {
        public class RunMethodTests : WyamRunnerTests
        {
            [Test]
            public void ShouldThrowIfFixieRunnerWasNotFound()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture();
                fixture.GivenDefaultToolDoNotExist();

                // When, Then
                Assert.Throws<CakeException>(() => fixture.Run(), "Wyam: Could not locate executable.");
            }
            
            [TestCase("/bin/tools/Wyam/Wyam.exe", "/bin/tools/Wyam/Wyam.exe")]
            [TestCase("./tools/Wyam/Wyam.exe", "/Working/tools/Wyam/Wyam.exe")]
            public void ShouldUseWyamRunnerFromToolPathIfProvided(string toolPath, string expected)
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { ToolPath = toolPath } };
                fixture.GivenSettingsToolPathExist();

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual(expected, result.Path.FullPath);
            }
            
            
            [TestCase("C:/Wyam/Wyam.exe", "C:/Wyam/Wyam.exe")]
            public void ShouldUseWyamRunnerFromToolPathIfProvidedOnWindows(string toolPath, string expected)
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { ToolPath = toolPath } };
                fixture.GivenSettingsToolPathExist();

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual(expected, result.Path.FullPath);
            }

            [Test]
            public void ShouldFindWyamRunnerIfToolPathNotProvided()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture();

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("/Working/tools/Wyam.exe", result.Path.FullPath);
            }

            [Test]
            public void ShouldSetWorkingDirectory()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture();

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("/Working", result.Process.WorkingDirectory.FullPath);
            }

            [Test]
            public void ShouldThrowIfProcessWasNotStarted()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture();
                fixture.GivenProcessCannotStart();

                // When, Then
                Assert.Throws<CakeException>(() => fixture.Run(), "Wyam: Process was not started.");
            }

            [Test]
            public void ShouldThrowIfProcessHasANonZeroExitCode()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture();
                fixture.GivenProcessExitsWithCode(1);

                // When, Then
                Assert.Throws<CakeException>(() => fixture.Run(), "Wyam: Process returned an error.");
            }

            [Test]
            public void ShouldSetConfigurationFile()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { ConfigurationFile = "C:/temp/config.wyam" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--config \"C:/temp/config.wyam\"", result.Args);
            }

            [Test]
            public void ShouldSetInputDirectory()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { InputDirectory = "C:/temp" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--input \"C:/temp\"", result.Args);
            }

            [Test]
            public void ShouldSetOutputDirectory()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { OutputDirectory = "C:/temp" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--output \"C:/temp\"", result.Args);
            }

            [Test]
            public void ShouldSetNoCleanFlag()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { NoClean = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--noclean", result.Args);
            }

            [Test]
            public void ShouldSetNoCacheFlag()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { NoCache = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--nocache", result.Args);
            }

            [Test]
            public void ShouldSetUpdatePackagesFlag()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { UpdatePackages = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--update-packages", result.Args);
            }

            [Test]
            public void ShouldSetWatchFlag()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { Watch = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--watch", result.Args);
            }

            [Test]
            public void ShouldSetPreviewFlag()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { Preview = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--preview", result.Args);
            }

            [Test]
            public void ShouldSetPreviewFlagAndPort()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { Preview = true, PreviewPort = 5081 } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--preview 5081", result.Args);
            }

            [Test]
            public void ShouldSetPreviewFlagAndForceExtensions()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { Preview = true, PreviewForceExtensions = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--preview force-ext", result.Args);
            }

            [Test]
            public void ShouldSetPreviewFlagAndPortAndForceExtensions()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture
                {
                    Settings =
                        {
                            Preview = true,
                            PreviewPort = 5081,
                            PreviewForceExtensions = true
                        }
                };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--preview 5081 force-ext", result.Args);
            }

            [Test]
            public void ShouldSetOutputScriptsFlag()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { OutputScripts = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--output-scripts", result.Args);
            }

            [Test]
            public void ShouldAddLogFilePathToArgumentsIfSet()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { LogFilePath = @"/temp/log.txt" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--log \"/temp/log.txt\"", result.Args);
            }

            [Test]
            public void ShouldSetVerboseFlag()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { Verbose = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--verbose", result.Args);
            }

            [Test]
            public void ShouldSetPauseFlag()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { Pause = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--pause", result.Args);
            }
        }
    }
}