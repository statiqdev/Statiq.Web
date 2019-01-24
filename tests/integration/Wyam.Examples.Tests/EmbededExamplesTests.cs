using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.IO;
using Wyam.Configuration;
using Wyam.Core.Execution;

namespace Wyam.Examples.Tests
{
    [TestFixture(Category = "ExcludeFromAppVeyor")]
    public class EmbededExamplesTests
    {
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
        [Ignore("TODO")]
        [TestCaseSource(typeof(ExamplesTests), nameof(ExamplesTests.Paths))]
        public void ExecuteExample(string example)
        {
            Engine engine = new Engine();
            Configurator configurator = new Configurator(engine);
            configurator.Configure(new FilePath(example));

            engine.Execute();
        }
    }
}
