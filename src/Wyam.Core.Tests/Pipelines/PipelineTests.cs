using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common;
using Wyam.Common.Tracing;
using Wyam.Testing;

namespace Wyam.Core.Tests.Pipelines
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class PipelineTests : BaseFixture
    {
        public class ExecuteMethodTests : PipelineTests
        {
            [Test]
            public void ReprocessesPreviousDocumentsWithDistinctSources()
            {
                // Given
                Engine engine = new Engine();
                engine.CleanOutputFolderOnExecute = false;
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
                engine.CleanOutputFolderOnExecute = false;
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
                a.Value = 0; // Reset a.Value so output from a has same content
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

            [Test]
            public void ReprocessPreviousDocumentsWithDifferentContent()
            {
                // Given
                Engine engine = new Engine();
                engine.CleanOutputFolderOnExecute = false;
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
                Assert.AreEqual(4, b.InputCount);
                Assert.AreEqual(12, c.InputCount);
                Assert.AreEqual(4, a.OutputCount);
                Assert.AreEqual(12, b.OutputCount);
                Assert.AreEqual(48, c.OutputCount);
            }
        }
    }
}
