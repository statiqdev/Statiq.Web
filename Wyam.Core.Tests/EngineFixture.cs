using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;


namespace Wyam.Core.Tests
{
    [TestFixture]
    public class EngineFixture
    {
        [Test]
        public void ConfigureSetsPrimitiveMetadata()
        {
            // Given
            Engine engine = new Engine();
            string configScript = @"
                Metadata[""TestString""] = ""teststring"";
                Metadata[""TestInt""] = 1234;
                Metadata[""TestFloat""] = 1234.567;
                Metadata[""TestBool""] = true;
            ";

            // When
            engine.Configure(configScript);

            // Then
            Assert.AreEqual("teststring", engine.Metadata["TestString"]);
            Assert.AreEqual(1234, engine.Metadata["TestInt"]);
            Assert.AreEqual(1234.567, engine.Metadata["TestFloat"]);
            Assert.AreEqual(true, engine.Metadata["TestBool"]);
        }

        [Test]
        public void ConfigureSetsAnonymousObjectMetadata()
        {
            // Given
            Engine engine = new Engine();
            string configScript = @"
                Metadata[""TestAnonymous""] = new { A = 1, B = ""b"" };
            ";

            // When
            engine.Configure(configScript);

            // Then
            Assert.AreEqual(1, ((dynamic)engine.Metadata["TestAnonymous"]).A);
            Assert.AreEqual("b", ((dynamic)engine.Metadata["TestAnonymous"]).B);
        }

        [Test]
        public void ConfigureAddsPipelineAndModules()
        {
            // Given
            Engine engine = new Engine();
            string configScript = @"
                Pipelines.Create()
	                .ReadFiles(""*.cshtml"")
	                .WriteFiles("".html"");
            ";

            // When
            engine.Configure(configScript);

            // Then
            Assert.AreEqual(1, engine.Pipelines.All.Count());
            Assert.AreEqual(2, engine.Pipelines.All.First().Count);
        }

        [Test]
        public void ExecuteResultsInCorrectCounts()
        {
            // Given
            Engine engine = new Engine();
            CountModule a = new CountModule("A")
            {
                AdditionalOutputs = 1
            };
            CountModule b = new CountModule("B")
            {
                AdditionalOutputs = 2
            };
            CountModule c = new CountModule("C")
            {
                AdditionalOutputs = 3
            };
            engine.Pipelines.Create()
                .AddModule(a)
                .AddModule(b)
                .AddModule(c);

            // When
            engine.Run();

            // Then
            Assert.AreEqual(1, a.PrepareCount);
            Assert.AreEqual(2, b.PrepareCount);
            Assert.AreEqual(6, c.PrepareCount);
            Assert.AreEqual(2, a.OutputCount);
            Assert.AreEqual(6, b.OutputCount);
            Assert.AreEqual(24, c.OutputCount);
            Assert.AreEqual(1, a.ExecuteCount);
            Assert.AreEqual(2, b.ExecuteCount);
            Assert.AreEqual(6, c.ExecuteCount);
        }
    }
}
