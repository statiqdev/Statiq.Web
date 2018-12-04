using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

namespace Wyam.Examples.Tests
{
    [TestFixture(Category = "ExcludeFromBuildServer")]
    public class ExamplesTests
    {
        private static IEnumerable<string> _paths;

        public static IEnumerable<string> Paths
        {
            get
            {
                if (_paths == null)
                {
                    string rootPath = AppDomain.CurrentDomain.BaseDirectory;
                    string examplesPath = Path.Combine(rootPath, "examples");
                    while (!Directory.Exists(examplesPath))
                    {
                        rootPath = Path.GetDirectoryName(rootPath);
                        examplesPath = Path.Combine(rootPath, "examples");
                    }

                    _paths = Directory.EnumerateDirectories(examplesPath);
                }

                return _paths;
            }
        }

        [OneTimeSetUp]
        public void SetUp()
        {
            Directory.CreateDirectory(Path.Combine(TestContext.CurrentContext.TestDirectory, "packages"));

            // Make sure there a "Wyam.runtimeconfig.json" since it won't get copied over with CopyLocalLockFileAssemblies
            // This is a big hack, but hopefully it'll go away when we switch to running via a project and/or global tool
            string runtimePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Wyam.runtimeconfig.json");
            if (!File.Exists(runtimePath))
            {
                File.WriteAllText(runtimePath, @"{ ""runtimeOptions"": { ""tfm"": ""netcoreapp2.1"", ""framework"": { ""name"": ""Microsoft.NETCore.App"", ""version"": ""2.1.0"" } } }");
            }
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Directory.Delete(Path.Combine(TestContext.CurrentContext.TestDirectory, "packages"), true);
        }

        [Test]
        [TestCaseSource(typeof(ExamplesTests), nameof(Paths))]
        public void ExecuteExample(string example)
        {
            // Given
            string packagesPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "packages");
            string arguments = "\"" + Path.Combine(TestContext.CurrentContext.TestDirectory, "Wyam.dll") + "\""
                + $@" --no-output-config-assembly --use-local-packages --verbose --packages-path ""{packagesPath}"" ""{example}""";
            TestContext.Out.WriteLine($"Packages path: {packagesPath}");
            TestContext.Out.WriteLine($"Command: dotnet {arguments}");

            // When
            Process process = new Process();
            process.StartInfo.FileName = "dotnet";
            process.StartInfo.Arguments = arguments;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += (s, e) => TestContext.Out.WriteLine(e.Data);
            process.ErrorDataReceived += (s, e) => TestContext.Out.WriteLine(e.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            // Then
            process.ExitCode.ShouldBe(0);
        }
    }
}
