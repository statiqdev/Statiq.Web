using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core
{
    public class Trace
    {
        private readonly TraceSource _traceSource = new TraceSource("Wyam", SourceLevels.Information);

        public void SetVerbose(bool verbose)
        {
            _traceSource.Switch.Level = verbose ? SourceLevels.Verbose : SourceLevels.Information;
        }

        public void AddListener(TraceListener listener)
        {
            _traceSource.Listeners.Add(listener);
        }

        public void RemoveListener(TraceListener listener)
        {
            _traceSource.Listeners.Remove(listener);
        }

        // Stops the application
        public void Critical(string format, params object[] args)
        {
            _traceSource.TraceEvent(TraceEventType.Critical, 0, format, args);
        }

        // Prevents expected behavior
        public void Error(string format, params object[] args)
        {
            _traceSource.TraceEvent(TraceEventType.Error, 0, format, args);
        }

        // Unexpected behavior that does not prevent expected behavior
        public void Warning(string format, params object[] args)
        {
            _traceSource.TraceEvent(TraceEventType.Warning, 0, format, args);
        }

        public void Information(string format, params object[] args)
        {
            _traceSource.TraceEvent(TraceEventType.Information, 0, format, args);
        }

        public void Verbose(string format, params object[] args)
        {
            _traceSource.TraceEvent(TraceEventType.Verbose, 0, format, args);
        }
    }
}
