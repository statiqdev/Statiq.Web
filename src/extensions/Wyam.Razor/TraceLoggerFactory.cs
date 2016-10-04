using Microsoft.Extensions.Logging;

namespace Wyam.Razor
{
    internal class TraceLoggerFactory : ILoggerFactory
    {
        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName) => new TraceLogger(categoryName);

        public void AddProvider(ILoggerProvider provider)
        {
        }
    }
}