using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.TextGeneration.Tests
{
    [TestFixture]
    public class GenerateContentFixture : BaseFixture
    {
        public class ExecuteTests : GenerateContentFixture
        {
            [Test]
            public void GeneratingContentFromStringTemplateSetsContent()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                IModule generateContent = new GenerateContent(@"[rs:4;,\s]{<noun>}").WithSeed(1000);

                // When
                IList<IDocument> results = generateContent.Execute(new[] { document }, context).ToList(); // Make sure to materialize the result list

                // Then
                Assert.That(results.Select(x => x.Content), Is.EquivalentTo(new[] { "nectarine, gambler, marijuana, chickadee" }));
            }
        }
    }
}
