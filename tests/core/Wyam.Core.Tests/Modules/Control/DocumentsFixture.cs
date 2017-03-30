using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Wyam.Core.Execution;
using Wyam.Core.Modules.Extensibility;
using Wyam.Testing;
using Wyam.Common.Execution;

namespace Wyam.Core.Tests.Modules.Control
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class DocumentsFixture : BaseFixture
    {
        public class ExecuteTests : DocumentsFixture
        {
            [Test]
            public void CountReturnsCorrectDocuments()
            {
                // Given
                Engine engine = new Engine();
                Core.Modules.Control.Documents documents = new Core.Modules.Control.Documents(5);
                engine.Pipelines.Add(documents);

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(5, engine.Documents.Count());
            }

            [Test]
            public void ContentReturnsCorrectDocuments()
            {
                // Given
                List<string> content = new List<string>();
                Engine engine = new Engine();
                Core.Modules.Control.Documents documents = new Core.Modules.Control.Documents("A", "B", "C", "D");
                Execute gatherData = new Execute(
                    (d, c) =>
                {
                    content.Add(d.Content);
                    return null;
                }, false);
                engine.Pipelines.Add(documents, gatherData);

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(4, content.Count);
                CollectionAssert.AreEqual(new[] { "A", "B", "C", "D" }, content);
            }

            [Test]
            public void MetadataReturnsCorrectDocuments()
            {
                // Given
                List<object> values = new List<object>();
                Engine engine = new Engine();
                Core.Modules.Control.Documents documents = new Core.Modules.Control.Documents(
                    new Dictionary<string, object> { { "Foo", "a" } },
                    new Dictionary<string, object> { { "Foo", "b" } },
                    new Dictionary<string, object> { { "Foo", "c" } });
                Execute gatherData = new Execute(
                    (d, c) =>
                {
                    values.Add(d["Foo"]);
                    return null;
                }, false);
                engine.Pipelines.Add(documents, gatherData);

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(3, values.Count);
                CollectionAssert.AreEqual(new[] { "a", "b", "c" }, values);
            }

            [Test]
            public void ContentAndMetadataReturnsCorrectDocuments()
            {
                // Given
                List<string> content = new List<string>();
                List<object> values = new List<object>();
                Engine engine = new Engine();
                Core.Modules.Control.Documents documents = new Core.Modules.Control.Documents(
                    Tuple.Create("A", new Dictionary<string, object> { { "Foo", "a" } }.AsEnumerable()),
                    Tuple.Create("B", new Dictionary<string, object> { { "Foo", "b" } }.AsEnumerable()),
                    Tuple.Create("C", new Dictionary<string, object> { { "Foo", "c" } }.AsEnumerable()));
                Execute gatherData = new Execute(
                    (d, c) =>
                {
                    content.Add(d.Content);
                    values.Add(d["Foo"]);
                    return null;
                }, false);
                engine.Pipelines.Add(documents, gatherData);

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(3, content.Count);
                Assert.AreEqual(3, values.Count);
                CollectionAssert.AreEqual(new[] { "A", "B", "C" }, content);
                CollectionAssert.AreEqual(new[] { "a", "b", "c" }, values);
            }
        }
    }
}
