using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;

namespace Wyam.Core.Tracing
{
    internal class IndentedTraceEvent : IIndentedTraceEvent, IDisposable
    {
        private readonly ITrace _trace;
        private int? _indentLevel = null;
        private bool _disposed = false;

        public IndentedTraceEvent(ITrace trace)
        {
            _trace = trace;
        }

        public IDisposable Critical(string messageOrFormat, params object[] args)
        {
            CheckDisposed();
            _trace.Critical(messageOrFormat, args);
            _indentLevel = _trace.Indent();
            return this;
        }

        public IDisposable Error(string messageOrFormat, params object[] args)
        {
            CheckDisposed();
            _trace.Error(messageOrFormat, args);
            _indentLevel = _trace.Indent();
            return this;
        }

        public IDisposable Warning(string messageOrFormat, params object[] args)
        {
            CheckDisposed();
            _trace.Warning(messageOrFormat, args);
            _indentLevel = _trace.Indent();
            return this;
        }

        public IDisposable Information(string messageOrFormat, params object[] args)
        {
            CheckDisposed();
            _trace.Information(messageOrFormat, args);
            _indentLevel = _trace.Indent();
            return this;
        }

        public IDisposable Verbose(string messageOrFormat, params object[] args)
        {
            CheckDisposed();
            _trace.Verbose(messageOrFormat, args);
            _indentLevel = _trace.Indent();
            return this;
        }

        public IDisposable TraceEvent(TraceEventType eventType, string messageOrFormat, params object[] args)
        {
            CheckDisposed();
            _trace.TraceEvent(eventType, messageOrFormat, args);
            _indentLevel = _trace.Indent();
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
                _trace.IndentLevel = _indentLevel.Value;
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
