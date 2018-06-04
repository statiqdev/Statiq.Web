using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;
using Wyam.Testing;

namespace Wyam.Json.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class JsonFixture : BaseFixture
    {
        private static string _jsonContent = @"{
  ""Email"": ""james@example.com"",
  ""Active"": true,
  ""CreatedDate"": ""2013-01-20T00:00:00Z"",
  ""Roles"": [
    ""User"",
    ""Admin""
  ]
}";
        public class ExecuteTests : JsonFixture
        {
            [Test]
            public void SetsMetadataKey()
            {
                // Given
                IExecutionContext context = Substitute.For<IExecutionContext>();
                IDocument document = Substitute.For<IDocument>();
                document.Content.Returns(_jsonContent);
                Json json = new Json("MyJson");

                // When
                json.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
                context.Received().GetDocument(document, Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.First().Key == "MyJson"));
            }

            [Test]
            public void GeneratesDynamicObject()
            {
                // Given
                IExecutionContext context = Substitute.For<IExecutionContext>();
                IDocument document = Substitute.For<IDocument>();
                IEnumerable<KeyValuePair<string, object>> items = null;
                context
                    .When(x => x.GetDocument(Arg.Any<IDocument>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                    .Do(x => items = x.Arg<IEnumerable<KeyValuePair<string, object>>>());
                document.Content.Returns(_jsonContent);
                Json json = new Json("MyJson");

                // When
                json.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
                Assert.AreEqual(1, items.Count());
                Assert.IsInstanceOf<ExpandoObject>(items.First().Value);
                Assert.AreEqual("james@example.com", (string)((dynamic)items.First().Value).Email);
                Assert.AreEqual(true, (bool)((dynamic)items.First().Value).Active);
                Assert.AreEqual(new DateTime(2013, 1, 20, 0, 0, 0, DateTimeKind.Utc), (DateTime)((dynamic)items.First().Value).CreatedDate);
                CollectionAssert.AreEqual(new[] { "User", "Admin" }, (IEnumerable)((dynamic)items.First().Value).Roles);
            }

            [Test]
            public void FlattensTopLevel()
            {
                // Given
                IExecutionContext context = Substitute.For<IExecutionContext>();
                IDocument document = Substitute.For<IDocument>();
                IEnumerable<KeyValuePair<string, object>> items = null;
                context
                    .When(x => x.GetDocument(Arg.Any<IDocument>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                    .Do(x => items = x.Arg<IEnumerable<KeyValuePair<string, object>>>());
                document.Content.Returns(_jsonContent);
                Json json = new Json();

                // When
                json.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
                Assert.AreEqual(4, items.Count());
                Assert.AreEqual("james@example.com", items.First(x => x.Key == "Email").Value);
                Assert.AreEqual(true, items.First(x => x.Key == "Active").Value);
                Assert.AreEqual(new DateTime(2013, 1, 20, 0, 0, 0, DateTimeKind.Utc), items.First(x => x.Key == "CreatedDate").Value);
                CollectionAssert.AreEqual(new[] { "User", "Admin" }, (IEnumerable)items.First(x => x.Key == "Roles").Value);
            }

            [Test]
            [Parallelizable(ParallelScope.None)]
            public void ReturnsDocumentOnError()
            {
                // Given
                RemoveListener();
                IDocument document = Substitute.For<IDocument>();
                document.Content.Returns("asdf");
                IExecutionContext context = Substitute.For<IExecutionContext>();
                Json json = new Json("MyJson");

                // When
                List<IDocument> results = json.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(0).GetDocument(Arg.Any<IDocument>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
                Assert.IsTrue(results.Single().Equals(document));
            }
        }
    }
}