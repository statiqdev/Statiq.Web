using NUnit.Framework;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;
using Wyam.Core.Execution;
using Wyam.Core.Modules.Control;
using Wyam.Testing;
using Wyam.Testing.Modules;

namespace Wyam.Core.Tests.Modules.Control
{
    [TestFixture]
    [NonParallelizable]
    public class ConcatBranchFixture : BaseFixture
    {
        public class ExecuteTests : ConcatBranchFixture
        {
            [Test]
            public void ResultsInCorrectCounts()
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
                engine.Pipelines.Add(a, new ConcatBranch(b), c);

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(2, b.InputCount);
                Assert.AreEqual(8, c.InputCount);
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(6, b.OutputCount);
                Assert.AreEqual(32, c.OutputCount);
            }

            [Test]
            public void ResultsInCorrectCountsWithPredicate()
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
                engine.Pipelines.Add(a, new ConcatBranch(b).Where((x, y) => x.Content == "1"), c);

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(1, a.ExecuteCount);
                Assert.AreEqual(1, b.ExecuteCount);
                Assert.AreEqual(1, c.ExecuteCount);
                Assert.AreEqual(1, a.InputCount);
                Assert.AreEqual(1, b.InputCount);
                Assert.AreEqual(5, c.InputCount);
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(3, b.OutputCount);
                Assert.AreEqual(20, c.OutputCount);
            }
        }
    }
}
