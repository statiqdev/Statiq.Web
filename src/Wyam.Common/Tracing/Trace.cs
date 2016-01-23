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
    public class Trace
    {
        public static Trace Current { get; } = new Trace();

        private static readonly TraceSource TraceSource = new TraceSource("Wyam", SourceLevels.Information);
        private static int _indent = 0;

        public static void SetLevel(SourceLevels level) => TraceSource.Switch.Level = level;

        public static void AddListener(TraceListener listener)
        {
            TraceSource.Listeners.Add(listener);
            listener.IndentLevel = _indent;
        }

        public static void RemoveListener(TraceListener listener)
        {
            listener.IndentLevel = 0;
            TraceSource.Listeners.Remove(listener);
        }

        public static IEnumerable<TraceListener> GetListeners() => 
            TraceSource.Listeners.OfType<TraceListener>();

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

        public static int Indent()
        {
            int indenet = _indent;
            Interlocked.Increment(ref _indent);
            return indenet;
        }

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
    }
}
