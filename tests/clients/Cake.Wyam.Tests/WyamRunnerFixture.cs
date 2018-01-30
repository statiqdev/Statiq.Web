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
    public class WyamRunnerFixture
    {
        public class RunTests : WyamRunnerFixture
        {
            [Test]
            public void ShouldThrowIfFixieRunnerWasNotFound()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture();
                fixture.GivenDefaultToolDoNotExist();

                // When, Then
                Assert.Throws<CakeException>(() => fixture.Run(), "Wyam: Could not locate executable.");
            }

            [TestCase("/bin/tools/Wyam/Wyam.exe", "/bin/tools/Wyam/Wyam.exe")]
            [TestCase("./tools/Wyam/Wyam.exe", "/Working/tools/Wyam/Wyam.exe")]
            public void ShouldUseWyamRunnerFromToolPathIfProvided(string toolPath, string expected)
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { ToolPath = toolPath } };
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
                WyamToolFixture fixture = new WyamToolFixture { Settings = { ToolPath = toolPath } };
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
                WyamToolFixture fixture = new WyamToolFixture();

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("/Working/tools/Wyam.exe", result.Path.FullPath);
            }

            [Test]
            public void ShouldSetWorkingDirectory()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture();

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("/Working", result.Process.WorkingDirectory.FullPath);
            }

            [Test]
            public void ShouldThrowIfProcessWasNotStarted()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture();
                fixture.GivenProcessCannotStart();

                // When, Then
                Assert.Throws<CakeException>(() => fixture.Run(), "Wyam: Process was not started.");
            }

            [Test]
            public void ShouldThrowIfProcessHasANonZeroExitCode()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture();
                fixture.GivenProcessExitsWithCode(1);

                // When, Then
                Assert.Throws<CakeException>(() => fixture.Run(), "Wyam: Process returned an error.");
            }

            // Individual settings tests...

            [Test]
            public void ShouldSetWatchFlag()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { Watch = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--watch \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetPreviewFlag()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { Preview = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--preview 5080 \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetPreviewFlagAndPort()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { Preview = true, PreviewPort = 5081 } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--preview 5081 \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetPreviewFlagAndForceExtensions()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { Preview = true, PreviewForceExtensions = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--preview 5080 --force-ext \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetPreviewFlagAndVirtualDirectory()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { Preview = true, PreviewVirtualDirectory = "foo" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--preview 5080 --virtual-dir \"foo\" \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetPreviewFlagAndPortAndForceExtensions()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture
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
                WyamToolFixture fixture = new WyamToolFixture
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
                WyamToolFixture fixture = new WyamToolFixture
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
                WyamToolFixture fixture = new WyamToolFixture
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
                WyamToolFixture fixture = new WyamToolFixture
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
                WyamToolFixture fixture = new WyamToolFixture { Settings = { OutputPath = "C:/temp" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--output \"C:/temp\" \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetConfigurationFile()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { ConfigurationFile = "C:/temp/config.wyam" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--config \"C:/temp/config.wyam\" \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetUpdatePackagesFlag()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { UpdatePackages = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--update-packages \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetUseLocalPackagesFlag()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { UseLocalPackages = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--use-local-packages \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetPackagesPath()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { PackagesPath = "C:/temp" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--packages-path \"C:/temp\" \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetOutputScriptFlag()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { OutputScript = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--output-script \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetVerifyConfigFlag()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { VerifyConfig = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--verify-config \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetNoCleanFlag()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { NoClean = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--noclean \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetNoCacheFlag()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { NoCache = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--nocache \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetVerboseFlag()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { Verbose = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--verbose \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetSettingsMetadata()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture
                {
                    Settings =
                    {
                        Settings = new Dictionary<string, object>
                        {
                            { "A", "a" },
                            { "B", 1 },
                            { "X", new object[] { "y", 1, "x,y", "z\"z" } }
                        }
                    }
                };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--setting \"A=a\" --setting \"B=1\" --setting \"X=[y,1,x\\,y,z\\\"z]\" \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetLogFilePath()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { LogFilePath = @"/temp/log.txt" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--log \"/temp/log.txt\" \"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetWorkingPathAsRootPathIfNoneSpecified()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture();

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("\"/Working\"", result.Args);
            }

            [Test]
            public void ShouldSetAbsoluteRootPath()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { RootPath = "/a/b" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("\"/a/b\"", result.Args);
            }

            [Test]
            public void ShouldSetRelativeRootPath()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { RootPath = "a/b" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("\"/Working/a/b\"", result.Args);
            }

            [Test]
            public void ShouldSetContentType()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture
                {
                    Settings =
                    {
                        ContentTypes = new Dictionary<string, string>
                        {
                            { ".foo", " application/xml" },
                            { "bar", "text/bar" }
                        }
                    }
                };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                Assert.AreEqual("--content-type .foo=application/xml --content-type bar=text/bar \"/Working\"", result.Args);
            }
        }
    }
}