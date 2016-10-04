using System.Collections.Generic;
using System.Diagnostics;

namespace Wyam.Common.Tracing
{
    public interface ITrace
    {
        SourceLevels Level { get; set; }

        void AddListener(TraceListener listener);
        void RemoveListener(TraceListener listener);
        IEnumerable<TraceListener> Listeners { get; }

        void Critical(string messageOrFormat, params object[] args);
        void Error(string messageOrFormat, params object[] args);
        void Warning(string messageOrFormat, params object[] args);
        void Information(string messageOrFormat, params object[] args);
        void Verbose(string messageOrFormat, params object[] args);
        void TraceEvent(TraceEventType eventType, string messageOrFormat, params object[] args);

        int Indent();
        int IndentLevel { get; set; }
        IIndentedTraceEvent WithIndent();
    }
}