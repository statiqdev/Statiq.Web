using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.App;
using Statiq.Common;
using Statiq.Html;
using Statiq.Testing;
using Statiq.Web.Pipelines;

namespace Statiq.Web.Tests.Pipelines
{
    [TestFixture]
    public class ContentFixture : BaseFixture
    {
        public class ExecuteTests : DataFixture
        {
            [Test]
            public async Task DefaultGatherHeadingsLevel()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/foo.html",
                        GatherHeadingsFile
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.Process].ShouldHaveSingleItem();
                document
                    .GetDocumentList(HtmlKeys.Headings)
                    .Flatten()
                    .Select(x => x.GetContentStringAsync().Result)
                    .ShouldBe(new[] { "1.1", "1.2" }, true);
            }

            [Test]
            public async Task GlobalGatherHeadingsLevelSetting()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(WebKeys.GatherHeadingsLevel, 2);
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/foo.html",
                        GatherHeadingsFile
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.Process].ShouldHaveSingleItem();
                document
                    .GetDocumentList(HtmlKeys.Headings)
                    .Flatten()
                    .Select(x => x.GetContentStringAsync().Result)
                    .ShouldBe(new[] { "1.1", "1.2", "2.1", "2.2", "2.3" }, true);
            }

            [Test]
            public async Task DocumentGatherHeadingsLevelSetting()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(WebKeys.GatherHeadingsLevel, 3);
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/foo.html",
                        @"GatherHeadingsLevel: 2
---" + GatherHeadingsFile
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.Process].ShouldHaveSingleItem();
                document
                    .GetDocumentList(HtmlKeys.Headings)
                    .Flatten()
                    .Select(x => x.GetContentStringAsync().Result)
                    .ShouldBe(new[] { "1.1", "1.2", "2.1", "2.2", "2.3" }, true);
            }

            [Test]
            public async Task LayoutMetadata()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/Layout/Test.md",
                        @"Layout: _Layout.cshtml
---
# Heading

This is a test"
                    },
                    {
                        "/input/Layout/_Layout.cshtml",
                        @"<div>LAYOUT</div>
    @RenderBody()"
                    },
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.Output].ShouldHaveSingleItem();
                (await document.GetContentStringAsync()).ShouldBe(
                    @"<div>LAYOUT</div>
    <h1 id=""heading"">Heading</h1>
<p>This is a test</p>
",
                    StringCompareShould.IgnoreLineEndings);
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
                        @"/*
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

        public const string GatherHeadingsFile = @"
<html>
  <head>
  </head>
  <body>
    <div>a</div>
    <h1>1.1</h1>
    <div>b</div>
    <h2>2.1</h2>
    <div>b</div>
    <h2>2.2</h2>
    <div>c</div>
    <h1>1.2</h1>
    <div>d</div>
    <h2>2.3</h2>
    <div>e</div>
    <h3>3.1</h3>
    <div>f</div>
  </body>
</html>";
    }
}
