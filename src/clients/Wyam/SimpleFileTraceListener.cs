using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam
{
    // A special trace listener that doesn't output the source name or event id
    internal class SimpleFileTraceListener : TextWriterTraceListener
    {
        public SimpleFileTraceListener(string fileName) : base(fileName)
        {
            StreamWriter writer = Writer as StreamWriter;
            if (writer != null)
            {
                writer.AutoFlush = true;
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            TraceEvent(eventCache, source, eventType, id, "{0}", message);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            base.WriteLine(string.Format(format, args));
        }
    }
}
