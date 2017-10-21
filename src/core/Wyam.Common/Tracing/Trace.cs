using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Tracing;

namespace Wyam.Common.Tracing
{
    /// <summary>
    /// Provides access to tracing functionality. This class is thread safe.
    /// </summary>
    public class Trace : ITrace
    {
        private static readonly TraceSource TraceSource = new TraceSource("Wyam", SourceLevels.Information);
        private static int _indent = 0;
        private static object _lock = new object();

        public static ITrace Current { get; } = new Trace();

        private Trace()
        {
        }

        public static SourceLevels Level
        {
            get { return TraceSource.Switch.Level; }
            set { TraceSource.Switch.Level = value; }
        }

        public static void AddListener(TraceListener listener)
        {
            lock (_lock)
            {
                TraceSource.Listeners.Add(listener);
                listener.IndentLevel = _indent;
            }
        }

        public static void RemoveListener(TraceListener listener)
        {
            lock (_lock)
            {
                listener.IndentLevel = 0;
                TraceSource.Listeners.Remove(listener);
            }
        }

        public static IEnumerable<TraceListener> GetListeners()
        {
            lock (_lock)
            {
                return TraceSource.Listeners.OfType<TraceListener>().ToArray();
            }
        }

        // Stops the application
        public static void Critical(string messageOrFormat, params object[] args) =>
            TraceEvent(TraceEventType.Critical, messageOrFormat, args);

        // Prevents expected behavior
        public static void Error(string messageOrFormat, params object[] args) =>
            TraceEvent(TraceEventType.Error, messageOrFormat, args);

        // Unexpected behavior that does not prevent expected behavior
        public static void Warning(string messageOrFormat, params object[] args) =>
            TraceEvent(TraceEventType.Warning, messageOrFormat, args);

        public static void Information(string messageOrFormat, params object[] args) =>
            TraceEvent(TraceEventType.Information, messageOrFormat, args);

        public static void Verbose(string messageOrFormat, params object[] args) =>
            TraceEvent(TraceEventType.Verbose, messageOrFormat, args);

        public static void TraceEvent(TraceEventType eventType, string messageOrFormat, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                TraceSource.TraceEvent(eventType, 0, messageOrFormat);
            }
            else
            {
                TraceSource.TraceEvent(eventType, 0, messageOrFormat, args);
            }
        }

        public static int Indent() => IndentLevel++;

        public static int IndentLevel
        {
            get
            {
                return _indent;
            }
            set
            {
                if (value >= 0)
                {
                    Interlocked.Exchange(ref _indent, value);
                    foreach (TraceListener listener in TraceSource.Listeners)
                    {
                        listener.IndentLevel = value;
                    }
                }
            }
        }

        public static IIndentedTraceEvent WithIndent() => new IndentedTraceEvent();

        SourceLevels ITrace.Level
        {
            get { return Level; }
            set { Level = value; }
        }

        void ITrace.AddListener(TraceListener listener) => AddListener(listener);

        void ITrace.RemoveListener(TraceListener listener) => RemoveListener(listener);

        IEnumerable<TraceListener> ITrace.Listeners => GetListeners();

        void ITrace.Critical(string messageOrFormat, params object[] args) =>
            Critical(messageOrFormat, args);

        void ITrace.Error(string messageOrFormat, params object[] args) =>
            Error(messageOrFormat, args);

        void ITrace.Warning(string messageOrFormat, params object[] args) =>
            Warning(messageOrFormat, args);

        void ITrace.Information(string messageOrFormat, params object[] args) =>
            Information(messageOrFormat, args);

        void ITrace.Verbose(string messageOrFormat, params object[] args) =>
            Verbose(messageOrFormat, args);

        void ITrace.TraceEvent(TraceEventType eventType, string messageOrFormat, params object[] args) =>
            TraceEvent(eventType, messageOrFormat, args);

        int ITrace.Indent() => Indent();

        int ITrace.IndentLevel
        {
            get { return IndentLevel; }
            set { IndentLevel = value; }
        }

        IIndentedTraceEvent ITrace.WithIndent() => WithIndent();
    }
}
