using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common;

namespace Wyam.Core.Tests
{
    [TestFixture]
    public class PipelineFixture
    {
        [Test]
        public void ProcessesPreviousDocumentsWithDistinctSources()
        {
            // Given
            Engine engine = new Engine();
            CountModule a = new CountModule("A")
            {
                CloneSource = true,
                AdditionalOutputs = 1
            };
            CountModule b = new CountModule("B")
            {
                CloneSource = true,
                AdditionalOutputs = 2
            };
            CountModule c = new CountModule("C")
            {
                CloneSource = true,
                AdditionalOutputs = 3
            };
            engine.Pipelines.Add("Count", true, a, b, c);

            // When
            engine.Execute();
            engine.Execute();

            // Then
            Assert.AreEqual(24, engine.Documents.FromPipeline("Count").Count());
            Assert.AreEqual(2, a.ExecuteCount);
            Assert.AreEqual(2, b.ExecuteCount);
            Assert.AreEqual(2, c.ExecuteCount);
            Assert.AreEqual(2, a.InputCount);
            Assert.AreEqual(4, b.InputCount);
            Assert.AreEqual(12, c.InputCount);
            Assert.AreEqual(4, a.OutputCount);
            Assert.AreEqual(12, b.OutputCount);
            Assert.AreEqual(48, c.OutputCount);
        }

        [Test]
        public void DoesNotProcessPreviousDocumentsWhenSameSource()
        {
            // Given
            Engine engine = new Engine();
            CountModule a = new CountModule("A")
            {
                CloneSource = true,
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
            engine.Pipelines.Add("Count", true, a, b, c);

            // When
            engine.Execute();
            engine.Execute();

            // Then
            Assert.AreEqual(24, engine.Documents.FromPipeline("Count").Count());
            Assert.AreEqual(2, a.ExecuteCount);
            Assert.AreEqual(2, b.ExecuteCount);
            Assert.AreEqual(2, c.ExecuteCount);
            Assert.AreEqual(2, a.InputCount);
            Assert.AreEqual(2, b.InputCount);
            Assert.AreEqual(6, c.InputCount);
            Assert.AreEqual(4, a.OutputCount);
            Assert.AreEqual(6, b.OutputCount);
            Assert.AreEqual(24, c.OutputCount);
        }

        // TODO: Test for reprocess if content changes

        [Test]
        public void AccessingMetadataWhileProcessDocumentsOnceIsTrueThrowsException()
        {
            // Given

            // When

            // Then
        }
    }
}
