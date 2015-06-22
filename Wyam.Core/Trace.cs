using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Abstractions;

namespace Wyam.Core
{
    public class Trace : ITrace
    {
        private readonly TraceSource _traceSource = new TraceSource("Wyam", SourceLevels.Information);
        private int _indent = 0;

        public Trace()
        {
            System.Diagnostics.Trace.Listeners.Clear();
            System.Diagnostics.Trace.Listeners.Add(new DiagnosticsTraceListener(this));
        }

        public void SetLevel(SourceLevels level)
        {
            
            _traceSource.Switch.Level = level;
        }

        public void AddListener(TraceListener listener)
        {
            _traceSource.Listeners.Add(listener);
            listener.IndentLevel = _indent;
        }

        public void RemoveListener(TraceListener listener)
        {
            listener.IndentLevel = 0;
            _traceSource.Listeners.Remove(listener);
        }

        // Stops the application
        public void Critical(string messageOrFormat, params object[] args)
        {
            TraceEvent(TraceEventType.Critical, messageOrFormat, args);
        }

        // Prevents expected behavior
        public void Error(string messageOrFormat, params object[] args)
        {
            TraceEvent(TraceEventType.Error, messageOrFormat, args);
        }

        // Unexpected behavior that does not prevent expected behavior
        public void Warning(string messageOrFormat, params object[] args)
        {
            TraceEvent(TraceEventType.Warning, messageOrFormat, args);
        }

        public void Information(string messageOrFormat, params object[] args)
        {
            TraceEvent(TraceEventType.Information, messageOrFormat, args);
        }

        public void Verbose(string messageOrFormat, params object[] args)
        {
            TraceEvent(TraceEventType.Verbose, messageOrFormat, args);
        }

        public void TraceEvent(TraceEventType eventType, string messageOrFormat, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                _traceSource.TraceEvent(eventType, 0, messageOrFormat);
            }
            else
            {
                _traceSource.TraceEvent(eventType, 0, messageOrFormat, args);
            }
        }

        public int Indent()
        {
            return IndentLevel++;
        }

        public int IndentLevel
        {
            get { return _indent; }
            set
            {
                if (value >= 0)
                {
                    _indent = value;
                    foreach (TraceListener listener in _traceSource.Listeners)
                    {
                        listener.IndentLevel = _indent;
                    }
                }
            }
        }
    }
}
