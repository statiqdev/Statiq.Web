using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dotless.Core.Loggers;
using Wyam.Common;

namespace Wyam.Modules.Less
{
    internal class LessLogger : ILogger
    {
        public void Log(LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Info:
                    Info(message);
                    break;
                case LogLevel.Debug:
                    Debug(message);
                    break;
                case LogLevel.Warn:
                    Warn(message);
                    break;
                case LogLevel.Error:
                    Error(message);
                    break;
            }
        }

        public void Info(string message)
        {
            Trace.TraceInformation(message);
        }

        public void Info(string message, params object[] args)
        {
            Trace.TraceInformation(message, args);
        }

        public void Debug(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void Debug(string message, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(message, args);
        }

        public void Warn(string message)
        {
            Trace.TraceWarning(message);
        }

        public void Warn(string message, params object[] args)
        {
            Trace.TraceWarning(message, args);
        }

        public void Error(string message)
        {
            Trace.TraceError(message);
        }

        public void Error(string message, params object[] args)
        {
            Trace.TraceError(message, args);
        }
    }
}
