using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Testing.Tracing;
using Trace = Wyam.Common.Tracing.Trace;

namespace Wyam.Testing.Attributes
{
    public class NonWindowsTestCaseAttribute : TestCaseAttribute
    {
        public NonWindowsTestCaseAttribute(params object[] arguments)
            : base(arguments)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Ignore = "Non-Windows only";
            }
        }

        public NonWindowsTestCaseAttribute(object arg)
            : base(arg)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Ignore = "Non-Windows only";
            }
        }

        public NonWindowsTestCaseAttribute(object arg1, object arg2)
            : base(arg1, arg2)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Ignore = "Non-Windows only";
            }
        }

        public NonWindowsTestCaseAttribute(object arg1, object arg2, object arg3)
            : base(arg1, arg2, arg3)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Ignore = "Non-Windows only";
            }
        }
    }
}
