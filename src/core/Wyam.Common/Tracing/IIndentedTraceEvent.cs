using System;
using System.Diagnostics;

namespace Wyam.Common.Tracing
{
    /// <summary>
    /// Traces messages and indents all future messages until the returned <see cref="IDisposable"/> is disposed.
    /// </summary>
    public interface IIndentedTraceEvent
    {
        /// <summary>
        /// Traces a critical message.
        /// </summary>
        /// <param name="messageOrFormat">The formatted message to write.</param>
        /// <param name="args">The arguments for the formatted message.</param>
        /// <returns>An <see cref="IDisposable"/> that should be disposed when trace messages should no longer be indented.</returns>
        IDisposable Critical(string messageOrFormat, params object[] args);

        /// <summary>
        /// Traces an error message.
        /// </summary>
        /// <param name="messageOrFormat">The formatted message to write.</param>
        /// <param name="args">The arguments for the formatted message.</param>
        /// <returns>An <see cref="IDisposable"/> that should be disposed when trace messages should no longer be indented.</returns>
        IDisposable Error(string messageOrFormat, params object[] args);

        /// <summary>
        /// Traces a warning message.
        /// </summary>
        /// <param name="messageOrFormat">The formatted message to write.</param>
        /// <param name="args">The arguments for the formatted message.</param>
        /// <returns>An <see cref="IDisposable"/> that should be disposed when trace messages should no longer be indented.</returns>
        IDisposable Warning(string messageOrFormat, params object[] args);

        /// <summary>
        /// Traces an informational message.
        /// </summary>
        /// <param name="messageOrFormat">The formatted message to write.</param>
        /// <param name="args">The arguments for the formatted message.</param>
        /// <returns>An <see cref="IDisposable"/> that should be disposed when trace messages should no longer be indented.</returns>
        IDisposable Information(string messageOrFormat, params object[] args);

        /// <summary>
        /// Traces a verbose message.
        /// </summary>
        /// <param name="messageOrFormat">The formatted message to write.</param>
        /// <param name="args">The arguments for the formatted message.</param>
        /// <returns>An <see cref="IDisposable"/> that should be disposed when trace messages should no longer be indented.</returns>
        IDisposable Verbose(string messageOrFormat, params object[] args);

        /// <summary>
        /// Traces a raw event.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        /// <param name="messageOrFormat">The formatted message to write.</param>
        /// <param name="args">The arguments for the formatted message.</param>
        /// <returns>An <see cref="IDisposable"/> that should be disposed when trace messages should no longer be indented.</returns>
        IDisposable TraceEvent(TraceEventType eventType, string messageOrFormat, params object[] args);
    }
}