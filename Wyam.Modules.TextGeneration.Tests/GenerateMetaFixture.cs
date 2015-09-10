using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common;

namespace Wyam.Modules.TextGeneration.Tests
{
    [TestFixture]
    public class GenerateMetaFixture
    {
        [Test]
        public void GeneratingMetadataFromStringTemplateSetsContent()
        {
            // Given
            IDocument document = Substitute.For<IDocument>();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(string.Empty));
            document.GetStream().Returns(stream);
            IEnumerable<KeyValuePair<string, object>> metadata = null;
            document
                .When(x => x.Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>()))
                .Do(x => metadata = x.Arg<IEnumerable<KeyValuePair<string, object>>>());
            IModule generateContent = new GenerateMeta("Foo", @"[rs:4;,\s]{<noun>}").SetSeed(1000);
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
            document.Received().Clone(Arg.Any<IEnumerable<KeyValuePair<string, object>>>());
            CollectionAssert.AreEqual(new[] { new KeyValuePair<string, object>("Foo", "nectarine, gambler, marijuana, chickadee") }, metadata);
            stream.Dispose();
        }

    }
}
