using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common;

namespace Wyam.Modules.CodeAnalysis.Tests
{
    [TestFixture]
    public class AnalyzeCSharpMethodsFixture : AnalyzeCSharpFixtureBase
    {
        [Test]
        public void ReturnsMethods()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    public class Blue
                    {
                        void Green()
                        {
                        }

                        void Red()
                        {
                        }
                    }
                }
            ";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new[] { "Green", "Red" }, 
                GetClass(results, "Blue").Get<IReadOnlyList<IDocument>>("Members").Select(x => x["Name"]));
            stream.Dispose();
        }

        [Test]
        public void ContainingTypeIsCorrect()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    public class Blue
                    {
                        void Green()
                        {
                        }
                    }
                }
            ";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual("Blue", GetMember(results, "Blue", "Green").Get<IDocument>("ContainingType")["Name"]);
            stream.Dispose();
        }
    }
}
