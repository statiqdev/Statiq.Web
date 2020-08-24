using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.App;
using Statiq.Common;
using Statiq.Testing;
using Statiq.Web.Pipelines;

namespace Statiq.Web.Tests.Pipelines
{
    [TestFixture]
    public class ScriptFixture : BaseFixture
    {
        public class ExecuteTests : ScriptFixture
        {
            [Test]
            public async Task ReturnsStringContent()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/a/b/c.csx", "int a = 1; int b = 2; return $\"The number is {a + b}.\";" }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Scripts)][Phase.Process].ShouldHaveSingleItem();
                (await document.GetContentStringAsync()).ShouldBe("The number is 3.");
            }

            [Test]
            public async Task ReturnsSingleDocument()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/a/b/c.csx",
                        @"
int a = 1;
int b = 2;
return await Context.CreateDocumentAsync(
    new MetadataItems
    {
        { ""Foo"", ""Bar"" }
    },
    $""The number is {a + b}."");"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Scripts)][Phase.Process].ShouldHaveSingleItem();
                document["Foo"].ShouldBe("Bar");
                (await document.GetContentStringAsync()).ShouldBe("The number is 3.");
            }

            [Test]
            public async Task ReturnsMultipleDocuments()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/a/b/c.csx",
                        @"
int a = 1;
int b = 2;
return new IDocument[]
{
    await Context.CreateDocumentAsync(
        new MetadataItems
        {
            { ""Foo"", ""Bar"" }
        },
        $""The number is {a}.""),
    await Context.CreateDocumentAsync(
        new MetadataItems
        {
            { ""Foo"", ""Baz"" }
        },
        $""The number is {b}."")
};"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                ImmutableArray<IDocument> documents = result.Outputs[nameof(Scripts)][Phase.Process];
                documents.Length.ShouldBe(2);
                documents.Select(x => x.GetContentStringAsync().Result).ShouldBe(new[] { "The number is 1.", "The number is 2." });
                documents.Select(x => x["Foo"]).ShouldBe(new[] { "Bar", "Baz" });
            }
        }
    }
}
