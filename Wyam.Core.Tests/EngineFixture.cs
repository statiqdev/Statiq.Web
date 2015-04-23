using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;
using Wyam.Core.Modules;


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
	                .AddModule(new ReadFiles(""*.cshtml""))
	                .WriteFiles("".html"");
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
            Assert.AreEqual(2, a.ExecuteCount);
            Assert.AreEqual(6, b.ExecuteCount);
            Assert.AreEqual(24, c.ExecuteCount);
        }

        [Test]
        public void AllMetadataIsPopulatedAfterRun()
        {
            // Given
            Engine engine = new Engine();
            int c = 0;
            engine.Pipelines.Create()
                .AddModule(new Delegates(x => new[]
                {
                    x.Clone(null, new Dictionary<string, object> { { c.ToString(), c++ } }), 
                    x.Clone(null, new Dictionary<string, object> { { c.ToString(), c++ } })
                }, null))
                .AddModule(new Delegates(x => new[]
                {
                    x.Clone(null, new Dictionary<string, object> { { c.ToString(), c++ } })
                }, null));

            // When
            engine.Run();

            // Then
            Assert.AreEqual(2, engine.AllMetadata.Count);
            
            Assert.IsTrue(engine.AllMetadata[0].ContainsKey("0"));
            Assert.AreEqual(0, engine.AllMetadata[0]["0"]);
            Assert.IsTrue(engine.AllMetadata[0].ContainsKey("2"));
            Assert.AreEqual(2, engine.AllMetadata[0]["2"]);
            Assert.IsFalse(engine.AllMetadata[0].ContainsKey("1"));
            Assert.IsFalse(engine.AllMetadata[0].ContainsKey("3"));

            Assert.IsTrue(engine.AllMetadata[1].ContainsKey("1"));
            Assert.AreEqual(1, engine.AllMetadata[1]["1"]);
            Assert.IsTrue(engine.AllMetadata[1].ContainsKey("3"));
            Assert.AreEqual(3, engine.AllMetadata[1]["3"]);
            Assert.IsFalse(engine.AllMetadata[1].ContainsKey("0"));
            Assert.IsFalse(engine.AllMetadata[1].ContainsKey("2"));
        }

        [Test]
        public void PersistedObjectIsPassedToExecute()
        {
            // Given
            Engine engine = new Engine();
            List<object> persistedObjects = new List<object>();
            int c = 0;
            engine.Pipelines.Create()
                .AddModule(new Delegates(x => new[] { x.Clone(c++), x.Clone(c++) }, (x, y) =>
                {
                    persistedObjects.Add(x.PersistedObject);
                    return y;
                }))
                .AddModule(new Delegates(x => new[] { x.Clone(c++) }, (x, y) =>
                {
                    persistedObjects.Add(x.PersistedObject);
                    return y;
                }));

            // When
            engine.Run();

            // Then
            Assert.AreEqual(4, persistedObjects.Count);
            Assert.AreEqual(0, persistedObjects[0]);
            Assert.AreEqual(1, persistedObjects[1]);
            Assert.AreEqual(2, persistedObjects[2]);
            Assert.AreEqual(3, persistedObjects[3]);
        }

        [Test]
        public void MetadataIsPassedToExecute()
        {
            // Given
            Engine engine = new Engine();
            int c = 0;
            List<IEnumerable<string>> metadata = new List<IEnumerable<string>>();
            engine.Pipelines.Create()
                .AddModule(new Delegates(x => new[]
                {
                    x.Clone(null, new Dictionary<string, object> { { "!" + c.ToString(), c++ } }), 
                    x.Clone(null, new Dictionary<string, object> { { "!" + c.ToString(), c++ } })
                }, (x, y) =>
                {
                    metadata.Add(x.Metadata.Where(z => z.Key.StartsWith("!")).Select(z => z.Key).ToList());
                    return y;
                }))
                .AddModule(new Delegates(x => new[]
                {
                    x.Clone(null, new Dictionary<string, object> { { "!" + c.ToString(), c++ } })
                }, (x, y) =>
                {
                    metadata.Add(x.Metadata.Where(z => z.Key.StartsWith("!")).Select(z => z.Key).ToList());
                    return y;
                }));

            // When
            engine.Run();

            // Then
            Assert.AreEqual(4, metadata.Count);
            CollectionAssert.AreEquivalent(new[] { "!0" }, metadata[0]);
            CollectionAssert.AreEquivalent(new[] { "!1" }, metadata[1]);
            CollectionAssert.AreEquivalent(new[] { "!0", "!2" }, metadata[2]);
            CollectionAssert.AreEquivalent(new[] { "!1", "!3" }, metadata[3]);
        }

        [Test]
        public void RecycledPersistedObjectIsNotPassedToExecute()
        {
            // Given
            Engine engine = new Engine();
            List<object> persistedObjects = new List<object>();
            engine.Pipelines.Create()
                .AddModule(new Delegates(x => new[] { x.Clone("A") }, (x, y) =>
                {
                    persistedObjects.Add(x.PersistedObject);
                    return y;
                }))
                .AddModule(new Delegates(x => new[] { x }, (x, y) =>
                {
                    persistedObjects.Add(x.PersistedObject);
                    return y;
                }));

            // When
            engine.Run();

            // Then
            Assert.AreEqual("A", persistedObjects[0]);
            Assert.AreEqual(null, persistedObjects[1]);
        }
    }
}
