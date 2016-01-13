using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Tracing;

namespace Wyam.Core.Tracing
{
    public class Trace : ITrace, IDisposable
    {
        private readonly TraceSource _traceSource = new TraceSource("Wyam", SourceLevels.Information);
        private int _indent = 0;
        private readonly DiagnosticsTraceListener _diagnosticsTraceListener;
        private bool _disposed;

        public Trace()
        {
            _diagnosticsTraceListener = new DiagnosticsTraceListener(this);
            System.Diagnostics.Trace.Listeners.Add(_diagnosticsTraceListener);
        }

        public void SetLevel(SourceLevels level)
        {
            CheckDisposed();
            _traceSource.Switch.Level = level;
        }

        public void AddListener(TraceListener listener)
        {
            CheckDisposed();
            _traceSource.Listeners.Add(listener);
            listener.IndentLevel = _indent;
        }

        public void RemoveListener(TraceListener listener)
        {
            CheckDisposed();
            listener.IndentLevel = 0;
            _traceSource.Listeners.Remove(listener);
        }

        // Stops the application
        public void Critical(string messageOrFormat, params object[] args)
        {
            CheckDisposed();
            TraceEvent(TraceEventType.Critical, messageOrFormat, args);
        }

        // Prevents expected behavior
        public void Error(string messageOrFormat, params object[] args)
        {
            CheckDisposed();
            TraceEvent(TraceEventType.Error, messageOrFormat, args);
        }

        // Unexpected behavior that does not prevent expected behavior
        public void Warning(string messageOrFormat, params object[] args)
        {
            CheckDisposed();
            TraceEvent(TraceEventType.Warning, messageOrFormat, args);
        }

        public void Information(string messageOrFormat, params object[] args)
        {
            CheckDisposed();
            TraceEvent(TraceEventType.Information, messageOrFormat, args);
        }

        public void Verbose(string messageOrFormat, params object[] args)
        {
            CheckDisposed();
            TraceEvent(TraceEventType.Verbose, messageOrFormat, args);
        }

        public void TraceEvent(TraceEventType eventType, string messageOrFormat, params object[] args)
        {
            CheckDisposed();

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
            CheckDisposed();
            return IndentLevel++;
        }

        public int IndentLevel
        {
            get
            {
                CheckDisposed();
                return _indent;
            }
            set
            {
                CheckDisposed();
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(Trace));
                }

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

        public IIndentedTraceEvent WithIndent()
        {
            CheckDisposed();
            return new IndentedTraceEvent(this);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            System.Diagnostics.Trace.Listeners.Remove(_diagnosticsTraceListener);
            _disposed = true;
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Trace));
            }
        }
    }
}
