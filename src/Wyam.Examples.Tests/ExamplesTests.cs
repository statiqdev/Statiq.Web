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

namespace Wyam.Examples.Tests
{
    namespace Wyam.Examples.Tests
    {
        [TestFixture(Category = "ExcludeFromAppVeyor")]
        public class ExamplesTests
        {
            private static IEnumerable<string> _paths;

            public static IEnumerable<string> Paths
            {
                get
                {
                    if (_paths == null)
                    {
                        string rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "examples");
                        while (!Directory.Exists(rootPath))
                        {
                            rootPath = Directory.GetParent(rootPath).Parent.Parent.FullName;
                            rootPath = Path.Combine(rootPath, "examples");
                        }

                        _paths = Directory.EnumerateDirectories(rootPath);
                    }

                    return _paths;
                }
            }

            [OneTimeSetUp]
            public void SetUp()
            {
                Directory.CreateDirectory(Path.Combine(TestContext.CurrentContext.TestDirectory, "packages"));
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
                TestContext.Out.WriteLine($"Packages path: {packagesPath}");

                // When
                Process process = new Process();
                process.StartInfo.FileName = Path.Combine(TestContext.CurrentContext.TestDirectory, "Wyam.exe");
                process.StartInfo.Arguments = $@"--use-local-packages --packages-path ""{packagesPath}"" ""{example}""";
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
                Assert.AreEqual(0, process.ExitCode);
            }
        }
    }
}
