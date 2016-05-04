using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Core.Documents;
using Wyam.Core.Execution;
using Wyam.Core.Meta;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Testing;
using Wyam.Testing.Modules;

namespace Wyam.Core.Tests.Modules.Control
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class GroupByTests : BaseFixture
    {
        public class ExecuteMethodTests : GroupByTests
        {
            [Test]
            public void SetsCorrectMetadata()
            {
                // Given
                List<int> groupKey = new List<int>();
                Engine engine = new Engine();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 7
                };
                GroupBy groupBy = new GroupBy((d, c) => d.Get<int>("A")%3, count);
                Execute gatherData = new Execute((d, c) =>
                {
                    groupKey.Add(d.Get<int>(Keys.GroupKey));
                    return null;
                });
                engine.Pipelines.Add(groupBy, gatherData);

                // When
                engine.Execute();

                // Then
                CollectionAssert.AreEquivalent(new[] {0, 1, 2}, groupKey);
            }

            [Test]
            public void SetsDocumentsInMetadata()
            {
                // Given
                List<IList<string>> content = new List<IList<string>>();
                Engine engine = new Engine();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 7
                };
                GroupBy groupBy = new GroupBy((d, c) => d.Get<int>("A")%3, count);
                OrderBy orderBy = new OrderBy((d, c) => d.Get<int>(Keys.GroupKey));
                Execute gatherData = new Execute((d, c) =>
                {
                    content.Add(d.Get<IList<IDocument>>(Keys.GroupDocuments).Select(x => x.Content).ToList());
                    return null;
                });
                engine.Pipelines.Add(groupBy, orderBy, gatherData);

                // When
                engine.Execute();

                // Then
                Assert.AreEqual(3, content.Count);
                CollectionAssert.AreEquivalent(new[] {"3", "6"}, content[0]);
                CollectionAssert.AreEquivalent(new[] {"1", "4", "7"}, content[1]);
                CollectionAssert.AreEquivalent(new[] {"2", "5", "8"}, content[2]);
            }
        }
    }
}
