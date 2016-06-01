using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Testing;

namespace Wyam.TextGeneration.Tests
{
    [TestFixture]
    public class GenerateMetaTests : BaseFixture
    {
        public class ExecuteMethodTests : GenerateMetaTests
        {
            [Test]
            public void GeneratingMetadataFromStringTemplateSetsContent()
            {
                // Given
                IDocument document = Substitute.For<IDocument>();
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(string.Empty));
                document.GetStream().Returns(stream);
                IModule generateContent = new GenerateMeta("Foo", @"[rs:4;,\s]{<noun>}").WithSeed(1000);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                object result;
                context.TryConvert(new object(), out result)
                    .ReturnsForAnyArgs(x =>
                    {
                        x[1] = x[0];
                        return true;
                    });

                // When
                generateContent.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
                context.Received().GetDocument(Arg.Is(document), Arg.Is<IEnumerable<KeyValuePair<string, object>>>(
                    x => x.SequenceEqual(new[] { new KeyValuePair<string, object>("Foo", "nectarine, gambler, marijuana, chickadee") })));
                stream.Dispose();
            }
        }
    }
}