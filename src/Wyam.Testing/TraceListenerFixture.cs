using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Tracing;

namespace Wyam.Testing
{
    [TestFixture]
    public abstract class TraceListenerFixture
    {
        [SetUp]
        public void SetUp()
        {
            TestTraceListener listener = new TestTraceListener(TestContext.CurrentContext.Test.ID);
            Trace.AddListener(listener);
        }

        [TearDown]
        public void TearDown()
        {
            TestTraceListener listener = Trace.Listeners.OfType<TestTraceListener>()
                .First(x => x.TestId == TestContext.CurrentContext.Test.ID);
            Trace.RemoveListener(listener);
        }
    }
}
