using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Wyam.Hosting
{
    public class LogActionLoggerProvider : ILoggerProvider
    {
        private readonly Action<string> _logAction;

        public LogActionLoggerProvider(Action<string> logAction)
        {
            _logAction = logAction;
        }

        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName) => new Logger(_logAction);

        private class Logger : ILogger
        {
            private readonly Action<string> _logAction;

            public Logger(Action<string> logAction)
            {
                _logAction = logAction;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) => 
                _logAction(formatter(state, exception));

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state) => null;
        }
    }
}
