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
using Wyam.Common.Pipelines;
using Wyam.Common.Tracing;

namespace Wyam.Modules.Json.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class JsonFixture
    {
        private static string JsonContent = @"{
  ""Email"": ""james@example.com"",
  ""Active"": true,
  ""CreatedDate"": ""2013-01-20T00:00:00Z"",
  ""Roles"": [
    ""User"",
    ""Admin""
  ]
}";
        [Test]
        public void SetsMetadataKey()
        {
            // Given
            IDocument document = Substitute.For<IDocument>();
            document.Content.Returns(JsonContent);
            Json json = new Json("MyJson");

            // When
            json.Execute(new[] {document}, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(1).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            document.Received().Clone(Arg.Is<IEnumerable<KeyValuePair<string, object>>>(x => x.First().Key == "MyJson"));
        }

        [Test]
        public void GeneratesDynamicObject()
        {
            // Given
            IDocument document = Substitute.For<IDocument>();
            IEnumerable<KeyValuePair<string, object>> items = null;
            document
                .When(x => x.Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                .Do(x => items = x.Arg<IEnumerable<KeyValuePair<string, object>>>());
            document.Content.Returns(JsonContent);
            Json json = new Json("MyJson");

            // When
            json.Execute(new[] {document}, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(1).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            Assert.AreEqual(1, items.Count());
            Assert.IsInstanceOf<ExpandoObject>(items.First().Value);
            Assert.AreEqual("james@example.com", (string)((dynamic)items.First().Value).Email);
            Assert.AreEqual(true, (bool)((dynamic)items.First().Value).Active);
            Assert.AreEqual(new DateTime(2013, 1, 20, 0, 0, 0, DateTimeKind.Utc), (DateTime)((dynamic)items.First().Value).CreatedDate);
            CollectionAssert.AreEqual(new [] { "User", "Admin" }, (IEnumerable)((dynamic)items.First().Value).Roles);
        }

        [Test]
        public void FlattensTopLevel()
        {
            // Given
            IDocument document = Substitute.For<IDocument>();
            IEnumerable<KeyValuePair<string, object>> items = null;
            document
                .When(x => x.Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                .Do(x => items = x.Arg<IEnumerable<KeyValuePair<string, object>>>());
            document.Content.Returns(JsonContent);
            Json json = new Json();

            // When
            json.Execute(new[] { document }, null).ToList();  // Make sure to materialize the result list

            // Then
            document.Received(1).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
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
            IDocument document = Substitute.For<IDocument>();
            document.Content.Returns("asdf");
            document.Source.Returns(string.Empty);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            TraceListener traceListener = new TraceListener();
            Trace.AddListener(traceListener);
            Json json = new Json("MyJson");

            // When
            List<IDocument> results = json.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Trace.RemoveListener(traceListener);
            Assert.IsTrue(traceListener.Messages.Count > 0);
            document.Received(0).Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            Assert.IsTrue(results.Single().Equals(document));
        }

        public class TraceListener : System.Diagnostics.ConsoleTraceListener
        {
            public List<string> Messages { get; set; } = new List<string>();

            public override void TraceEvent(System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id, string message)
            {
                LogMessage(eventType, message);
            }

            public override void TraceEvent(System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id, string format, params object[] args)
            {
                LogMessage(eventType, string.Format(format, args));
            }

            private void LogMessage(System.Diagnostics.TraceEventType eventType, string message)
            {
                if (eventType == System.Diagnostics.TraceEventType.Critical
                    || eventType == System.Diagnostics.TraceEventType.Error)
                {
                    Messages.Add(message);
                }
            }
        }
    }
}
