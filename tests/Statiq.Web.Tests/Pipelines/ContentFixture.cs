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
                    .GetDocumentList(Keys.Headings)
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
                    .GetDocumentList(Keys.Headings)
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
                    .GetDocumentList(Keys.Headings)
                    .Flatten()
                    .Select(x => x.GetContentStringAsync().Result)
                    .ShouldBe(new[] { "1.1", "1.2", "2.1", "2.2", "2.3" }, true);
            }

            [Test]
            public async Task LayoutViewStart()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper
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
                Bootstrapper bootstrapper = Bootstrapper
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
                Bootstrapper bootstrapper = Bootstrapper
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
                Bootstrapper bootstrapper = Bootstrapper
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
                Bootstrapper bootstrapper = Bootstrapper
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
                Bootstrapper bootstrapper = Bootstrapper
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
                Bootstrapper bootstrapper = Bootstrapper
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
                Bootstrapper bootstrapper = Bootstrapper
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

            [Test]
            public async Task ShouldNotHighlightCodeByDefault()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
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
                Bootstrapper bootstrapper = Bootstrapper.Factory
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
                Bootstrapper bootstrapper = Bootstrapper.Factory
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
                Bootstrapper bootstrapper = Bootstrapper.Factory
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
                Bootstrapper bootstrapper = Bootstrapper.Factory
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