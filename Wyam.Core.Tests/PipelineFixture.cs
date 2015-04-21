using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Core.Modules;

namespace Wyam.Core.Tests
{
    [TestFixture]
    public class PipelineFixture
    {
        [Test]
        public void PersistedObjectIsReturnedFromPrepare()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline(engine);
            int c = 0;
            pipeline.AddModule(new Delegates(x => new[] { x.Clone(c++), x.Clone(c++) }, null));
            pipeline.AddModule(new Delegates(x => new[] { x.Clone(c++) }, null));

            // When
            PrepareTree prepareTree = pipeline.Prepare(metadata, new List<Metadata>());

            // Then
            Assert.AreEqual(2, prepareTree.RootBranch.Outputs.Count);
            Assert.AreEqual(0, prepareTree.RootBranch.Outputs[0].Context.PersistedObject);
            Assert.AreEqual(1, prepareTree.RootBranch.Outputs[1].Context.PersistedObject);
            Assert.AreEqual(1, prepareTree.RootBranch.Outputs[0].Outputs.Count);
            Assert.AreEqual(2, prepareTree.RootBranch.Outputs[0].Outputs[0].Context.PersistedObject);
            Assert.AreEqual(1, prepareTree.RootBranch.Outputs[1].Outputs.Count);
            Assert.AreEqual(3, prepareTree.RootBranch.Outputs[1].Outputs[0].Context.PersistedObject);
        }

        [Test]
        public void MetadataIsReturedFromPrepare()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline(engine);
            int c = 0;
            pipeline.AddModule(new Delegates(x => new[]
            {
                x.Clone(null, new Dictionary<string, object> { { c.ToString(), c++ } }), 
                x.Clone(null, new Dictionary<string, object> { { c.ToString(), c++ } })
            }, null));
            pipeline.AddModule(new Delegates(x => new[]
            {
                x.Clone(null, new Dictionary<string, object> { { c.ToString(), c++ } })
            }, null));

            // When
            PrepareTree prepareTree = pipeline.Prepare(metadata, new List<Metadata>());

            // Then
            Assert.AreEqual(2, prepareTree.RootBranch.Outputs.Count);
            Assert.IsTrue(prepareTree.RootBranch.Outputs[0].Context.Metadata.ContainsKey("0"));
            Assert.AreEqual(0, prepareTree.RootBranch.Outputs[0].Context.Metadata["0"]);
            Assert.IsFalse(prepareTree.RootBranch.Outputs[0].Context.Metadata.ContainsKey("1"));
            Assert.IsFalse(prepareTree.RootBranch.Outputs[0].Context.Metadata.ContainsKey("2"));
            Assert.IsFalse(prepareTree.RootBranch.Outputs[0].Context.Metadata.ContainsKey("3"));
            Assert.IsTrue(prepareTree.RootBranch.Outputs[1].Context.Metadata.ContainsKey("1"));
            Assert.AreEqual(1, prepareTree.RootBranch.Outputs[1].Context.Metadata["1"]);
            Assert.IsFalse(prepareTree.RootBranch.Outputs[1].Context.Metadata.ContainsKey("0"));
            Assert.IsFalse(prepareTree.RootBranch.Outputs[1].Context.Metadata.ContainsKey("2"));
            Assert.IsFalse(prepareTree.RootBranch.Outputs[1].Context.Metadata.ContainsKey("3"));

            Assert.AreEqual(1, prepareTree.RootBranch.Outputs[0].Outputs.Count);
            Assert.IsTrue(prepareTree.RootBranch.Outputs[0].Outputs[0].Context.Metadata.ContainsKey("0"));
            Assert.AreEqual(0, prepareTree.RootBranch.Outputs[0].Outputs[0].Context.Metadata["0"]);
            Assert.IsTrue(prepareTree.RootBranch.Outputs[0].Outputs[0].Context.Metadata.ContainsKey("2"));
            Assert.AreEqual(2, prepareTree.RootBranch.Outputs[0].Outputs[0].Context.Metadata["2"]);
            Assert.IsFalse(prepareTree.RootBranch.Outputs[0].Outputs[0].Context.Metadata.ContainsKey("1"));
            Assert.IsFalse(prepareTree.RootBranch.Outputs[0].Outputs[0].Context.Metadata.ContainsKey("3"));

            Assert.AreEqual(1, prepareTree.RootBranch.Outputs[1].Outputs.Count);
            Assert.IsTrue(prepareTree.RootBranch.Outputs[1].Outputs[0].Context.Metadata.ContainsKey("1"));
            Assert.AreEqual(1, prepareTree.RootBranch.Outputs[1].Outputs[0].Context.Metadata["1"]);
            Assert.IsTrue(prepareTree.RootBranch.Outputs[1].Outputs[0].Context.Metadata.ContainsKey("3"));
            Assert.AreEqual(3, prepareTree.RootBranch.Outputs[1].Outputs[0].Context.Metadata["3"]);
            Assert.IsFalse(prepareTree.RootBranch.Outputs[1].Outputs[0].Context.Metadata.ContainsKey("0"));
            Assert.IsFalse(prepareTree.RootBranch.Outputs[1].Outputs[0].Context.Metadata.ContainsKey("2"));
        }
    }
}
