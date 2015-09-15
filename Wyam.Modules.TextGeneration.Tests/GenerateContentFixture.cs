using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common;
using System.IO;
using NSubstitute;

namespace Wyam.Modules.TextGeneration.Tests
{
    [TestFixture]
    public class GenerateContentFixture
    {
        [Test]
        public void GeneratingContentFromStringTemplateSetsContent()
        {
            // Given
            IDocument document = Substitute.For<IDocument>();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(string.Empty));
            document.GetStream().Returns(stream);
            string content = null;
            document
                .When(x => x.Clone(Arg.Any<string>()))
                .Do(x => content = x.Arg<string>());
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
            generateContent.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            document.Received().Clone(Arg.Any<string>());
            Assert.AreEqual("nectarine, gambler, marijuana, chickadee", content);
            stream.Dispose();
        }
    }
}
