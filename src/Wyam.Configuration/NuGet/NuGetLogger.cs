using System;
using NuGet.Logging;
using Wyam.Common.Tracing;

namespace Wyam.Configuration.NuGet
{
    internal class NuGetLogger : ILogger
    {
        public void LogDebug(string data) => Trace.Verbose(data);

        public void LogVerbose(string data) => Trace.Verbose(data);

        public void LogInformation(string data) => Trace.Verbose(data);

        public void LogMinimal(string data) => Trace.Verbose(data);

        public void LogWarning(string data) => Trace.Warning(data);

        public void LogError(string data) => Trace.Error(data);

        public void LogSummary(string data) => Trace.Verbose(data);
    }
}