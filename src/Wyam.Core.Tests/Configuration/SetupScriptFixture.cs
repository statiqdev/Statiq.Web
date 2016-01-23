using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.IO;
using Wyam.Common.Tracing;
using Wyam.Core.Configuration;
using Wyam.Core.IO;
using Wyam.Core.NuGet;

namespace Wyam.Core.Tests.Configuration
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class SetupScriptFixture
    {
        [Test]
        public void SetupMaintainsFolders()
        {
            // Given
            Trace.AddListener(new TestTraceListener());
            FileSystem fileSystem = new FileSystem
            {
                RootPath = @"C:/A",
                OutputPath = "C"
            };
            fileSystem.InputPaths.Add("B");
            PackagesCollection packages = new PackagesCollection(fileSystem);
            AssemblyCollection assemblies = new AssemblyCollection();
            string setup = @"
Assemblies.Load("""");
";
            SetupScript setupScript = new SetupScript(setup);
            setupScript.Compile();

            // When
            setupScript.Invoke(packages, assemblies, fileSystem);

            // Then
            Assert.AreEqual(@"C:/A", fileSystem.RootPath.FullPath);
            Assert.AreEqual(@"C", fileSystem.OutputPath.FullPath);
            CollectionAssert.AreEquivalent(new [] { "input", "B" }, fileSystem.InputPaths.Select(x => x.FullPath));
        }

        [Test]
        public void SetupModifiesFolders()
        {
            // Given
            Trace.AddListener(new TestTraceListener());
            FileSystem fileSystem = new FileSystem
            {
                RootPath = @"C:/A",
                OutputPath = "C"
            };
            fileSystem.InputPaths.Add("B");
            PackagesCollection packages = new PackagesCollection(fileSystem);
            AssemblyCollection assemblies = new AssemblyCollection();
            string setup = @"
FileSystem.RootPath = @""C:\X"";
FileSystem.InputPaths.Add(""Y"");
FileSystem.OutputPath = ""Z"";
";
            SetupScript setupScript = new SetupScript(setup);
            setupScript.Compile();

            // When
            setupScript.Invoke(packages, assemblies, fileSystem);

            // Then
            Assert.AreEqual(@"C:/X", fileSystem.RootPath.FullPath);
            Assert.AreEqual(@"Z", fileSystem.OutputPath.FullPath);
            CollectionAssert.AreEquivalent(new[] { "input", "B", "Y" }, fileSystem.InputPaths.Select(x => x.FullPath));
        }
    }
}
