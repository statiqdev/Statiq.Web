using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Modules;
using Wyam.Common.Documents;
using Wyam.Common.Pipelines;
using Wyam.Core.Modules;

namespace Wyam.Core.Tests.Modules
{
    [TestFixture]
    public class ReplaceInFixture
    {
        [Test]
        public void TestReplaceSuccess()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());

            DummyTextModule input = new DummyTextModule("Hello World");
            DummyTextModule output = new DummyTextModule();

            engine.Pipelines.Add(input, new ReplaceIn("Hello", "Goodbye"), output);

            // When
            engine.Execute();

            // Then
            Assert.AreEqual("Goodbye World", output.Text);
        }

        [Test]
        public void TestNoMatchingTextSuccess()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());

            DummyTextModule input = new DummyTextModule("Hello World");
            DummyTextModule output = new DummyTextModule();

            engine.Pipelines.Add(input, new ReplaceIn("Huhu", "Goodbye"), output);

            // When
            engine.Execute();

            // Then
            Assert.AreEqual("Hello World", output.Text);
        }

        private class DummyTextModule : IModule
        {
            public string Text { get; private set; }

            public DummyTextModule(string text = null)
            {
                Text = text;
            }

            public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
            {
                if (Text != null)
                {
                    return inputs.Select(f => f.Clone(Text));
                }
                else
                {
                    Text = inputs.First().Content;
                    return inputs;
                }
            }
        }
    }
}
