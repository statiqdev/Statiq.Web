using System.Collections.Generic;
using System.Diagnostics;

namespace Wyam.Common.Tracing
{
    /// <summary>
    /// An interface for tracing messages to the console and other attached outputs.
    /// </summary>
    public interface ITrace
    {
        /// <summary>
        /// Specifies the levels of trace messages.
        /// </summary>
        SourceLevels Level { get; set; }

        /// <summary>
        /// Adds a trace listener.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        void AddListener(TraceListener listener);

        /// <summary>
        /// Removes trace listener.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        void RemoveListener(TraceListener listener);

        /// <summary>
        /// A collection of attached trace listeners which will receive tracing events.
        /// </summary>
        IEnumerable<TraceListener> Listeners { get; }

        /// <summary>
        /// Traces a critical message.
        /// </summary>
        /// <param name="messageOrFormat">The formatted message to write.</param>
        /// <param name="args">The arguments for the formatted message.</param>
        void Critical(string messageOrFormat, params object[] args);

        /// <summary>
        /// Traces a critical message.
        /// </summary>
        /// <param name="messageOrFormat">The formatted message to write.</param>
        /// <param name="args">The arguments for the formatted message.</param>
        void Error(string messageOrFormat, params object[] args);

        /// <summary>
        /// Traces a warning message.
        /// </summary>
        /// <param name="messageOrFormat">The formatted message to write.</param>
        /// <param name="args">The arguments for the formatted message.</param>
        void Warning(string messageOrFormat, params object[] args);

        /// <summary>
        /// Traces an informational message.
        /// </summary>
        /// <param name="messageOrFormat">The formatted message to write.</param>
        /// <param name="args">The arguments for the formatted message.</param>
        void Information(string messageOrFormat, params object[] args);

        /// <summary>
        /// Traces a verbose message.
        /// </summary>
        /// <param name="messageOrFormat">The formatted message to write.</param>
        /// <param name="args">The arguments for the formatted message.</param>
        void Verbose(string messageOrFormat, params object[] args);

        /// <summary>
        /// Traces a raw event.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        /// <param name="messageOrFormat">The formatted message to write.</param>
        /// <param name="args">The arguments for the formatted message.</param>
        void TraceEvent(TraceEventType eventType, string messageOrFormat, params object[] args);

        /// <summary>
        /// Indents all future trace messages.
        /// </summary>
        /// <returns>The new indent level.</returns>
        int Indent();

        /// <summary>
        /// The current indent level.
        /// </summary>
        int IndentLevel { get; set; }

        /// <summary>
        /// Returns a <see cref="IIndentedTraceEvent"/> that can be used to trace a message and indent following messages.
        /// </summary>
        /// <returns>A <see cref="IIndentedTraceEvent"/> that should be used to trace an indented message and indent following messages.</returns>
        IIndentedTraceEvent WithIndent();
    }
}