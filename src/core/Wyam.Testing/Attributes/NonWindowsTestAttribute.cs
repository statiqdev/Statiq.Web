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
using NUnit.Framework.Interfaces;
using Wyam.Testing.Tracing;
using Trace = Wyam.Common.Tracing.Trace;

namespace Wyam.Testing.Attributes
{
    public class NonWindowsTestAttribute : Attribute, ITestAction
    {
        public ActionTargets Targets { get; }

        public void BeforeTest(ITest test)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Ignore("Non-Windows only");
            }
        }

        public void AfterTest(ITest test)
        {
        }
    }
}
