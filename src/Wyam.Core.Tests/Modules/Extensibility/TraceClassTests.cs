using System;
using System.Diagnostics;
using NUnit.Framework;
using Wyam.Testing;
using Trace = Wyam.Core.Modules.Extensibility.Trace;

namespace Wyam.Core.Tests.Modules.Extensibility
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class TraceClassTests : BaseFixture
    {
        public class ExecuteMethodTests : TraceClassTests
        {
            [TestCase(TraceEventType.Critical)]
            [TestCase(TraceEventType.Error)]
            [TestCase(TraceEventType.Warning)]
            public void TestTraceListenerThrows(TraceEventType traceEventType)
            {
                // Given
                Engine engine = new Engine();
                engine.CleanOutputFolderOnExecute = false;
                engine.Pipelines.Add(new Trace(traceEventType.ToString()).EventType(traceEventType));

                // When
                TestDelegate test = () => engine.Execute();

                // Then
                Assert.Throws<Exception>(test);
            }
        }
    }
}
