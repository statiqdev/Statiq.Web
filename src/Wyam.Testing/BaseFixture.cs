using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Trace = Wyam.Common.Tracing.Trace;

namespace Wyam.Testing
{
    public abstract class BaseFixture
    {
        [SetUp]
        public void BaseSetUp()
        {
            TestTraceListener listener = new TestTraceListener(TestContext.CurrentContext.Test.ID);
            Trace.AddListener(listener);
        }

        [TearDown]
        public void BaseTearDown()
        {
            RemoveListener();
        }

        public void RemoveListener()
        {
            TestTraceListener listener = Trace.Listeners.OfType<TestTraceListener>()
                .FirstOrDefault(x => x.TestId == TestContext.CurrentContext.Test.ID);
            if (listener != null)
            {
                Trace.RemoveListener(listener);
            }
        }

        public void ThrowOnTraceEventType(TraceEventType traceEventType)
        {
            TestTraceListener listener = Trace.Listeners.OfType<TestTraceListener>()
                .FirstOrDefault(x => x.TestId == TestContext.CurrentContext.Test.ID);
            if (listener != null)
            {
                listener.ThrowTraceEventType = traceEventType;
            }
        }
    }
}
