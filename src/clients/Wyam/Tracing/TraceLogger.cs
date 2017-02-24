using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Wyam.Common.Util;
using Trace = Wyam.Common.Tracing.Trace;

namespace Wyam.Tracing
{
    internal class TraceLogger : ILogger
    {
        private readonly string _categoryName;

        private static readonly Dictionary<LogLevel, SourceLevels> LevelMapping = new Dictionary<LogLevel, SourceLevels>
        {
            {LogLevel.Trace, SourceLevels.Verbose},
            {LogLevel.Debug, SourceLevels.Verbose},
            {LogLevel.Information, SourceLevels.Verbose},
            {LogLevel.Warning, SourceLevels.Warning},
            {LogLevel.Error, SourceLevels.Error},
            {LogLevel.Critical, SourceLevels.Critical},
            {LogLevel.None, SourceLevels.Off}
        };

        private static readonly Dictionary<LogLevel, TraceEventType> TraceMapping = new Dictionary<LogLevel, TraceEventType>
        {
            {LogLevel.Trace, TraceEventType.Verbose},
            {LogLevel.Debug, TraceEventType.Verbose},
            {LogLevel.Information, TraceEventType.Verbose},
            {LogLevel.Warning, TraceEventType.Warning},
            {LogLevel.Error, TraceEventType.Error},
            {LogLevel.Critical, TraceEventType.Critical},
            {LogLevel.None, TraceEventType.Verbose}
        };

        public TraceLogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                Common.Tracing.Trace.TraceEvent(TraceMapping[logLevel], $"{_categoryName}: {formatter(state, exception)}");
            }
        }

        public bool IsEnabled(LogLevel logLevel) => LevelMapping[logLevel].HasFlag(Trace.Level);

        public IDisposable BeginScope<TState>(TState state) => EmptyDisposable.Instance;
    }
}
