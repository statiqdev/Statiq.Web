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

[assembly: SuppressMessage("", "RCS1008", Justification = "Stop !")]
[assembly: SuppressMessage("", "RCS1009", Justification = "Stop !")]
[assembly: SuppressMessage("", "SA1503", Justification = "Stop !")]
[assembly: SuppressMessage("", "SA1401", Justification = "Stop !")]
[assembly: SuppressMessage("", "IDE0008", Justification = "Stop !")]
[assembly: SuppressMessage("", "RCS1012", Justification = "Stop !")]
[assembly: SuppressMessage("", "SA1401", Justification = "Stop !")]
[assembly: SuppressMessage("", "SA1310", Justification = "Stop !")]
[assembly: SuppressMessage("", "SA1515", Justification = "Stop !")]
[assembly: SuppressMessage("", "SA1005", Justification = "Stop !")]

namespace Wyam.Examples.Tests
{
    [TestFixture]
    //[TestFixture(Category = "ExcludeFromAppVeyor")]
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
            var engine = new Engine();
            var configurator = new Configurator(engine);
            configurator.Configure(new FilePath(example));

            engine.Execute();
        }
    }
}
