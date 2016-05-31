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
using Wyam.Testing.IO;
using NSubstitute;
using System.IO;
using Wyam.Common.Meta;
using System.Text;
using Wyam.Common.Modules;
using System.Collections.ObjectModel;
using System;

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
                IExecutionContext context = GetExecutionContext(engine);

                string documentContent = "File a1";
                string sidecarContent = "data: a1";

                IDocument[] inputs =
                {
                    GetDocument("a/1.md", "File a1")
                };
                string lodedSidecarContent = null;
                Sidecar sidecar = new Sidecar(new Execute((x, ctx) =>
                {
                    lodedSidecarContent = x.Content;
                    return new IDocument[] { x };
                }));

                // When
                IEnumerable<IDocument> documents = sidecar.Execute(inputs, context).ToArray();

                // Then

                Assert.AreEqual(sidecarContent, lodedSidecarContent);
                Assert.AreEqual(documentContent, documents.Single().Content);
            }

            [Test]
            public void LoadsCustomSidecarFile()
            {
                // Given
                Engine engine = new Engine();
                IExecutionContext context = GetExecutionContext(engine);

                string documentContent = "File a1";
                string sidecarContent = "data: other";

                IDocument[] inputs =
                {
                    GetDocument("a/1.md", "File a1")
                };
                string lodedSidecarContent = null;
                Sidecar sidecar = new Sidecar(".other", new Execute((x, ctx) =>
                 {
                     lodedSidecarContent = x.Content;
                     return new IDocument[] { x };
                 }));

                // When
                IEnumerable<IDocument> documents = sidecar.Execute(inputs, context).ToArray();

                // Then

                Assert.AreEqual(sidecarContent, lodedSidecarContent);
                Assert.AreEqual(documentContent, documents.Single().Content);
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
                IExecutionContext context = Substitute.For<IExecutionContext>();
                context.Assemblies.Returns(engine.Assemblies);
                context.Namespaces.Returns(engine.Namespaces);
                IReadOnlyFileSystem fileSystem = GetFileSystem();
                context.FileSystem.Returns(fileSystem);
                FilePath result;

                context.GetDocument(Arg.Any<IDocument>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>()).Returns(x =>
                {
                    IDocument document = (IDocument)x[0];
                    string content = (string)x[1];
                    return GetDocument(document.Source.FullPath, content);
                });

                context.GetDocument(Arg.Any<IDocument>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>()).Returns(x =>
                {
                    IDocument document = (IDocument)x[0];
                    string content = document.Content;
                    return GetDocument(document.Source.FullPath, content);
                });

                context.Execute(Arg.Any<IEnumerable<IModule>>(), Arg.Any<IEnumerable<IDocument>>()).Returns(x =>
                {
                    IModule[] modules = ((IEnumerable<IModule>)x[0]).ToArray();
                    IEnumerable<IDocument> documents = (IEnumerable<IDocument>)x[1];

                    for (int i = 0; i < modules.Length; i++)
                    {
                        documents = modules[i].Execute(new ReadOnlyCollection<IDocument>(documents.ToList()), context);
                    }


                    return new ReadOnlyCollection<IDocument>(documents.ToArray());
                });
                context.TryConvert(Arg.Any<object>(), out result).Returns(x =>
                {
                    x[1] = (FilePath)x[0];
                    return true;
                });
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
