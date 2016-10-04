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
        private readonly Dictionary<TraceEventType, Tuple<ConsoleColor, ConsoleColor?>> _eventColors
            = new Dictionary<TraceEventType, Tuple<ConsoleColor, ConsoleColor?>>
            {
                { TraceEventType.Verbose, Tuple.Create(ConsoleColor.DarkGray, (ConsoleColor?)null) },
                { TraceEventType.Information, Tuple.Create(ConsoleColor.Gray, (ConsoleColor?)null) },
                { TraceEventType.Warning, Tuple.Create(ConsoleColor.Yellow, (ConsoleColor?)null) },
                { TraceEventType.Error, Tuple.Create(ConsoleColor.Red, (ConsoleColor?)null) },
                { TraceEventType.Critical, Tuple.Create(ConsoleColor.White, (ConsoleColor?)ConsoleColor.Red) },
                { TraceEventType.Start, Tuple.Create(ConsoleColor.DarkCyan, (ConsoleColor?)null) },
                { TraceEventType.Stop, Tuple.Create(ConsoleColor.DarkCyan, (ConsoleColor?)null) }
            };
 
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            TraceEvent(eventCache, source, eventType, id, "{0}", message);
        }
 
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            Tuple<ConsoleColor, ConsoleColor?> colors;
            if (!_eventColors.TryGetValue(eventType, out colors))
            {
                base.WriteLine(string.Format(format, args));
                return;
            }

            ConsoleColor originalForegroundColor = Console.ForegroundColor;
            ConsoleColor originalBackgroundColor = Console.BackgroundColor;
            Console.ForegroundColor = colors.Item1;
            if (colors.Item2.HasValue)
            {
                Console.BackgroundColor = colors.Item2.Value;
            }
            base.WriteLine(string.Format(format, args));
            Console.ForegroundColor = originalForegroundColor;
            if (colors.Item2.HasValue)
            {
                Console.BackgroundColor = originalBackgroundColor;
            }
        }
    }
}
