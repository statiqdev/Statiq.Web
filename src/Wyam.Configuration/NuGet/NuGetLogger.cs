using System;
using NuGet;
using Wyam.Common.Tracing;

namespace Wyam.Configuration.NuGet
{
    internal class NuGetLogger : ILogger
    {
        public FileConflictResolution ResolveFileConflict(string message)
        {
            Trace.Verbose(message);
            return FileConflictResolution.OverwriteAll;
        }

        public void Log(MessageLevel level, string message, params object[] args)
        {
            switch (level)
            {
                case MessageLevel.Info:
                    Trace.Verbose(message, args);
                    break;
                case MessageLevel.Warning:
                    Trace.Warning(message, args);
                    break;
                case MessageLevel.Debug:
                    Trace.Verbose(message, args);
                    break;
                case MessageLevel.Error:
                    Trace.Error(message, args);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
    }
}