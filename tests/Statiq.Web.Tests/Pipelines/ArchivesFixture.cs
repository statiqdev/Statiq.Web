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
    public class ArchivesFixture : BaseFixture
    {
        public class ExecuteTests : ArchivesFixture
        {
            [Test]
            public async Task ScriptArchive()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/1.json", "{ \"A\": \"a1\", \"B\": \"b1\" }" },
                    { "/input/2.json", "{ \"A\": \"a2\", \"B\": \"b2\" }" },
                    {
                        "/input/archive.csx",
                        @"
ArchivePipelines: Data
DestinationPath: archive.json
ArchiveOrderKey: A
---
return Document.GetChildren().Select(x => x.FilterMetadata(""A"", ""B"")).ToJson();
"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Archives)][Phase.Output].ShouldHaveSingleItem();
                document.Destination.ShouldBe("archive.json");
                (await document.GetContentStringAsync()).ShouldBe(@"[{""A"":""a1"",""B"":""b1""},{""A"":""a2"",""B"":""b2""}]");
            }

            [Test]
            public async Task ScriptArchiveWithExtensionPrefix()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/1.json", "{ \"A\": \"a1\", \"B\": \"b1\" }" },
                    { "/input/2.json", "{ \"A\": \"a2\", \"B\": \"b2\" }" },
                    {
                        "/input/archive.json.csx",
                        @"
ArchivePipelines: Data
ArchiveOrderKey: A
---
return Document.GetChildren().Select(x => x.FilterMetadata(""A"", ""B"")).ToJson();
"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Archives)][Phase.Output].ShouldHaveSingleItem();
                document.Destination.ShouldBe("archive.json");
                (await document.GetContentStringAsync()).ShouldBe(@"[{""A"":""a1"",""B"":""b1""},{""A"":""a2"",""B"":""b2""}]");
            }

            [Test]
            public async Task GeneratesArchiveWithChildren()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/1.cshtml", "{ \"A\": \"a1\", \"B\": \"b1\" }" },
                    { "/input/2.cshtml", "{ \"A\": \"a2\", \"B\": \"b2\" }" },
                    {
                        "/input/archive.cshtml",
                        @"ArchiveSources: ""**/*""
---
<p>@Document.GetChildren().Count</p>
"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Archives)][Phase.Output].ShouldHaveSingleItem();
                document.Destination.ShouldBe("archive.html");
                (await document.GetContentStringAsync()).ShouldBe(@"<p>2</p>
");
            }

            [Test]
            public async Task ExcludesAssetContentType()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/1.cshtml", "{ \"A\": \"a1\", \"B\": \"b1\" }" },
                    { "/input/2.cshtml", "{ \"A\": \"a2\", \"B\": \"b2\" }" },
                    {
                        "/input/archive.cshtml",
                        @"ArchiveSources: ""**/*""
ContentType: Asset
---
<p>@Document.GetChildren().Count</p>
"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Outputs[nameof(Archives)][Phase.Output].ShouldBeEmpty();
            }

            [Test]
            public async Task KeyedOrder()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/1.cshtml",
                        @"A: a2
B: b1
---
Foo"
                    },
                    {
                        "/input/2.cshtml",
                        @"A: a1
B: b2
---
Bar"
                    },
                    {
                        "/input/archive.cshtml",
                        @"ArchiveSources: ""**/*""
ArchiveOrderKey: A
---
@string.Join("","", Document.GetChildren().Select(x => x.GetContentStringAsync().Result))"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Archives)][Phase.Output].ShouldHaveSingleItem();
                document.Destination.ShouldBe("archive.html");
                (await document.GetContentStringAsync()).ShouldBe(@"Bar,Foo");
            }

            [Test]
            public async Task ComputedOrder()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/1.cshtml",
                        @"A: a2
B: b1
---
Foo"
                    },
                    {
                        "/input/2.cshtml",
                        @"A: a1
B: b2
---
Bar"
                    },
                    {
                        "/input/archive.cshtml",
                        @"ArchiveSources: ""**/*""
ArchiveOrder: => int.Parse(GetString(""A"").Substring(1))
---
@string.Join("","", Document.GetChildren().Select(x => x.GetContentStringAsync().Result))"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Archives)][Phase.Output].ShouldHaveSingleItem();
                document.Destination.ShouldBe("archive.html");
                (await document.GetContentStringAsync()).ShouldBe(@"Bar,Foo");
            }

            [Test]
            public async Task ComputedOrderAfterKeyedOrder()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/1.cshtml",
                        @"A: a2
B: b1
---
Foo"
                    },
                    {
                        "/input/2.cshtml",
                        @"A: a1
B: b2
---
Bar"
                    },
                    {
                        "/input/archive.cshtml",
                        @"ArchiveSources: ""**/*""
ArchiveOrderKey: A
ArchiveOrder: => int.Parse(GetString(""B"").Substring(1))
---
@string.Join("","", Document.GetChildren().Select(x => x.GetContentStringAsync().Result))"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Archives)][Phase.Output].ShouldHaveSingleItem();
                document.Destination.ShouldBe("archive.html");
                (await document.GetContentStringAsync()).ShouldBe(@"Foo,Bar");
            }

            [Test]
            public async Task ArchiveKey()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/1.cshtml",
                        @"A: a1
G: a1
---
Foo"
                    },
                    {
                        "/input/2.cshtml",
                        @"A: a2
G: a2
---
Bar"
                    },
                    {
                        "/input/3.cshtml",
                        @"A: a3
G: a1
---
Bizz"
                    },
                    {
                        "/input/4.cshtml",
                        @"A: a4
G: b1
---
Buzz"
                    },
                    {
                        "/input/5.cshtml",
                        @"A: a5
G: b1
---
Bazz"
                    },
                    {
                        "/input/archive.cshtml",
                        @"ArchiveSources: ""**/*""
ArchiveKey: G
---
@if(!Document.ContainsKey(""GroupKey""))
{
    @foreach(IDocument doc in Document.GetChildren().OrderBy(x => x.GetString(""GroupKey"")))
    {
      <div>@doc.GetString(""GroupKey"")</div>
      <div>@string.Join("","", doc.GetChildren().Select(x => x.GetContentStringAsync().Result).OrderBy(x => x))</div>
    }
}"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Outputs[nameof(Archives)][Phase.Output].Select(x => x.Destination.FullPath).ShouldBe(
                    new[]
                    {
                        "archive.html",
                        "archive/a1.html",
                        "archive/a2.html",
                        "archive/b1.html"
                    },
                    true);
                IDocument document = result.Outputs[nameof(Archives)][Phase.Output].Single(x => x.Destination.FileNameWithoutExtension.FullPath == "archive");
                document.Destination.ShouldBe("archive.html");
                (await document.GetContentStringAsync()).Replace(" ", string.Empty).ShouldBe(@"<div>a1</div>
<div>Bizz,Foo</div>
<div>a2</div>
<div>Bar</div>
<div>b1</div>
<div>Bazz,Buzz</div>
");
            }

            [Test]
            public async Task ComputedArchiveKey()
            {
                // Given
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/1.cshtml",
                        @"A: a1
G: a1
---
Foo"
                    },
                    {
                        "/input/2.cshtml",
                        @"A: a2
G: a2
---
Bar"
                    },
                    {
                        "/input/3.cshtml",
                        @"A: a3
G: a1
---
Bizz"
                    },
                    {
                        "/input/4.cshtml",
                        @"A: a4
G: b1
---
Buzz"
                    },
                    {
                        "/input/5.cshtml",
                        @"A: a5
G: b1
---
Bazz"
                    },
                    {
                        "/input/archive.cshtml",
                        @"ArchiveSources: ""**/*""
ArchiveKey: => GetString(""G"").Substring(1)
---
@if(!Document.ContainsKey(""GroupKey""))
{
    @foreach(IDocument doc in Document.GetChildren().OrderBy(x => x.GetString(""GroupKey"")))
    {
      <div>@doc.GetString(""GroupKey"")</div>
      <div>@string.Join("","", doc.GetChildren().Select(x => x.GetContentStringAsync().Result).OrderBy(x => x))</div>
    }
}"
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Outputs[nameof(Archives)][Phase.Output].Select(x => x.Destination.FullPath).ShouldBe(
                    new[]
                    {
                        "archive.html",
                        "archive/1.html",
                        "archive/2.html"
                    },
                    true);
                IDocument document = result.Outputs[nameof(Archives)][Phase.Output].Single(x => x.Destination.FileNameWithoutExtension.FullPath == "archive");
                document.Destination.ShouldBe("archive.html");
                (await document.GetContentStringAsync()).Replace(" ", string.Empty).ShouldBe(@"<div>1</div>
<div>Bazz,Bizz,Buzz,Foo</div>
<div>2</div>
<div>Bar</div>
");
            }
        }
    }
}