using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Pipelines;
using Wyam.Testing;

namespace Wyam.Modules.Json.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class GenerateJsonTests : BaseFixture
    {
        private class Account
        {
            public string Email { get; set; }
            public bool Active { get; set; }
            public DateTime CreatedDate { get; set; }
            public IList<string> Roles { get; set; }
        }

        private static Account JsonObject = new Account
        {
            Email = "james@example.com",
            Active = true,
            CreatedDate = new DateTime(2013, 1, 20, 0, 0, 0, DateTimeKind.Utc),
            Roles = new List<string>
            {
                "User",
                "Admin"
            }
        };

        private static string JsonContent = @"{
  ""Email"": ""james@example.com"",
  ""Active"": true,
  ""CreatedDate"": ""2013-01-20T00:00:00Z"",
  ""Roles"": [
    ""User"",
    ""Admin""
  ]
}";

        public class ExecuteMethodTests : GenerateJsonTests
        {
            [Test]
            public void GetsObjectFromMetadata()
            {
                // Given
                IExecutionContext context = Substitute.For<IExecutionContext>();
                IDocument document = Substitute.For<IDocument>();
                document.Get("JsonObject").Returns(JsonObject);
                GenerateJson generateJson = new GenerateJson("JsonObject");

                // When
                generateJson.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, Arg.Is(JsonContent));
            }

            [Test]
            public void GetsObjectFromContextDelegate()
            {
                // Given
                IExecutionContext context = Substitute.For<IExecutionContext>();
                IDocument document = Substitute.For<IDocument>();
                GenerateJson generateJson = new GenerateJson(ctx => JsonObject);

                // When
                generateJson.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, Arg.Is(JsonContent));
            }

            [Test]
            public void GetsObjectFromDocumentDelegate()
            {
                // Given
                IExecutionContext context = Substitute.For<IExecutionContext>();
                IDocument document = Substitute.For<IDocument>();
                document.Get("JsonObject").Returns(JsonObject);
                GenerateJson generateJson = new GenerateJson((doc, ctx) => doc.Get("JsonObject"));

                // When
                generateJson.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, Arg.Is(JsonContent));
            }

            [Test]
            public void SetsMetadataKey()
            {
                // Given
                IExecutionContext context = Substitute.For<IExecutionContext>();
                IDocument document = Substitute.For<IDocument>();
                document.Get("JsonObject").Returns(JsonObject);
                GenerateJson generateJson = new GenerateJson("JsonObject", "OutputKey");

                // When
                generateJson.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
                context.Received().GetDocument(document, Arg.Is<IEnumerable<KeyValuePair<string, object>>>(
                    x => x.First().Key == "OutputKey" && (string)x.First().Value == JsonContent));
            }

            [Test]
            public void DoesNotIndent()
            {
                // Given
                IExecutionContext context = Substitute.For<IExecutionContext>();
                IDocument document = Substitute.For<IDocument>();
                document.Get("JsonObject").Returns(JsonObject);
                GenerateJson generateJson = new GenerateJson("JsonObject").WithIndenting(false);
                string nonIndentedJsonContent = JsonContent.Replace(" ", "").Replace("\r\n", "");

                // When
                generateJson.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, Arg.Is(nonIndentedJsonContent));
            }
        }
    }
}