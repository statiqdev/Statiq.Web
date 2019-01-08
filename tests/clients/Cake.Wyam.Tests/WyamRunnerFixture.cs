using System.Collections.Generic;
using Cake.Core;
using Cake.Core.IO;
using Cake.Testing;
using Cake.Testing.Fixtures;
using NUnit.Framework;
using Shouldly;
using Wyam.Testing.Attributes;

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

            [TestCase("/bin/tools/Wyam/Wyam.dll", "/bin/tools/Wyam/Wyam.dll")]
            [TestCase("./tools/Wyam/Wyam.dll", "/Working/tools/Wyam/Wyam.dll")]
            public void ShouldUseWyamRunnerFromToolPathIfProvided(string toolPath, string expected)
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { ToolPath = toolPath } };
                fixture.GivenSettingsToolPathExist();

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Path.FullPath.ShouldBe("dotnet");
                result.Args.ShouldStartWith(expected);
            }

            [WindowsTestCase("C:/Wyam/Wyam.dll", "C:/Wyam/Wyam.dll")]
            public void ShouldUseWyamRunnerFromToolPathIfProvidedOnWindows(string toolPath, string expected)
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { ToolPath = toolPath } };
                fixture.GivenSettingsToolPathExist();

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Path.FullPath.ShouldBe("dotnet");
                result.Args.ShouldStartWith(expected);
            }

            [Test]
            public void ShouldFindWyamRunnerIfToolPathNotProvided()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture();

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Path.FullPath.ShouldBe("dotnet");
                result.Args.ShouldStartWith("/Working/tools/Wyam.dll");
            }

            [Test]
            public void ShouldSetWorkingDirectory()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture();

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Process.WorkingDirectory.FullPath.ShouldBe("/Working");
            }

            [Test]
            public void ShouldThrowIfProcessWasNotStarted()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture();
                fixture.GivenProcessCannotStart();

                // When, Then
                Should.Throw<CakeException>(() => fixture.Run(), "Wyam: Process was not started.");
            }

            [Test]
            public void ShouldThrowIfProcessHasANonZeroExitCode()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture();
                fixture.GivenProcessExitsWithCode(1);

                // When, Then
                Should.Throw<CakeException>(() => fixture.Run(), "Wyam: Process returned an error.");
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
                result.Args.ShouldBe("/Working/tools/Wyam.dll --watch \"/Working\"");
            }

            [Test]
            public void ShouldSetPreviewFlag()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { Preview = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Args.ShouldBe("/Working/tools/Wyam.dll --preview 5080 \"/Working\"");
            }

            [Test]
            public void ShouldSetPreviewFlagAndPort()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { Preview = true, PreviewPort = 5081 } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Args.ShouldBe("/Working/tools/Wyam.dll --preview 5081 \"/Working\"");
            }

            [Test]
            public void ShouldSetPreviewFlagAndForceExtensions()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { Preview = true, PreviewForceExtensions = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Args.ShouldBe("/Working/tools/Wyam.dll --preview 5080 --force-ext \"/Working\"");
            }

            [Test]
            public void ShouldSetPreviewFlagAndVirtualDirectory()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { Preview = true, PreviewVirtualDirectory = "foo" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Args.ShouldBe("/Working/tools/Wyam.dll --preview 5080 --virtual-dir \"foo\" \"/Working\"");
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
                result.Args.ShouldBe("/Working/tools/Wyam.dll --preview 5081 --force-ext \"/Working\"");
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
                result.Args.ShouldBe("/Working/tools/Wyam.dll --preview 5080 --preview-root \"PreviewRoot\" \"/Working\"");
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
                result.Args.ShouldBe("/Working/tools/Wyam.dll --preview 5081 --force-ext --preview-root \"PreviewRoot\" \"/Working\"");
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
                result.Args.ShouldBe("/Working/tools/Wyam.dll --input \"C:/temp\" \"/Working\"");
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
                result.Args.ShouldBe("/Working/tools/Wyam.dll --input \"C:/temp\" --input \"a/b\" \"/Working\"");
            }

            [Test]
            public void ShouldSetOutputPath()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { OutputPath = "C:/temp" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Args.ShouldBe("/Working/tools/Wyam.dll --output \"C:/temp\" \"/Working\"");
            }

            [Test]
            public void ShouldSetConfigurationFile()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { ConfigurationFile = "C:/temp/config.wyam" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Args.ShouldBe("/Working/tools/Wyam.dll --config \"C:/temp/config.wyam\" \"/Working\"");
            }

            [Test]
            public void ShouldSetUpdatePackagesFlag()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { UpdatePackages = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Args.ShouldBe("/Working/tools/Wyam.dll --update-packages \"/Working\"");
            }

            [Test]
            public void ShouldSetUseLocalPackagesFlag()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { UseLocalPackages = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Args.ShouldBe("/Working/tools/Wyam.dll --use-local-packages \"/Working\"");
            }

            [Test]
            public void ShouldSetPackagesPath()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { PackagesPath = "C:/temp" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Args.ShouldBe("/Working/tools/Wyam.dll --packages-path \"C:/temp\" \"/Working\"");
            }

            [Test]
            public void ShouldSetOutputScriptFlag()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { OutputScript = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Args.ShouldBe("/Working/tools/Wyam.dll --output-script \"/Working\"");
            }

            [Test]
            public void ShouldSetVerifyConfigFlag()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { VerifyConfig = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Args.ShouldBe("/Working/tools/Wyam.dll --verify-config --ignore-config-hash \"/Working\"");
            }

            [Test]
            public void ShouldSetNoCleanFlag()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { NoClean = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Args.ShouldBe("/Working/tools/Wyam.dll --noclean \"/Working\"");
            }

            [Test]
            public void ShouldSetNoCacheFlag()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { NoCache = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Args.ShouldBe("/Working/tools/Wyam.dll --nocache \"/Working\"");
            }

            [Test]
            public void ShouldSetVerboseFlag()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { Verbose = true } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Args.ShouldBe("/Working/tools/Wyam.dll --verbose \"/Working\"");
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
                result.Args.ShouldBe("/Working/tools/Wyam.dll --setting \"A=a\" --setting \"B=1\" --setting \"X=[y,1,x\\,y,z\\\"z]\" \"/Working\"");
            }

            [Test]
            public void ShouldSetLogFilePath()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { LogFilePath = "/temp/log.txt" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Args.ShouldBe("/Working/tools/Wyam.dll --log \"/temp/log.txt\" \"/Working\"");
            }

            [Test]
            public void ShouldSetWorkingPathAsRootPathIfNoneSpecified()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture();

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Args.ShouldBe("/Working/tools/Wyam.dll \"/Working\"");
            }

            [Test]
            public void ShouldSetAbsoluteRootPath()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { RootPath = "/a/b" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Args.ShouldBe("/Working/tools/Wyam.dll \"/a/b\"");
            }

            [Test]
            public void ShouldSetRelativeRootPath()
            {
                // Given
                WyamToolFixture fixture = new WyamToolFixture { Settings = { RootPath = "a/b" } };

                // When
                ToolFixtureResult result = fixture.Run();

                // Then
                result.Args.ShouldBe("/Working/tools/Wyam.dll \"/Working/a/b\"");
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
                result.Args.ShouldBe("/Working/tools/Wyam.dll --content-type .foo=application/xml --content-type bar=text/bar \"/Working\"");
            }
        }
    }
}