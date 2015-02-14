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
                Metadata.AsDynamic.TestString = ""teststring"";
                Metadata.AsDynamic.TestInt = 1234;
                Metadata.Set(""TestFloat"", 1234.567);
                Metadata.Set(""TestBool"", true);
            ";

            // When
            engine.Configure(configScript);

            // Then
            Assert.AreEqual("teststring", engine.Metadata.Get("TestString"));
            Assert.AreEqual(1234, engine.Metadata.Get("TestInt"));
            Assert.AreEqual(1234.567, engine.Metadata.AsDynamic.TestFloat);
            Assert.AreEqual(true, engine.Metadata.AsDynamic.TestBool);
        }

        [Test]
        public void ConfigureSetsAnonymousObjectMetadata()
        {
            // Given
            Engine engine = new Engine();
            string configScript = @"
                Metadata.AsDynamic.TestAnonymous = new { A = 1, B = ""b"" };
            ";

            // When
            engine.Configure(configScript);

            // Then
            Assert.AreEqual(1, engine.Metadata.AsDynamic.TestAnonymous.A);
            Assert.AreEqual("b", ((dynamic)engine.Metadata.Get("TestAnonymous")).B);
        }

        [Test]
        public void ConfigureAddsPipelineAndModules()
        {
            // Given
            Engine engine = new Engine();
            string configScript = @"
                Pipelines.Add(
	                new ReadFile(m => m.AsDynamic.InputPath + @""\*.cshtml""),
	                new WriteFile(m => string.Format(@""{0}\{1}.html"", PathHelper.GetRelativePath(m.AsDynamic.InputPath, m.AsDynamic.FilePath), m.AsDynamic.FileBase)));
            ";

            // When
            engine.Configure(configScript);

            // Then
            Assert.AreEqual(1, engine.Pipelines.AllPipelines.Count());
            Assert.AreEqual(2, engine.Pipelines.AllPipelines.First().Count);
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
            engine.Pipelines.Add(a, b, c);

            // When
            engine.Execute();

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
