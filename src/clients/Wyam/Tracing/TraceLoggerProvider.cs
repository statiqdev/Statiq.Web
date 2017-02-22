using Microsoft.Extensions.Logging;

namespace Wyam.Tracing
{
    internal class TraceLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new TraceLogger(categoryName);

        public void Dispose()
        {
        }
    }
}
