using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Core.Modules.Control;
using Wyam.Testing;

namespace Wyam.Core.Tests.Modules.Control
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class MergeTests : BaseFixture
    {
        public class ExecuteMethodTests : MergeTests
        {
            [Test]
            public void ReplacesContent()
            {
                // Given
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20
                };
                engine.Pipelines.Add("Test", a, new Merge(b), 
                    new Core.Modules.Metadata.Meta("Content", (doc, ctx) => doc.Content));

                // When
                engine.Execute();

                // Then
                CollectionAssert.AreEqual(new [] { "21" }, engine.Documents["Test"].Select(x => x["Content"]));
            }

            [Test]
            public void CombinesMetadata()
            {
                // Given
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20
                };
                engine.Pipelines.Add("Test", a, new Merge(b));

                // When
                engine.Execute();

                // Then
                CollectionAssert.AreEqual(new[] { 11 }, engine.Documents["Test"].Select(x => x["A"]));
                CollectionAssert.AreEqual(new[] { 21 }, engine.Documents["Test"].Select(x => x["B"]));
            }

            [Test]
            public void CombinesAndOverwritesMetadata()
            {
                // Given
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10
                };
                CountModule b = new CountModule("A")
                {
                    Value = 20
                };
                engine.Pipelines.Add("Test", a, new Merge(b));

                // When
                engine.Execute();

                // Then
                CollectionAssert.AreEqual(new[] { 21 }, engine.Documents["Test"].Select(x => x["A"]));
            }

            [Test]
            public void SingleInputSingleResult()
            {
                // Given
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20
                };
                engine.Pipelines.Add("Test", a, new Merge(b));

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(1, a.OutputCount);
                Assert.AreEqual(1, b.OutputCount);
                CollectionAssert.AreEqual(new[] { 11 }, engine.Documents["Test"].Select(x => x["A"]));
                CollectionAssert.AreEqual(new[] { 21 }, engine.Documents["Test"].Select(x => x["B"]));
            }

            [Test]
            public void SingleInputMultipleResults()
            {
                // Given
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    AdditionalOutputs = 1
                };
                engine.Pipelines.Add("Test", a, new Merge(b));

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(1, a.OutputCount);
                Assert.AreEqual(2, b.OutputCount);
                CollectionAssert.AreEqual(new[] { 11, 11 }, engine.Documents["Test"].Select(x => x["A"]));
                CollectionAssert.AreEqual(new[] { 21, 22 }, engine.Documents["Test"].Select(x => x["B"]));
            }

            [Test]
            public void MultipleInputsSingleResult()
            {
                // Given
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10,
                    AdditionalOutputs = 1
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20
                };
                engine.Pipelines.Add("Test", a, new Merge(b));

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(1, b.OutputCount);
                CollectionAssert.AreEqual(new[] { 11, 12 }, engine.Documents["Test"].Select(x => x["A"]));
                CollectionAssert.AreEqual(new[] { 21, 21 }, engine.Documents["Test"].Select(x => x["B"]));
            }

            [Test]
            public void MultipleInputsMultipleResults()
            {
                // Given
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10,
                    AdditionalOutputs = 1
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    AdditionalOutputs = 1
                };
                engine.Pipelines.Add("Test", a, new Merge(b));

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(2, b.OutputCount);
                CollectionAssert.AreEqual(new[] { 11, 11, 12, 12 }, engine.Documents["Test"].Select(x => x["A"]));
                CollectionAssert.AreEqual(new[] { 21, 22, 21, 22 }, engine.Documents["Test"].Select(x => x["B"]));
            }

            [Test]
            public void SingleInputSingleResultForEachDocument()
            {
                // Given
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20
                };
                engine.Pipelines.Add("Test", a, new Merge(b).ForEachDocument(),
                    new Core.Modules.Metadata.Meta("Content", (doc, ctx) => doc.Content));

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(1, a.OutputCount);
                Assert.AreEqual(1, b.OutputCount);
                CollectionAssert.AreEqual(new[] { "1121" }, engine.Documents["Test"].Select(x => x["Content"]));
            }

            [Test]
            public void SingleInputMultipleResultsForEachDocument()
            {
                // Given
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    AdditionalOutputs = 1
                };
                engine.Pipelines.Add("Test", a, new Merge(b).ForEachDocument(),
                    new Core.Modules.Metadata.Meta("Content", (doc, ctx) => doc.Content));

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(1, a.OutputCount);
                Assert.AreEqual(2, b.OutputCount);
                CollectionAssert.AreEqual(new[] { "1121", "1122" }, engine.Documents["Test"].Select(x => x["Content"]));
            }

            [Test]
            public void MultipleInputsSingleResultForEachDocument()
            {
                // Given
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10,
                    AdditionalOutputs = 1
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20
                };
                engine.Pipelines.Add("Test", a, new Merge(b).ForEachDocument(),
                    new Core.Modules.Metadata.Meta("Content", (doc, ctx) => doc.Content));

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(2, b.OutputCount);
                CollectionAssert.AreEqual(new[] { "1121", "1222" }, engine.Documents["Test"].Select(x => x["Content"]));
            }

            [Test]
            public void MultipleInputsMultipleResultsForEachDocument()
            {
                // Given
                Engine engine = new Engine();
                CountModule a = new CountModule("A")
                {
                    Value = 10,
                    AdditionalOutputs = 1
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    AdditionalOutputs = 1
                };
                engine.Pipelines.Add("Test", a, new Merge(b).ForEachDocument(),
                    new Core.Modules.Metadata.Meta("Content", (doc, ctx) => doc.Content));

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(2, a.OutputCount);
                Assert.AreEqual(4, b.OutputCount);
                CollectionAssert.AreEqual(new[] { "1121", "1122", "1223", "1224" }, engine.Documents["Test"].Select(x => x["Content"]));
            }
        }
    }
}
