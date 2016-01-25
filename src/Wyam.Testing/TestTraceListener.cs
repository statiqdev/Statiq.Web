using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Wyam.Testing
{
    // Throws exceptions on error or warning traces, but only for the current thread
    internal class TestTraceListener : ConsoleTraceListener
    {
        public string TestId { get; private set; }

        public TraceEventType ThrowTraceEventType { get; set; }

        public TestTraceListener(string testId)
        {
            TestId = testId;
            ThrowTraceEventType = TraceEventType.Warning;
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            ThrowOnErrorOrWarning(eventType, message);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            ThrowOnErrorOrWarning(eventType, string.Format(format, args));
        }

        private void ThrowOnErrorOrWarning(TraceEventType eventType, string message)
        {
            if (TestContext.CurrentContext.Test.ID == TestId && eventType <= ThrowTraceEventType)
            {
                throw new Exception(message);
            }
        }
    }
}
