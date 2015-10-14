using System;
using System.Diagnostics;

namespace Wyam.Common.Tracing
{
    public interface IIndentedTraceEvent
    {
        IDisposable Critical(string messageOrFormat, params object[] args);
        IDisposable Error(string messageOrFormat, params object[] args);
        IDisposable Warning(string messageOrFormat, params object[] args);
        IDisposable Information(string messageOrFormat, params object[] args);
        IDisposable Verbose(string messageOrFormat, params object[] args);
        IDisposable TraceEvent(TraceEventType eventType, string messageOrFormat, params object[] args);
    }
}