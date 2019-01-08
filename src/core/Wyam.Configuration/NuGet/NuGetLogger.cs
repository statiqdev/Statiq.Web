using System;
using System.Threading.Tasks;
using NuGet.Common;
using Wyam.Common.Tracing;

namespace Wyam.Configuration.NuGet
{
    internal class NuGetLogger : ILogger
    {
        public void LogDebug(string data) => Trace.Verbose(data);

        public void LogVerbose(string data) => Trace.Verbose(data);

        public void LogInformation(string data) => Trace.Verbose(data);

        public void LogInformationSummary(string data) => Trace.Verbose(data);

        public void LogMinimal(string data) => Trace.Verbose(data);

        public void LogWarning(string data) => Trace.Warning(data);

        public void LogError(string data) => Trace.Error(data);

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task LogAsync(LogLevel level, string data) => Log(level, data);
        public async Task LogAsync(ILogMessage message) => Log(message.Level, message.Message);
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        public void Log(ILogMessage message) => Log(message.Level, message.Message);

        public void Log(LogLevel level, string data)
        {
            if (level == LogLevel.Error)
            {
                Trace.Error(data);
                return;
            }
            if (level == LogLevel.Warning)
            {
                Trace.Warning(data);
                return;
            }
            Trace.Verbose(data);
        }
    }
}