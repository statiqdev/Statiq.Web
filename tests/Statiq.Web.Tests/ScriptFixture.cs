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

namespace Statiq.Web.Tests
{
    // Scripts are processed by the Inputs pipeline, so these tests are more about how they flow through all the pipelines
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
                IDocument document = result.Outputs[nameof(Assets)][Phase.Output].ShouldHaveSingleItem();
                document.Get<ContentType>(WebKeys.ContentType).ShouldBe(ContentType.Asset);
                (await document.GetContentStringAsync()).ShouldBe("The number is 3.");
            }

            [Test]
            public async Task SetsDestinationFromFrontMatter()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/a/b/c.csx",
                        @"DestinationExtension: .txt
---
int a = 1; int b = 2; return $\""The number is {a + b}.\"";"
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
                IDocument document = result.Outputs[nameof(Assets)][Phase.Process].ShouldHaveSingleItem();
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
                ImmutableArray<IDocument> documents = result.Outputs[nameof(Assets)][Phase.Process];
                documents.Length.ShouldBe(2);
                documents.Select(x => x.GetContentStringAsync().Result).ShouldBe(new[] { "The number is 1.", "The number is 2." });
                documents.Select(x => x["Foo"]).ShouldBe(new[] { "Bar", "Baz" });
            }

            [Test]
            public async Task ScriptWithMarkdownExtensionReturnsMarkdownContent()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/foo.md",
                        @"
Script: true
---
string foo = ""Bar"";
return $""# {foo}\nContent"";"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.Process].ShouldHaveSingleItem();
                document.Destination.ShouldBe("foo.html");
                document.GetContentStringAsync().Result.ShouldBe(
                    @"<h1 id=""bar"">Bar</h1>
<p>Content</p>
",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ScriptWithCsxExtensionAndMediaTypeReturnsMarkdownContent()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/foo.csx",
                        @"
/*
MediaType: text/markdown
Script: true
*/
string foo = ""Bar"";
return $""# {foo}\nContent"";"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.Process].ShouldHaveSingleItem();
                document.Destination.ShouldBe("foo.html");
                document.GetContentStringAsync().Result.ShouldBe(
                    @"<h1 id=""bar"">Bar</h1>
<p>Content</p>
",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ScriptWithMdAndCsxExtensionReturnsMarkdownContent()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/foo.md.csx",
                        @"
string foo = ""Bar"";
return $""# {foo}\nContent"";"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.Process].ShouldHaveSingleItem();
                document.Destination.ShouldBe("foo.html");
                document.GetContentStringAsync().Result.ShouldBe(
                    @"<h1 id=""bar"">Bar</h1>
<p>Content</p>
",
                    StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}
