using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Tracing;

namespace Wyam.Common.Tracing
{
    internal class IndentedTraceEvent : IIndentedTraceEvent, IDisposable
    {
        private int? _indentLevel = null;
        private bool _disposed = false;

        public IDisposable Critical(string messageOrFormat, params object[] args)
        {
            CheckDisposed();
            Trace.Critical(messageOrFormat, args);
            _indentLevel = Trace.Indent();
            return this;
        }

        public IDisposable Error(string messageOrFormat, params object[] args)
        {
            CheckDisposed();
            Trace.Error(messageOrFormat, args);
            _indentLevel = Trace.Indent();
            return this;
        }

        public IDisposable Warning(string messageOrFormat, params object[] args)
        {
            CheckDisposed();
            Trace.Warning(messageOrFormat, args);
            _indentLevel = Trace.Indent();
            return this;
        }

        public IDisposable Information(string messageOrFormat, params object[] args)
        {
            CheckDisposed();
            Trace.Information(messageOrFormat, args);
            _indentLevel = Trace.Indent();
            return this;
        }

        public IDisposable Verbose(string messageOrFormat, params object[] args)
        {
            CheckDisposed();
            Trace.Verbose(messageOrFormat, args);
            _indentLevel = Trace.Indent();
            return this;
        }

        public IDisposable TraceEvent(TraceEventType eventType, string messageOrFormat, params object[] args)
        {
            CheckDisposed();
            Trace.TraceEvent(eventType, messageOrFormat, args);
            _indentLevel = Trace.Indent();
            return this;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_indentLevel.HasValue)
            {
                Trace.IndentLevel = _indentLevel.Value;
            }
            _disposed = true;
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(IndentedTraceEvent));
            }
        }
    }
}
