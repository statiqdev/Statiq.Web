using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Core.Documents;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Execution;
using Wyam.Testing;
using Wyam.Common.IO;

namespace Wyam.Core.Tests.Modules.Control
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class SideCarTests : BaseFixture
    {
        public class ExecuteMethodTests : FrontMatterTests
        {
            [Test]
            public void LoadsSidecarFile()
            {
                // Given
                Engine engine = new Engine();
                Pipeline pipeline = new Pipeline("Pipeline", null);
                IExecutionContext context = new ExecutionContext(engine, pipeline);

                string documentContent = "I'm document content";
                string sidecarContent = "I'm sidecar content";

                string documentPath = "/test.md";
                string sidecarPath = "/test.md.meta";


                IDocument[] inputs =
                {
                    context.GetDocument((FilePath)documentPath, "")
                };
                string lodedSidecarContent = null;
                Sidecar sidecar = new Sidecar(new Execute((x, ctx) =>
                {
                    lodedSidecarContent = x.Content;
                    return new[] { x };
                }));

                // When
                IEnumerable<IDocument> documents = sidecar.Execute(inputs, context);

                // Then

                Assert.AreEqual(sidecarContent, lodedSidecarContent);
                Assert.AreEqual(documentContent, documents.Single().Content);


            }

        }
    }
}
