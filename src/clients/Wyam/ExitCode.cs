using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam
{
    internal enum ExitCode
    {
        Normal = 0,
        UnhandledError = 1,
        CommandLineError = 2,
        ConfigurationError = 3,
        ExecutionError = 4,
        UnsupportedRuntime = 5
    }
}
