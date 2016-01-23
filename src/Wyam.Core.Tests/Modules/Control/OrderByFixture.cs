using System.Collections.Generic;
using NUnit.Framework;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Testing;

namespace Wyam.Core.Tests.Modules.Control
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class OrderByFixture : TraceListenerFixture
    {
        [Test]
        public void OrderByOrdersInAscendingOrder()
        {
            // Given
            List<string> content = new List<string>();
            Engine engine = new Engine();
            engine.CleanOutputFolderOnExecute = false;
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
            engine.CleanOutputFolderOnExecute = false;
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


        [Test]
        public void OrderByOrdersThenByInAscendingOrder()
        {
            // Given
            List<string> content = new List<string>();
            Engine engine = new Engine();
            engine.CleanOutputFolderOnExecute = false;
            CountModule count = new CountModule("A")
            {
                AdditionalOutputs = 4
            };
            CountModule count2 = new CountModule("B")
            {
                AdditionalOutputs = 1
            };
            OrderBy orderBy = new OrderBy((d, c) => d.Get<int>("A"))
                .ThenBy((d, c) => d.Get<int>("B"));
            Execute gatherData = new Execute((d, c) =>
            {
                content.Add(d.Content);
                return null;
            });
            engine.Pipelines.Add(count, count2, orderBy, gatherData);

            // When
            engine.Execute();

            // Then
            Assert.AreEqual(10, content.Count); // (4+1) * (21+1)
            CollectionAssert.AreEqual(new[] { "11", "12", "23", "24", "35", "36", "47", "48", "59", "510" }, content);
        }

        [Test]
        public void OrderByOrdersThenByInDescendingOrder()
        {
            // Given
            List<string> content = new List<string>();
            Engine engine = new Engine();
            engine.CleanOutputFolderOnExecute = false;
            CountModule count = new CountModule("A")
            {
                AdditionalOutputs = 4
            };
            CountModule count2 = new CountModule("B")
            {
                AdditionalOutputs = 1
            };
            OrderBy orderBy = new OrderBy((d, c) => d.Get<int>("A"))
                .ThenBy((d, c) => d.Get<int>("B"))
                .Descending();
            Execute gatherData = new Execute((d, c) =>
            {
                content.Add(d.Content);
                return null;
            });
            engine.Pipelines.Add(count, count2, orderBy, gatherData);

            // When
            engine.Execute();

            // Then
            Assert.AreEqual(10, content.Count); // (4+1) * (21+1)
            CollectionAssert.AreEqual(new[] { "12", "11", "24", "23", "36", "35", "48", "47", "510", "59" }, content);
        }

        [Test]
        public void OrderByOrdersDescendingThenByInDescendingOrder()
        {
            // Given
            List<string> content = new List<string>();
            Engine engine = new Engine();
            engine.CleanOutputFolderOnExecute = false;
            CountModule count = new CountModule("A")
            {
                AdditionalOutputs = 4
            };
            CountModule count2 = new CountModule("B")
            {
                AdditionalOutputs = 1
            };
            OrderBy orderBy = new OrderBy((d, c) => d.Get<int>("A"))
                .Descending()
                .ThenBy((d, c) => d.Get<int>("B"))
                .Descending();
            Execute gatherData = new Execute((d, c) =>
            {
                content.Add(d.Content);
                return null;
            });
            engine.Pipelines.Add(count, count2, orderBy, gatherData);

            // When
            engine.Execute();

            // Then
            Assert.AreEqual(10, content.Count); // (4+1) * (21+1)
            CollectionAssert.AreEqual(new[] { "510", "59", "48", "47", "36", "35", "24", "23", "12", "11" }, content);
        }

        [Test]
        public void OrderByOrdersDescendingThenByInAscendingOrder()
        {
            // Given
            List<string> content = new List<string>();
            Engine engine = new Engine();
            engine.CleanOutputFolderOnExecute = false;
            CountModule count = new CountModule("A")
            {
                AdditionalOutputs = 4
            };
            CountModule count2 = new CountModule("B")
            {
                AdditionalOutputs = 1
            };
            OrderBy orderBy = new OrderBy((d, c) => d.Get<int>("A"))
                .Descending()
                .ThenBy((d, c) => d.Get<int>("B"));
            Execute gatherData = new Execute((d, c) =>
            {
                content.Add(d.Content);
                return null;
            });
            engine.Pipelines.Add(count, count2, orderBy, gatherData);

            // When
            engine.Execute();

            // Then
            Assert.AreEqual(10, content.Count); // (4+1) * (21+1)
            CollectionAssert.AreEqual(new[] { "59", "510", "47", "48", "35", "36", "23", "24", "11", "12" }, content);
        }
    }
}
