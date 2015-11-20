using NUnit.Framework;
using Wyam.Core.Modules.Control;

namespace Wyam.Core.Tests.Modules.Control
{
    [TestFixture]
    public class IfFixture
    {
        [Test]
        public void IfResultsInCorrectCounts()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            CountModule a = new CountModule("A")
            {
                AdditionalOutputs = 2
            };
            CountModule b = new CountModule("B")
            {
                AdditionalOutputs = 2
            };
            CountModule c = new CountModule("C")
            {
                AdditionalOutputs = 3
            };
            engine.Pipelines.Add(a, new If((x, y) => x.Content == "1", b), c);

            // When
            engine.Execute();

            // Then
            Assert.AreEqual(1, a.ExecuteCount);
            Assert.AreEqual(1, b.ExecuteCount);
            Assert.AreEqual(1, c.ExecuteCount);
            Assert.AreEqual(1, a.InputCount);
            Assert.AreEqual(1, b.InputCount);
            Assert.AreEqual(5, c.InputCount);
            Assert.AreEqual(3, a.OutputCount);
            Assert.AreEqual(3, b.OutputCount);
            Assert.AreEqual(20, c.OutputCount);
        }

        [Test]
        public void ElseIfResultsInCorrectCounts()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            CountModule a = new CountModule("A")
            {
                AdditionalOutputs = 2
            };
            CountModule b = new CountModule("B")
            {
                AdditionalOutputs = 2
            };
            CountModule c = new CountModule("C")
            {
                AdditionalOutputs = 3
            };
            CountModule d = new CountModule("B")
            {
                AdditionalOutputs = 2
            };
            engine.Pipelines.Add(
                a, 
                new If((x, y) => x.Content == "1", b)
                    .ElseIf((x, y) => x.Content == "2", c),
                d);

            // When
            engine.Execute();

            // Then
            Assert.AreEqual(1, a.ExecuteCount);
            Assert.AreEqual(1, b.ExecuteCount);
            Assert.AreEqual(1, c.ExecuteCount);
            Assert.AreEqual(1, d.ExecuteCount);
            Assert.AreEqual(1, a.InputCount);
            Assert.AreEqual(1, b.InputCount);
            Assert.AreEqual(1, c.InputCount);
            Assert.AreEqual(8, d.InputCount);
            Assert.AreEqual(3, a.OutputCount);
            Assert.AreEqual(3, b.OutputCount);
            Assert.AreEqual(4, c.OutputCount);
            Assert.AreEqual(24, d.OutputCount);
        }

        [Test]
        public void ElseResultsInCorrectCounts()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            CountModule a = new CountModule("A")
            {
                AdditionalOutputs = 2
            };
            CountModule b = new CountModule("B")
            {
                AdditionalOutputs = 2
            };
            CountModule c = new CountModule("C")
            {
                AdditionalOutputs = 3
            };
            CountModule d = new CountModule("B")
            {
                AdditionalOutputs = 2
            };
            engine.Pipelines.Add(
                a,
                new If((x, y) => x.Content == "1", b)
                    .Else(c),
                d);

            // When
            engine.Execute();

            // Then
            Assert.AreEqual(1, a.ExecuteCount);
            Assert.AreEqual(1, b.ExecuteCount);
            Assert.AreEqual(1, c.ExecuteCount);
            Assert.AreEqual(1, d.ExecuteCount);
            Assert.AreEqual(1, a.InputCount);
            Assert.AreEqual(1, b.InputCount);
            Assert.AreEqual(2, c.InputCount);
            Assert.AreEqual(11, d.InputCount);
            Assert.AreEqual(3, a.OutputCount);
            Assert.AreEqual(3, b.OutputCount);
            Assert.AreEqual(8, c.OutputCount);
            Assert.AreEqual(33, d.OutputCount);
        }

        [Test]
        public void IfElseAndElseResultsInCorrectCounts()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            CountModule a = new CountModule("A")
            {
                AdditionalOutputs = 3
            };
            CountModule b = new CountModule("B")
            {
                AdditionalOutputs = 2
            };
            CountModule c = new CountModule("C")
            {
                AdditionalOutputs = 3
            };
            CountModule d = new CountModule("B")
            {
                AdditionalOutputs = 2
            };
            CountModule e = new CountModule("B")
            {
                AdditionalOutputs = 3
            };
            engine.Pipelines.Add(
                a,
                new If((x, y) => x.Content == "1", b)
                    .ElseIf((x, y) => x.Content == "3", c)
                    .Else(d),
                e);

            // When
            engine.Execute();

            // Then
            Assert.AreEqual(1, a.ExecuteCount);
            Assert.AreEqual(1, b.ExecuteCount);
            Assert.AreEqual(1, c.ExecuteCount);
            Assert.AreEqual(1, d.ExecuteCount);
            Assert.AreEqual(1, e.ExecuteCount);
            Assert.AreEqual(1, a.InputCount);
            Assert.AreEqual(1, b.InputCount);
            Assert.AreEqual(1, c.InputCount);
            Assert.AreEqual(2, d.InputCount);
            Assert.AreEqual(13, e.InputCount);
            Assert.AreEqual(4, a.OutputCount);
            Assert.AreEqual(3, b.OutputCount);
            Assert.AreEqual(4, c.OutputCount);
            Assert.AreEqual(6, d.OutputCount);
            Assert.AreEqual(52, e.OutputCount);
        }
    }
}
