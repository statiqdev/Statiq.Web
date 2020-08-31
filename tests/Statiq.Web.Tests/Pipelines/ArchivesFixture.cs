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
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
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
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
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
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
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
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
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
        }
    }
}
