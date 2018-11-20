using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Execution;
using Wyam.Testing;
using Wyam.Common.IO;
using Wyam.Testing.IO;
using Wyam.Common.Meta;
using Wyam.Testing.Execution;
using Wyam.Testing.Documents;

namespace Wyam.Core.Tests.Modules.Control
{
    [TestFixture]
    [NonParallelizable]
    public class SidecarFixture : BaseFixture
    {
        public class ExecuteTests : SidecarFixture
        {
            [Test]
            public void LoadsSidecarFile()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument[] inputs =
                {
                    GetDocument("a/1.md", "File a1")
                };
                string lodedSidecarContent = null;
                Sidecar sidecar = new Sidecar(new Execute((x, ctx) =>
                {
                    lodedSidecarContent = x.Content;
                    return new[] { x };
                }));

                // When
                IEnumerable<IDocument> documents = sidecar.Execute(inputs, context).ToArray();

                // Then
                Assert.AreEqual("data: a1", lodedSidecarContent);
                Assert.AreEqual("File a1", documents.Single().Content);
            }

            [Test]
            public void LoadsCustomSidecarFile()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument[] inputs =
                {
                    GetDocument("a/1.md", "File a1")
                };
                string lodedSidecarContent = null;
                Sidecar sidecar = new Sidecar(".other", new Execute((x, ctx) =>
                 {
                     lodedSidecarContent = x.Content;
                     return new[] { x };
                 }));

                // When
                IEnumerable<IDocument> documents = sidecar.Execute(inputs, context).ToArray();

                // Then
                Assert.AreEqual("data: other", lodedSidecarContent);
                Assert.AreEqual("File a1", documents.Single().Content);
            }

            [Test]
            public void ReturnsOriginalDocumentForMissingSidecarFile()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);
                IDocument[] inputs =
                {
                    GetDocument("a/1.md", "File a1")
                };
                bool executedSidecarModules = false;
                Sidecar sidecar = new Sidecar(".foo", new Execute((x, ctx) =>
                {
                    executedSidecarModules = true;
                    return new[] { x };
                }));

                // When
                IEnumerable<IDocument> documents = sidecar.Execute(inputs, context).ToArray();

                // Then
                Assert.IsFalse(executedSidecarModules);
                Assert.AreEqual(inputs.First(), documents.First());
            }

            private IDocument GetDocument(string source, string content)
            {
                IDocument document = new TestDocument(
                    content,
                    new Dictionary<string, object>
                    {
                        { Keys.RelativeFilePath, source },
                        { Keys.SourceFilePath, new FilePath("/" + source) },
                        { Keys.SourceFileName, new FilePath(source).FileName }
                    })
                    {
                        Source = new FilePath("/" + source)
                    };
                return document;
            }

            private IExecutionContext GetExecutionContext(Engine engine)
            {
                TestExecutionContext context = new TestExecutionContext
                {
                    Namespaces = engine.Namespaces,
                    FileSystem = GetFileSystem()
                };
                return context;
            }

            private IReadOnlyFileSystem GetFileSystem()
            {
                TestFileProvider fileProvider = GetFileProvider();
                TestFileSystem fileSystem = new TestFileSystem
                {
                    InputPaths = new PathCollection<DirectoryPath>(new[]
                    {
                        new DirectoryPath("/")
                    }),
                    FileProvider = fileProvider
                };
                return fileSystem;
            }

            private TestFileProvider GetFileProvider()
            {
                TestFileProvider fileProvider = new TestFileProvider();

                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/a");
                fileProvider.AddDirectory("/b");

                fileProvider.AddFile("/a/1.md", @"File a1");
                fileProvider.AddFile("/a/1.md.meta", @"data: a1");
                fileProvider.AddFile("/a/1.md.other", @"data: other");
                fileProvider.AddFile("/a/2.md", @"File a2");
                fileProvider.AddFile("/a/2.md.meta", @"data: a2");

                fileProvider.AddFile("/b/1.md", @"File b1");
                fileProvider.AddFile("/b/1.md.meta", @"data: b1");

                return fileProvider;
            }
        }
    }
}
