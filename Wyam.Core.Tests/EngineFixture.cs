using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;
using Wyam.Core.Modules;
using Wyam.Core.Pipelines;


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
        public void ConfigureAddsPipelineAndModules()
        {
            // Given
            Engine engine = new Engine();
            string configScript = @"
                Pipelines.Add(
                    new ReadFiles(""*.cshtml""),
	                new WriteFiles("".html""));
            ";

            // When
            engine.Configure(configScript);

            // Then
            Assert.AreEqual(1, ((PipelineCollection)engine.Pipelines).Pipelines.Count());
            Assert.AreEqual(2, ((PipelineCollection)engine.Pipelines).Pipelines.First().Count);
        }

        [Test]
        public void ConfigureSupportsGlobalConstructorMethods()
        {
            // Given
            Engine engine = new Engine();
            string configScript = @"
                Pipelines.Add(
                    ReadFiles(""*.cshtml""),
	                WriteFiles("".html""));
            ";

            // When
            engine.Configure(configScript);

            // Then
            Assert.AreEqual(1, ((PipelineCollection)engine.Pipelines).Pipelines.Count());
            Assert.AreEqual(2, ((PipelineCollection)engine.Pipelines).Pipelines.First().Count);
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
            Assert.AreEqual(1, a.ExecuteCount);
            Assert.AreEqual(1, b.ExecuteCount);
            Assert.AreEqual(1, c.ExecuteCount);
            Assert.AreEqual(1, a.InputCount);
            Assert.AreEqual(2, b.InputCount);
            Assert.AreEqual(6, c.InputCount);
            Assert.AreEqual(2, a.OutputCount);
            Assert.AreEqual(6, b.OutputCount);
            Assert.AreEqual(24, c.OutputCount);
        }

        [Test]
        public void CompletedMetadataIsPopulatedAfterRun()
        {
            // Given
            Engine engine = new Engine();
            int c = 0;
            engine.Pipelines.Add("Pipeline",
                new Execute(x => new[]
                {
                    x.Clone((string)null, new Dictionary<string, object> { { c.ToString(), c++ } }), 
                    x.Clone((string)null, new Dictionary<string, object> { { c.ToString(), c++ } })
                }),
                new Execute(x => new[]
                {
                    x.Clone((string)null, new Dictionary<string, object> { { c.ToString(), c++ } })
                }));

            // When
            engine.Execute();

            // Then
            Assert.AreEqual(2, engine.Documents.FromPipeline("Pipeline").Count());

            Assert.IsTrue(engine.Documents.FromPipeline("Pipeline").First().Metadata.ContainsKey("0"));
            Assert.AreEqual(0, engine.Documents.FromPipeline("Pipeline").First().Metadata["0"]);
            Assert.IsTrue(engine.Documents.FromPipeline("Pipeline").First().Metadata.ContainsKey("2"));
            Assert.AreEqual(2, engine.Documents.FromPipeline("Pipeline").First().Metadata["2"]);
            Assert.IsFalse(engine.Documents.FromPipeline("Pipeline").First().Metadata.ContainsKey("1"));
            Assert.IsFalse(engine.Documents.FromPipeline("Pipeline").First().Metadata.ContainsKey("3"));

            Assert.IsTrue(engine.Documents.FromPipeline("Pipeline").Skip(1).First().Metadata.ContainsKey("1"));
            Assert.AreEqual(1, engine.Documents.FromPipeline("Pipeline").Skip(1).First().Metadata["1"]);
            Assert.IsTrue(engine.Documents.FromPipeline("Pipeline").Skip(1).First().Metadata.ContainsKey("3"));
            Assert.AreEqual(3, engine.Documents.FromPipeline("Pipeline").Skip(1).First().Metadata["3"]);
            Assert.IsFalse(engine.Documents.FromPipeline("Pipeline").Skip(1).First().Metadata.ContainsKey("0"));
            Assert.IsFalse(engine.Documents.FromPipeline("Pipeline").Skip(1).First().Metadata.ContainsKey("2"));
        }

        [Test]
        public void CompletedContentIsPopulatedAfterRun()
        {
            // Given
            Engine engine = new Engine();
            int c = 0;
            engine.Pipelines.Add(
                new Execute(x => new[]
                {
                    x.Clone((c++).ToString()), 
                    x.Clone((c++).ToString())
                }),
                new Execute(x => new[]
                {
                    x.Clone((c++).ToString())
                }),
                new Meta("Content", (x, y) => x.Content));

            // When
            engine.Execute();

            // Then
            Assert.AreEqual(2, engine.Documents.FromPipeline("Pipeline 1").Count());
            Assert.AreEqual("2", engine.Documents.FromPipeline("Pipeline 1").First().String("Content"));
            Assert.AreEqual("3", engine.Documents.FromPipeline("Pipeline 1").Skip(1).First().String("Content"));
        }
    }
}
