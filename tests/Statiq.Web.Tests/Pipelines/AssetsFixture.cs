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
    public class AssetsFixture : BaseFixture
    {
        public class ExecuteTests : AssetsFixture
        {
            [Test]
            public async Task ProcessesJsonSidecarFileWithDifferentExtension()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/a/b/c.foo", "Foobar" },
                    { "/input/a/b/_c.json", "{ \"Fizz\": \"Buzz\" }" }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Assets)][Phase.Process].ShouldHaveSingleItem();
                document.Destination.ShouldBe("a/b/c.foo");
                document["Fizz"].ShouldBe("Buzz");
                (await document.GetContentStringAsync()).ShouldBe("Foobar");
            }

            [Test]
            public async Task ReturnsStringContent()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/a/b/c.csx", "int a = 1; int b = 2; return $\"The number is {a + b}.\";" }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Assets)][Phase.Output].ShouldHaveSingleItem();
                document.Get<ContentType>(WebKeys.ContentType).ShouldBe(ContentType.Asset);
                (await document.GetContentStringAsync()).ShouldBe("The number is 3.");
            }

            [Test]
            public async Task SetsDestinationFromFrontMatter()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/a/b/c.csx",
                        @"DestinationExtension: .txt
---
int a = 1; int b = 2; return $""The number is {a + b}."";"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Assets)][Phase.Output].ShouldHaveSingleItem();
                document.Get<ContentType>(WebKeys.ContentType).ShouldBe(ContentType.Asset);
                document.Destination.ShouldBe("a/b/c.txt");
                (await document.GetContentStringAsync()).ShouldBe("The number is 3.");
            }

            [Test]
            public async Task ReturnsSingleDocument()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/a/b/c.csx",
                        @"
int a = 1;
int b = 2;
return Context.CreateDocument(
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
                IDocument document = result.Outputs[nameof(Assets)][Phase.Process].ShouldHaveSingleItem();
                document["Foo"].ShouldBe("Bar");
                (await document.GetContentStringAsync()).ShouldBe("The number is 3.");
            }

            [Test]
            public async Task ReturnsMultipleDocuments()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/a/b/c.csx",
                        @"
int a = 1;
int b = 2;
return new IDocument[]
{
    Context.CreateDocument(
        new MetadataItems
        {
            { ""Foo"", ""Bar"" }
        },
        $""The number is {a}.""),
    Context.CreateDocument(
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
                ImmutableArray<IDocument> documents = result.Outputs[nameof(Assets)][Phase.Process];
                documents.Length.ShouldBe(2);
                documents.Select(x => x.GetContentStringAsync().Result)
                    .ShouldBe(new[] { "The number is 1.", "The number is 2." }); // Should be returned in natural order
                documents.Select(x => x["Foo"]).ShouldBe(new[] { "Bar", "Baz" });  // Should be returned in natural order
            }

            [Test]
            public async Task RemoveScriptExtensionIsFalse()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/foo.md.csx",
                        @"RemoveScriptExtension: false
---
string foo = ""Bar"";
return $""# {foo}\nContent"";"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Assets)][Phase.Process].ShouldHaveSingleItem();
                document.Destination.ShouldBe("foo.md.csx");
                document.GetContentStringAsync().Result.ShouldBe(
                    @"# Bar
Content",
                    StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}