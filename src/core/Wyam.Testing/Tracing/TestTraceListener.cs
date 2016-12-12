using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

namespace Wyam.Testing.Tracing
{
    /// <summary>
    /// Throws exceptions on error or warning traces, but only for the current thread
    /// </summary>
    public class TestTraceListener : ConsoleTraceListener
    {
        public string TestId { get; private set; }

        public TraceEventType? ThrowTraceEventType { get; set; }

        public List<KeyValuePair<TraceEventType, string>> Messages { get; } = new List<KeyValuePair<TraceEventType, string>>();

        public TestTraceListener(string testId)
        {
            TestId = testId;
            ThrowTraceEventType = TraceEventType.Warning;
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            Messages.Add(new KeyValuePair<TraceEventType, string>(eventType, message));
            ThrowOnErrorOrWarning(eventType, message);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            Messages.Add(new KeyValuePair<TraceEventType, string>(eventType, string.Format(format, args)));
            ThrowOnErrorOrWarning(eventType, string.Format(format, args));
        }

        private void ThrowOnErrorOrWarning(TraceEventType eventType, string message)
        {
            if (TestContext.CurrentContext.Test.ID == TestId && ThrowTraceEventType.HasValue && eventType <= ThrowTraceEventType.Value)
            {
                throw new Exception(message);
            }
        }
    }
}
