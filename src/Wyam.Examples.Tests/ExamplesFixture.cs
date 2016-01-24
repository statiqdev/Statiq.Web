using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Tracing;
using Wyam.Core;
using Wyam.Testing;

namespace Wyam.Examples.Tests
{
    namespace Wyam.Examples.Tests
    {
        [TestFixture(Category = "ExcludeFromAppVeyor")]
        public class ExamplesFixture : TraceListenerFixture
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

            [Test]
            [TestCaseSource(typeof(ExamplesFixture), nameof(Paths))]
            public void ExecuteAllExamples(string example)
            {
                Engine engine = new Engine();
                engine.FileSystem.RootPath = example;
                engine.Config.Assemblies.LoadDirectory(TestContext.CurrentContext.TestDirectory);
                string config = Path.Combine(example, "config.wyam");
                if (File.Exists(config))
                {
                    engine.Configure(File.ReadAllText(config));
                }
                engine.Execute();
            }
        }
    }
}
