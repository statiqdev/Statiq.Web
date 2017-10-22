using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private readonly ConcurrentDictionary<string, TestTraceListener> _listeners =
            new ConcurrentDictionary<string, TestTraceListener>();

        public TestTraceListener Listener => 
            _listeners.TryGetValue(TestContext.CurrentContext.Test.ID, out var listener) ? listener : null;

        [SetUp]
        public void BaseSetUp()
        {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
            TestTraceListener listener = new TestTraceListener(TestContext.CurrentContext.Test.ID);
            _listeners.AddOrUpdate(TestContext.CurrentContext.Test.ID, listener, (x, y) => listener);
            Trace.AddListener(Listener);
        }

        [TearDown]
        public void BaseTearDown()
        {
            RemoveListener();
        }

        public void RemoveListener()
        {
            TestTraceListener listener = Listener;
            if (listener != null)
            {
                Trace.RemoveListener(listener);
            }
        }

        public void ThrowOnTraceEventType(TraceEventType? traceEventType)
        {
            TestTraceListener listener = Listener;
            if (listener != null)
            {
                listener.ThrowTraceEventType = traceEventType;
            }
        }
    }
}
