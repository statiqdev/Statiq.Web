using System;
using System.Collections.Immutable;
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
    public class DataFixture : BaseFixture
    {
        public class ExecuteTests : DataFixture
        {
            [Test]
            public async Task ParsesJson()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/a/b/c.json", "{ \"Foo\": \"Bar\" }" }
                };

                // When
                ImmutableArray<IDocument> outputs = await bootstrapper.RunTestAsync(nameof(Data), Phase.Process, fileProvider);

                // Then
                IDocument document = outputs.ShouldHaveSingleItem();
                document["Foo"].ShouldBe("Bar");
            }

            [Test]
            public async Task ParsesYaml()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    { "/input/a/b/c.yaml", "Foo: Bar" }
                };

                // When
                ImmutableArray<IDocument> outputs = await bootstrapper.RunTestAsync(nameof(Data), Phase.Process, fileProvider);

                // Then
                IDocument document = outputs.ShouldHaveSingleItem();
                document["Foo"].ShouldBe("Bar");
            }
        }
    }
}
