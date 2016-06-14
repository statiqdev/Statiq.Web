using System.Collections.Generic;
using Cake.Core;
using Cake.Core.IO;
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

            // Individual settings tests...

            [Test]
            public void ShouldSetWatchFlag()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { Watch = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--watch \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetPreviewFlag()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { Preview = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--preview 5080 \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetPreviewFlagAndPort()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { Preview = true, PreviewPort = 5081 } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--preview 5081 \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetPreviewFlagAndForceExtensions()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { Preview = true, PreviewForceExtensions = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--preview 5080 --force-ext \"/Working\"", result.Args);
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
                Assert.AreEqual("--preview 5081 --force-ext \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetPreviewRoot()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture
                {
                    Settings =
                    {
                        Preview = true,
                        PreviewRoot = "PreviewRoot"
                    }
                };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--preview 5080 --preview-root \"PreviewRoot\" \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetPreviewFlagAndPortAndForceExtensionsAndPreviewRoot()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture
                {
                    Settings =
                    {
                        Preview = true,
                        PreviewPort = 5081,
                        PreviewForceExtensions = true,
                        PreviewRoot = "PreviewRoot"
                    }
                };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--preview 5081 --force-ext --preview-root \"PreviewRoot\" \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetInputPaths()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture
                {
                    Settings =
                    {
                        InputPaths = new DirectoryPath[] { "C:/temp" }
                    }
                };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--input \"C:/temp\" \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetMultipleInputPaths()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture
                {
                    Settings =
                    {
                        InputPaths = new DirectoryPath[]
                        {
                            "C:/temp",
                            "a/b"
                        }
                    }
                };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--input \"C:/temp\" --input \"a/b\" \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetOutputPath()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { OutputPath = "C:/temp" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--output \"C:/temp\" \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetConfigurationFile()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { ConfigurationFile = "C:/temp/config.wyam" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--config \"C:/temp/config.wyam\" \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetUpdatePackagesFlag()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { UpdatePackages = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--update-packages \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetUseLocalPackagesFlag()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { UseLocalPackages = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--use-local-packages \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetPackagesPath()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { PackagesPath = "C:/temp" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--packages-path \"C:/temp\" \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetOutputScriptFlag()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { OutputScript = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--output-script \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetVerifyConfigFlag()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { VerifyConfig = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--verify-config \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetNoCleanFlag()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { NoClean = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--noclean \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetNoCacheFlag()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { NoCache = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--nocache \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetVerboseFlag()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { Verbose = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--verbose \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetGlobalMetadata()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture
                {
                    Settings =
                    {
                        GlobalMetadata = new Dictionary<string, string>
                        {
                            { "A", "a" },
                            { "B", "C" }
                        }
                    }
                };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--global \"A=a\" --global \"B=C\" \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetLogFilePath()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { LogFilePath = @"/temp/log.txt" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--log \"/temp/log.txt\" \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetWorkingPathAsRootPathIfNoneSpecified()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture();

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("\"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetAbsoluteRootPath()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { RootPath = "/a/b" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("\"/a/b\"", result.Args);
            }

            [Test]
            public void ShouldSetRelativeRootPath()
            {
                // Given
                WyamRunnerFixture fixture = new WyamRunnerFixture { Settings = { RootPath = "a/b" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("\"/Working/a/b\"", result.Args);
            }
        }
    }
}