using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Abstractions;

namespace Wyam.Core
{
    // This routes trace and debug messages from the Trace/Debug classes to the Wyam Trace TraceSource
    internal class DiagnosticsTraceListener : TraceListener
    {
        private readonly Trace _trace;

        public DiagnosticsTraceListener(Trace trace)
        {
            _trace = trace;
        }
        
        public override void Write(string message)
        {
            _trace.Verbose(message);
        }

        public override void WriteLine(string message)
        {
            _trace.Verbose(message);
        }

        public override void Fail(string message)
        {
            _trace.Error(message);
        }

        public override void Fail(string message, string detailMessage)
        {
            _trace.Error(message + " " + detailMessage);
        }
        
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            this.TraceData(eventCache, source, eventType, id, new object[] { data });
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < data.Length; ++i)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append("{");
                sb.Append(i);
                sb.Append("}");
            }

            _trace.Verbose(sb.ToString());
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            _trace.TraceEvent(eventType, id.ToString());
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            _trace.TraceEvent(eventType, format, args);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            _trace.TraceEvent(eventType, message);
        }

        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            _trace.Verbose(message);
        }
    }
}
