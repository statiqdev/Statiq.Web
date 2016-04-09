using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common;
using System.IO;
using NSubstitute;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Testing;

namespace Wyam.Modules.TextGeneration.Tests
{
    [TestFixture]
    public class GenerateContentTests : BaseFixture
    {
        public class ExecuteMethodTests : GenerateContentTests
        {
            [Test]
            public void GeneratingContentFromStringTemplateSetsContent()
            {
                // Given
                IDocument document = Substitute.For<IDocument>();
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(string.Empty));
                document.GetStream().Returns(stream);
                IModule generateContent = new GenerateContent(@"[rs:4;,\s]{<noun>}").WithSeed(1000);
                IExecutionContext context = Substitute.For<IExecutionContext>();
                object result;
                context.TryConvert(new object(), out result)
                    .ReturnsForAnyArgs(x =>
                    {
                        x[1] = x[0];
                        return true;
                    });

                // When
                generateContent.Execute(new[] {document}, context).ToList(); // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(Arg.Is(document), "nectarine, gambler, marijuana, chickadee");
                stream.Dispose();
            }
        }
    }
}
