using System;
using System.Diagnostics;
using NUnit.Framework;
using Trace = Wyam.Core.Modules.Extensibility.Trace;

namespace Wyam.Core.Tests.Modules.Extensibility
{
    [TestFixture]
    public class TraceFixture
    {
        [TestCase(TraceEventType.Critical)]
        [TestCase(TraceEventType.Error)]
        [TestCase(TraceEventType.Warning)]
        public void TestTraceListenerThrowsOnErrorOrWarning(TraceEventType traceEventType)
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.Pipelines.Add(new Trace(traceEventType.ToString()).EventType(traceEventType));

            // When/Then
            Assert.Throws<Exception>(() => engine.Execute());
        }
    }
}
