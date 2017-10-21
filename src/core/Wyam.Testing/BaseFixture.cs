using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Testing.Tracing;
using Trace = Wyam.Common.Tracing.Trace;

namespace Wyam.Testing
{
    public abstract class BaseFixture
    {
        public TestTraceListener Listener { get; private set; }

        [SetUp]
        public void BaseSetUp()
        {
            Listener = new TestTraceListener(TestContext.CurrentContext.Test.ID);
            Trace.AddListener(Listener);
        }

        [TearDown]
        public void BaseTearDown()
        {
            RemoveListener();
        }

        public void RemoveListener()
        {
            TestTraceListener listener = Trace.GetListeners().OfType<TestTraceListener>()
                .FirstOrDefault(x => x.TestId == TestContext.CurrentContext.Test.ID);
            if (listener != null)
            {
                Trace.RemoveListener(listener);
            }
        }

        public void ThrowOnTraceEventType(TraceEventType? traceEventType)
        {
            TestTraceListener listener = Trace.GetListeners().OfType<TestTraceListener>()
                .FirstOrDefault(x => x.TestId == TestContext.CurrentContext.Test.ID);
            if (listener != null)
            {
                listener.ThrowTraceEventType = traceEventType;
            }
        }
    }
}
