using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;

namespace Wyam.Core.Tests
{
    // Throws exceptions on error or warning traces
    public class TestTraceListener : ConsoleTraceListener
    {
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
            if (eventType == TraceEventType.Critical
                || eventType == TraceEventType.Error
                || eventType == TraceEventType.Warning)
            {
                throw new Exception(message);
            }
        }
    }
}
