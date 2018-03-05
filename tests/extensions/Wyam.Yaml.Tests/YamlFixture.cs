using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Core;
using Wyam.Core.Modules;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Testing;
using Wyam.Yaml.Dynamic;

namespace Wyam.Yaml.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class YamlFixture : BaseFixture
    {
        public class ExecuteTests : YamlFixture
        {
            [Test]
            public void SetsMetadataKey()
            {
                // Given
                IExecutionContext context = Substitute.For<IExecutionContext>();
                IDocument document = Substitute.For<IDocument>();
                document.Content.Returns(@"A: 1");
                Yaml yaml = new Yaml("MyYaml");

                // When
                yaml.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
                context.Received().GetDocument(Arg.Is(document), Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.First().Key == "MyYaml"));
            }

            [Test]
            public void GeneratesDynamicObject()
            {
                // Given
                IDocument document = Substitute.For<IDocument>();
                IEnumerable<KeyValuePair<string, object>> items = null;
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context
                    .When(x => x.GetDocument(Arg.Any<IDocument>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                    .Do(x => items = x.Arg<IEnumerable<KeyValuePair<string, object>>>());
                document.Content.Returns(@"
A: 1
B: true
C: Yes
");
                Yaml yaml = new Yaml("MyYaml");

                // When
                yaml.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
                Assert.AreEqual(1, items.Count());
                Assert.IsInstanceOf<DynamicYaml>(items.First().Value);
                Assert.AreEqual(1, (int)((dynamic)items.First().Value).A);
                Assert.AreEqual(true, (bool)((dynamic)items.First().Value).B);
                Assert.AreEqual("Yes", (string)((dynamic)items.First().Value).C);
            }

            [Test]
            public void FlattensTopLevelScalarNodes()
            {
                // Given
                IDocument document = Substitute.For<IDocument>();
                IEnumerable<KeyValuePair<string, object>> items = null;
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context
                    .When(x => x.GetDocument(Arg.Any<IDocument>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                    .Do(x => items = x.Arg<IEnumerable<KeyValuePair<string, object>>>());
                document.Content.Returns(@"
A: 1
B: true
C: Yes
");
                Yaml yaml = new Yaml();

                // When
                yaml.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
                Assert.AreEqual(3, items.Count());
                Assert.AreEqual("1", items.First(x => x.Key == "A").Value);
                Assert.AreEqual("true", items.First(x => x.Key == "B").Value);
                Assert.AreEqual("Yes", items.First(x => x.Key == "C").Value);
            }

            [Test]
            public void UsesDocumentNestingForComplexChildren()
            {
                // Given
                IDocument document = Substitute.For<IDocument>();
                IList<KeyValuePair<string, object>> items = null;
                List<IList<KeyValuePair<string, object>>> subItems = new List<IList<KeyValuePair<string, object>>>();
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context
                    .When(x => x.GetDocument(Arg.Any<IDocument>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                    .Do(x => items = x.Arg<IEnumerable<KeyValuePair<string, object>>>().ToList());
                context
                    .When(x => x.GetDocument(Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                    .Do(x => subItems.Add(x.Arg<IEnumerable<KeyValuePair<string, object>>>().ToList()));
                document.Content.Returns(@"
C:
  - X: 1
    Y: 2
  - X: 4
    Z: 5
");
                Yaml yaml = new Yaml();

                // When
                yaml.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
                Assert.AreEqual(1, items.Count);
                IDocument[] documents = items.First(x => x.Key == "C").Value as IDocument[];
                Assert.IsNotNull(documents);

                context.Received(2).GetDocument(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
                Assert.AreEqual(2, subItems.Count);
                Assert.AreEqual("X", subItems[0][0].Key);
                Assert.AreEqual("1", subItems[0][0].Value);
                Assert.AreEqual("Y", subItems[0][1].Key);
                Assert.AreEqual("2", subItems[0][1].Value);
                Assert.AreEqual("X", subItems[1][0].Key);
                Assert.AreEqual("4", subItems[1][0].Value);
                Assert.AreEqual("Z", subItems[1][1].Key);
                Assert.AreEqual("5", subItems[1][1].Value);
            }
        }
    }
}