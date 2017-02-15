using System;
using System.Diagnostics;
using NUnit.Framework;
using Wyam.Core.Execution;
using Wyam.Testing;
using Trace = Wyam.Core.Modules.Extensibility.Trace;
using Wyam.Common.Execution;

namespace Wyam.Core.Tests.Modules.Extensibility
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class TraceFixture : BaseFixture
    {
        public class ExecuteTests : TraceFixture
        {
            [TestCase(TraceEventType.Critical)]
            [TestCase(TraceEventType.Error)]
            [TestCase(TraceEventType.Warning)]
            public void TestTraceListenerThrows(TraceEventType traceEventType)
            {
                // Given
                Engine engine = new Engine();
                engine.Pipelines.Add(new Trace(traceEventType.ToString()).EventType(traceEventType));

                // When
                TestDelegate test = () => engine.Execute();

                // Then
                Assert.Throws<Exception>(test);
            }
        }
    }
}
