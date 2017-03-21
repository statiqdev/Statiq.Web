using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Execution;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Json.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class GenerateJsonFixture : BaseFixture
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

        public class ExecuteTests : GenerateJsonFixture
        {
            [Test]
            public void GetsObjectFromMetadata()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    {"JsonObject", JsonObject}
                });
                GenerateJson generateJson = new GenerateJson("JsonObject");

                // When
                IList<IDocument> results = generateJson.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { JsonContent }));
            }

            [Test]
            public void GetsObjectFromContextDelegate()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                GenerateJson generateJson = new GenerateJson(ctx => JsonObject);

                // When
                IList<IDocument> results = generateJson.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { JsonContent }));
            }

            [Test]
            public void GetsObjectFromDocumentDelegate()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    {"JsonObject", JsonObject}
                });
                GenerateJson generateJson = new GenerateJson((doc, ctx) => doc.Get("JsonObject"));

                // When
                IList<IDocument> results = generateJson.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { JsonContent }));
            }

            [Test]
            public void SetsMetadataKey()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "JsonObject", JsonObject }
                });
                GenerateJson generateJson = new GenerateJson("JsonObject", "OutputKey");

                // When
                IList<IDocument> results = generateJson.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(
                    results.Select(x => x.First(y => y.Key == "OutputKey")),
                    Is.EquivalentTo(new[] { new KeyValuePair<string, object>("OutputKey", JsonContent) }));
            }

            [Test]
            public void DoesNotIndent()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    {"JsonObject", JsonObject}
                });
                GenerateJson generateJson = new GenerateJson("JsonObject").WithIndenting(false);
                string nonIndentedJsonContent = JsonContent.Replace(" ", "").Replace("\r\n", "");

                // When
                IList<IDocument> results = generateJson.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { nonIndentedJsonContent }));
            }
        }
    }
}