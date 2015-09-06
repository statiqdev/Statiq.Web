using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Core.Documents;
using Wyam.Core.Modules;

namespace Wyam.Core.Tests.Modules
{
    [TestFixture]
    public class OrderByFixture
    {
        [Test]
        public void OrderByOrdersInAscendingOrder()
        {
            // Given
            List<string> content = new List<string>();
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            CountModule count = new CountModule("A")
            {
                AdditionalOutputs = 4
            };
            CountModule count2 = new CountModule("A")
            {
                AdditionalOutputs = 2
            };
            Concat concat = new Concat(count2);
            OrderBy orderBy = new OrderBy((d, c) => d.Get<int>("A"));
            Execute gatherData = new Execute((d, c) =>
            {
                content.Add(d.Content);
                return null;
            });
            engine.Pipelines.Add(count, concat, orderBy, gatherData);

            // When
            engine.Execute();

            // Then
            Assert.AreEqual(8, content.Count);
            CollectionAssert.AreEqual(new[] { "1", "1", "2", "2", "3", "3", "4", "5" }, content);
        }

        [Test]
        public void OrderByOrdersInDescendingOrder()
        {
            // Given
            List<string> content = new List<string>();
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            CountModule count = new CountModule("A")
            {
                AdditionalOutputs = 4
            };
            CountModule count2 = new CountModule("A")
            {
                AdditionalOutputs = 2
            };
            Concat concat = new Concat(count2);
            OrderBy orderBy = new OrderBy((d, c) => d.Get<int>("A")).Descending();
            Execute gatherData = new Execute((d, c) =>
            {
                content.Add(d.Content);
                return null;
            });
            engine.Pipelines.Add(count, concat, orderBy, gatherData);

            // When
            engine.Execute();

            // Then
            Assert.AreEqual(8, content.Count);
            CollectionAssert.AreEqual(new[] { "5", "4", "3", "3", "2", "2", "1", "1" }, content);
        }
    }
}
