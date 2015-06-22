using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dotless.Core.Loggers;
using Wyam.Abstractions;

namespace Wyam.Modules.Less
{
    internal class LessLogger : Logger
    {
        public LessLogger(LogLevel level) : base(level)
        {
        }

        protected override void Log(string message)
        {
            Trace.WriteLine(message);
        }
    }
}
