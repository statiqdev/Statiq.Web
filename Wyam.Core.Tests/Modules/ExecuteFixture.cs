using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Core.Modules;

namespace Wyam.Core.Tests.Modules
{
    [TestFixture]
    public class ExecuteFixture
    {
        [Test]
        public void ExecuteDoesNotThrowForNullResultWithDocumentConfig()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Execute execute = new Execute((d, c) => null);
            engine.Pipelines.Add(execute);

            // When
            engine.Execute();

            // Then
        }

        [Test]
        public void ExecuteDoesNotThrowForNullResultWithContextConfig()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Execute execute = new Execute((c) => null);
            engine.Pipelines.Add(execute);

            // When
            engine.Execute();

            // Then
        }

    }
}
