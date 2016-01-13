using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam
{
    // A special trace listener that shows colors in the console and doesn't output the source name or event id
    // This was helpful: http://blog.flimflan.com/ASimpleColorConsoleTraceListener.html
    internal class SimpleColorConsoleTraceListener : ConsoleTraceListener
    {
        private readonly Dictionary<TraceEventType, ConsoleColor> _eventColor = new Dictionary<TraceEventType, ConsoleColor>()
        {
            { TraceEventType.Verbose, ConsoleColor.DarkGray },
            { TraceEventType.Information, ConsoleColor.Gray },
            { TraceEventType.Warning, ConsoleColor.Yellow },
            { TraceEventType.Error, ConsoleColor.DarkRed },
            { TraceEventType.Critical, ConsoleColor.Red },
            { TraceEventType.Start, ConsoleColor.DarkCyan },
            { TraceEventType.Stop, ConsoleColor.DarkCyan }
        };
 
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            TraceEvent(eventCache, source, eventType, id, "{0}", message);
        }
 
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = GetEventColor(eventType, originalColor);
            base.WriteLine(string.Format(format, args));
            Console.ForegroundColor = originalColor;
        }
 
        private ConsoleColor GetEventColor(TraceEventType eventType, ConsoleColor defaultColor)
        {
            if (!_eventColor.ContainsKey(eventType))
            {
                return defaultColor;
            }
            return _eventColor[eventType];
        }
    }
}
