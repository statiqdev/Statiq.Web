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
using NSubstitute;
using Wyam.Common.Meta;
using Wyam.Testing.Execution;

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
                IDocument document = Substitute.For<IDocument>();
                document.Source.Returns(new FilePath("/" + source));

                document.ContainsKey(Keys.RelativeFilePath).Returns(true);
                document.String(Keys.RelativeFilePath).Returns(source);

                document.ContainsKey(Keys.SourceFilePath).Returns(true);
                document.String(Keys.SourceFilePath).Returns("/" + source);
                document.FilePath(Keys.SourceFilePath).Returns(new FilePath("/" + source));

                document.ContainsKey(Keys.SourceFileName).Returns(true);
                document.FilePath(Keys.SourceFileName).Returns(new FilePath(source).FileName);

                document.Content.Returns(content);
                document.GetStream().Returns(
                    new MemoryStream(Encoding.UTF8.GetBytes(content)),
                    new MemoryStream(Encoding.UTF8.GetBytes(content)));  // Return a new memory stream if called again
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
                IReadOnlyFileSystem fileSystem = Substitute.For<IReadOnlyFileSystem>();
                IFileProvider fileProvider = GetFileProvider();
                fileSystem.GetInputFile(Arg.Any<FilePath>()).Returns(x =>
                {
                    FilePath path = x.ArgAt<FilePath>(0);
                    if (!path.IsAbsolute)
                    {
                        path = new FilePath("/" + path.FullPath);
                    }
                    return fileProvider.GetFile(path);
                });
                fileSystem.GetInputDirectory(Arg.Any<DirectoryPath>()).Returns(x => fileProvider.GetDirectory(x.ArgAt<DirectoryPath>(0)));
                return fileSystem;
            }

            private IFileProvider GetFileProvider()
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
