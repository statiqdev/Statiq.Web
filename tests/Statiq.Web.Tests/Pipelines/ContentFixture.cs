using System;
using System.Linq;
using System.Threading.Tasks;
using Markdig.Extensions.Bootstrap;
using NUnit.Framework;
using Shouldly;
using Statiq.App;
using Statiq.Common;
using Statiq.Core;
using Statiq.Markdown;
using Statiq.Testing;
using Statiq.Web.Pipelines;

namespace Statiq.Web.Tests.Pipelines
{
    [TestFixture]
    public class ContentFixture : BaseFixture
    {
        public class ExecuteTests : DataFixture
        {
            // https://github.com/statiqdev/Statiq.Web/issues/981
            // Markdig will encode the quote character reference, but we need to make sure AngleSharp
            // doesn't double-encode the "&" part of the "&quot;" that Markdig produced
            [Test]
            public async Task ExcerptShouldNotDoubleEscapeEntities()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddFile("/input/foo.md", @"# Hello World

Sunny ""day"" chasing

the clouds away
                ");

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.PostProcess].ShouldHaveSingleItem();
                document.GetString(Keys.Excerpt).ShouldBe(@"<p>Sunny &quot;day&quot; chasing</p>");
            }

            [Test]
            public async Task DefaultGatherHeadingsLevel()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
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
                    .GetDocumentList(Keys.Headings)
                    .Flatten()
                    .Select(x => x.GetContentStringAsync().Result)
                    .ShouldBe(new[] { "1.1", "1.2", "2.1", "2.2", "2.3" }, true);
            }

            [Test]
            public async Task GlobalGatherHeadingsLevelSetting()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper
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
                    .GetDocumentList(Keys.Headings)
                    .Flatten()
                    .Select(x => x.GetContentStringAsync().Result)
                    .ShouldBe(new[] { "1.1", "1.2", "2.1", "2.2", "2.3" }, true);
            }

            [Test]
            public async Task DocumentGatherHeadingsLevelSetting()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper
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
                    .GetDocumentList(Keys.Headings)
                    .Flatten()
                    .Select(x => x.GetContentStringAsync().Result)
                    .ShouldBe(new[] { "1.1", "1.2", "2.1", "2.2", "2.3" }, true);
            }

            [Test]
            public async Task LayoutViewStart()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/Test.md",
                        @"# Heading

This is a test"
                    },
                    {
                        "/input/_Layout.cshtml",
                        @"<html><head></head><body>
    <div>LAYOUT</div>
    @RenderBody()
</body></html>"
                    },
                    {
                        "/input/_ViewStart.cshtml",
                        @"@{
    Layout = ""_Layout"";
}"
                    },
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.Output].ShouldHaveSingleItem();
                (await document.GetContentStringAsync()).ShouldBe(
                    @"<html><head></head><body>
    <div>LAYOUT</div>
    <h1 id=""heading"">Heading</h1>
<p>This is a test</p>

</body></html>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task HtmlFragmentFileAppliesLayout()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/Test.fhtml",
                        @"<p>This is a test</p>"
                    },
                    {
                        "/input/_Layout.cshtml",
                        @"<html><head></head><body>
    <div>LAYOUT</div>
    @RenderBody()
</body></html>"
                    },
                    {
                        "/input/_ViewStart.cshtml",
                        @"@{
    Layout = ""_Layout"";
}"
                    },
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.Output].ShouldHaveSingleItem();
                (await document.GetContentStringAsync()).ShouldBe(
                    @"<html><head></head><body>
    <div>LAYOUT</div>
    <p>This is a test</p>
</body></html>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task HtmlFragmentAppliesLayout()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/Test.html",
                        @"<p>This is a test</p>"
                    },
                    {
                        "/input/_Layout.cshtml",
                        @"<html><head></head><body>
    <div>LAYOUT</div>
    @RenderBody()
</body></html>"
                    },
                    {
                        "/input/_ViewStart.cshtml",
                        @"@{
    Layout = ""_Layout"";
}"
                    },
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.Output].ShouldHaveSingleItem();
                (await document.GetContentStringAsync()).ShouldBe(
                    @"<html><head></head><body>
    <div>LAYOUT</div>
    <p>This is a test</p>
</body></html>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task HtmlDoesNotApplyLayout()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/Test.html",
                        @"<html><head></head><body><p>This is a test</p></body></html>"
                    },
                    {
                        "/input/_Layout.cshtml",
                        @"<html><head></head><body>
    <div>LAYOUT</div>
    @RenderBody()
</body></html>"
                    },
                    {
                        "/input/_ViewStart.cshtml",
                        @"@{
    Layout = ""_Layout"";
}"
                    },
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.Output].ShouldHaveSingleItem();
                (await document.GetContentStringAsync()).ShouldBe(
                    @"<html><head></head><body><p>This is a test</p></body></html>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task HtmlFragmentFileAppliesAlternateLayout()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddTemplate(
                        "foobar",
                        ContentType.Content,
                        Phase.PostProcess,
                        new ExecuteConfig(Config.FromDocument(async doc => "start" + (await doc.GetContentStringAsync()) + "end")))
                    .SetDefaultLayoutTemplate("foobar");
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/Test.fhtml",
                        @"<p>This is a test</p>"
                    },
                    {
                        "/input/_Layout.cshtml",
                        @"<html><head></head><body>
    <div>LAYOUT</div>
    @RenderBody()
</body></html>"
                    },
                    {
                        "/input/_ViewStart.cshtml",
                        @"@{
    Layout = ""_Layout"";
}"
                    },
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.Output].ShouldHaveSingleItem();
                (await document.GetContentStringAsync()).ShouldBe(
                    @"start<p>This is a test</p>end",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task HtmlFragmentAppliesAlternateLayout()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>())
                    .SetDefaultLayoutModule(new ExecuteConfig(Config.FromDocument(async doc => "start" + (await doc.GetContentStringAsync()) + "end")));
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/Test.html",
                        @"<p>This is a test</p>"
                    },
                    {
                        "/input/_Layout.cshtml",
                        @"<html><head></head><body>
    <div>LAYOUT</div>
    @RenderBody()
</body></html>"
                    },
                    {
                        "/input/_ViewStart.cshtml",
                        @"@{
    Layout = ""_Layout"";
}"
                    },
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.Output].ShouldHaveSingleItem();
                (await document.GetContentStringAsync()).ShouldBe(
                    @"start<p>This is a test</p>end",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task HtmlFragmentFileAppliesAlternateLayoutModule()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>())
                    .SetDefaultLayoutModule(new ExecuteConfig(Config.FromDocument(async doc => "start" + (await doc.GetContentStringAsync()) + "end")));
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/Test.fhtml",
                        @"<p>This is a test</p>"
                    },
                    {
                        "/input/_Layout.cshtml",
                        @"<html><head></head><body>
    <div>LAYOUT</div>
    @RenderBody()
</body></html>"
                    },
                    {
                        "/input/_ViewStart.cshtml",
                        @"@{
    Layout = ""_Layout"";
}"
                    },
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.Output].ShouldHaveSingleItem();
                (await document.GetContentStringAsync()).ShouldBe(
                    @"start<p>This is a test</p>end",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task HtmlFragmentAppliesAlternateLayoutModule()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddTemplate(
                        "foobar",
                        ContentType.Content,
                        Phase.PostProcess,
                        new ExecuteConfig(Config.FromDocument(async doc => "start" + (await doc.GetContentStringAsync()) + "end")))
                    .SetDefaultLayoutTemplate("foobar");
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/Test.html",
                        @"<p>This is a test</p>"
                    },
                    {
                        "/input/_Layout.cshtml",
                        @"<html><head></head><body>
    <div>LAYOUT</div>
    @RenderBody()
</body></html>"
                    },
                    {
                        "/input/_ViewStart.cshtml",
                        @"@{
    Layout = ""_Layout"";
}"
                    },
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.Output].ShouldHaveSingleItem();
                (await document.GetContentStringAsync()).ShouldBe(
                    @"start<p>This is a test</p>end",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task LayoutMetadata()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper
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
            public async Task RazorCommentFrontMatter()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/Layout/Test.cshtml",
                        @"@*---
Layout: _Layout.cshtml
---*@
<h1>Heading</h1>
<p>This is a test</p>"
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
    <h1>Heading</h1>
<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task HtmlCommentFrontMatter()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/Layout/Test.cshtml",
                        @"<!---
Layout: _Layout.cshtml
--->
<h1>Heading</h1>
<p>This is a test</p>"
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
    <h1>Heading</h1>
<p>This is a test</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ScriptWithMarkdownExtensionReturnsMarkdownContent()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
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
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/foo.csx",
                        @"/*-
MediaType: text/markdown
Script: true
-*/
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
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
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

            [Test]
            public async Task ShouldNotHighlightCodeByDefault()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/foo.html",
                        @"<pre><code>int foo = 1;</code></pre>"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.PostProcess].ShouldHaveSingleItem();
                document.Destination.ShouldBe("foo.html");
                document.GetContentStringAsync().Result.ShouldBe(
                    "<pre><code>int foo = 1;</code></pre>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldHighlightCode()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(WebKeys.HighlightCode, true);
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/foo.html",
                        @"<pre><code>int foo = 1;</code></pre>"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.PostProcess].ShouldHaveSingleItem();
                document.Destination.ShouldBe("foo.html");
                document.GetContentStringAsync().Result.ShouldBe(
                    "<html><head></head><body><pre><code class=\"language-ebnf hljs\"><span class=\"hljs-attribute\">int foo</span> = 1;</code></pre></body></html>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldNotHighlightCodeForUnspecifiedLanguage()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(WebKeys.HighlightCode, true)
                    .AddSetting(WebKeys.HighlightUnspecifiedLanguage, false);
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/foo.html",
                        @"<pre><code>int foo = 1;</code></pre>"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.PostProcess].ShouldHaveSingleItem();
                document.Destination.ShouldBe("foo.html");
                document.GetContentStringAsync().Result.ShouldBe(
                    "<pre><code>int foo = 1;</code></pre>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldChangeMarkdownExtensionsViaTemplates()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .ModifyTemplate(
                        MediaTypes.Markdown,
                        x => ((RenderMarkdown)x).UseExtension<BootstrapExtension>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/foo.md",
                        @"# Hi!

A simple | table
-- | --
with multiple | lines

End"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.PostProcess].ShouldHaveSingleItem();
                document.Destination.ShouldBe("foo.html");
                document.GetContentStringAsync().Result.ShouldBe(
                    @"<h1 id=""hi"">Hi!</h1>
<table class=""table"">
<thead>
<tr>
<th>A simple</th>
<th>table</th>
</tr>
</thead>
<tbody>
<tr>
<td>with multiple</td>
<td>lines</td>
</tr>
</tbody>
</table>
<p>End</p>
",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldChangeMarkdownExtensionsViaSettings()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(MarkdownKeys.MarkdownExtensions, "Bootstrap");
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/foo.md",
                        @"# Hi!

A simple | table
-- | --
with multiple | lines

End"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.PostProcess].ShouldHaveSingleItem();
                document.Destination.ShouldBe("foo.html");
                document.GetContentStringAsync().Result.ShouldBe(
                    @"<h1 id=""hi"">Hi!</h1>
<table class=""table"">
<thead>
<tr>
<th>A simple</th>
<th>table</th>
</tr>
</thead>
<tbody>
<tr>
<td>with multiple</td>
<td>lines</td>
</tr>
</tbody>
</table>
<p>End</p>
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