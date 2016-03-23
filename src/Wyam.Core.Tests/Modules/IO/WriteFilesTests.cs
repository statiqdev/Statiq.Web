using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Pipelines;
using Wyam.Core.Modules.IO;
using Wyam.Core.Pipelines;
using Wyam.Testing;
using Wyam.Testing.IO;

namespace Wyam.Core.Tests.Modules.IO
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class WriteFilesTests : BaseFixture
    {
        private Engine Engine { get; set; }
        private Pipeline Pipeline { get; set; }
        private IExecutionContext Context { get; set; }

        [SetUp]
        public void SetUp()
        {
            Engine = new Engine();
            Engine.FileSystem.FileProviders.Add(string.Empty, GetFileProvider());
            Engine.FileSystem.RootPath = "/";
            Engine.FileSystem.InputPaths.Clear();
            Engine.FileSystem.InputPaths.Add("/TestFiles/Input");
            Pipeline = new Pipeline("Pipeline", null);
            Context = new ExecutionContext(Engine, Pipeline);
        }

        private IFileProvider GetFileProvider()
        {
            TestFileProvider fileProvider = new TestFileProvider();

            fileProvider.AddDirectory("/");
            fileProvider.AddDirectory("/TestFiles");
            fileProvider.AddDirectory("/TestFiles/Input");
            fileProvider.AddDirectory("/TestFiles/Input/Subfolder");

            fileProvider.AddFile("/TestFiles/test-above-input.txt", "test");
            fileProvider.AddFile("/TestFiles/Input/markdown-x.md", "xxx");
            fileProvider.AddFile("/TestFiles/Input/test-a.txt", "aaa");
            fileProvider.AddFile("/TestFiles/Input/test-b.txt", "bbb");
            fileProvider.AddFile("/TestFiles/Input/Subfolder/markdown-y.md", "yyy");
            fileProvider.AddFile("/TestFiles/Input/Subfolder/test-c.txt", "ccc");

            return fileProvider;
        }

        public class ExecuteMethodTests : WriteFilesTests
        {
            [Test]
            public void ExtensionWithDotWritesFiles()
            {
                // Given
                Engine.InitialMetadata[Keys.RelativeFilePath] = new FilePath("Subfolder/write-test.abc");
                IDocument[] inputs = new[] { Context.GetDocument("Test") };
                WriteFiles writeFiles = new WriteFiles(".txt");

                // When
                writeFiles.Execute(inputs, Context).ToList();

                // Then
                IFile outputFile = Engine.FileSystem.GetOutputFile("Subfolder/write-test.txt");
                Assert.IsTrue(outputFile.Exists);
                Assert.AreEqual("Test", outputFile.ReadAllText());
            }

            [Test]
            public void ExtensionWithoutDotWritesFiles()
            {
                // Given
                Engine.InitialMetadata[Keys.RelativeFilePath] = new FilePath("Subfolder/write-test.abc");
                IDocument[] inputs = new[] { Context.GetDocument("Test") };
                WriteFiles writeFiles = new WriteFiles("txt");

                // When
                writeFiles.Execute(inputs, Context).ToList();

                // Then
                IFile outputFile = Engine.FileSystem.GetOutputFile("Subfolder/write-test.txt");
                Assert.IsTrue(outputFile.Exists);
                Assert.AreEqual("Test", outputFile.ReadAllText());
            }

            [Test]
            public void OutputDocumentContainsSameContent()
            {
                // Given
                IDocument[] inputs = new[] { Context.GetDocument("Test") };
                WriteFiles writeFiles = new WriteFiles((x, y) => null);

                // When
                IDocument output = writeFiles.Execute(inputs, Context).ToList().First();

                // Then
                Assert.AreEqual("Test", output.Content);
            }

            [TestCase(Keys.DestinationFileBase, "write-test")]
            [TestCase(Keys.DestinationFileName, "write-test.txt")]
            [TestCase(Keys.DestinationFilePath, "/output/Subfolder/write-test.txt")]
            [TestCase(Keys.DestinationFilePathBase, "/output/Subfolder/write-test")]
            [TestCase(Keys.RelativeFilePath, "Subfolder/write-test.txt")]
            [TestCase(Keys.RelativeFilePathBase, "Subfolder/write-test")]
            public void ShouldSetFilePathMetadata(string key, string expected)
            {
                // Given
                Engine.InitialMetadata[Keys.RelativeFilePath] = new FilePath("Subfolder/write-test.abc");
                IDocument[] inputs = new[] { Context.GetDocument("Test") };
                WriteFiles writeFiles = new WriteFiles(".txt");

                // When
                IDocument output = writeFiles.Execute(inputs, Context).ToList().First();

                // Then
                object result = output[key];
                Assert.IsInstanceOf<FilePath>(result);
                Assert.AreEqual(expected, ((FilePath)result).FullPath);
            }

            [TestCase(Keys.DestinationFileDir, "/output/Subfolder")]
            [TestCase(Keys.RelativeFileDir, "Subfolder")]
            public void ShouldSetDirectoryPathMetadata(string key, string expected)
            {
                // Given
                Engine.InitialMetadata[Keys.RelativeFilePath] = new FilePath("Subfolder/write-test.abc");
                IDocument[] inputs = new[] { Context.GetDocument("Test") };
                WriteFiles writeFiles = new WriteFiles(".txt");

                // When
                IDocument output = writeFiles.Execute(inputs, Context).ToList().First();

                // Then
                object result = output[key];
                Assert.IsInstanceOf<DirectoryPath>(result);
                Assert.AreEqual(expected, ((DirectoryPath)result).FullPath);
            }

            [TestCase(Keys.DestinationFileExt, ".txt")]
            public void ShouldSetStringMetadata(string key, string expected)
            {
                // Given
                Engine.InitialMetadata[Keys.RelativeFilePath] = new FilePath("Subfolder/write-test.abc");
                IDocument[] inputs = new[] { Context.GetDocument("Test") };
                WriteFiles writeFiles = new WriteFiles(".txt");

                // When
                IDocument output = writeFiles.Execute(inputs, Context).ToList().First();

                // Then
                object result = output[key];
                Assert.IsInstanceOf<string>(result);
                Assert.AreEqual(expected, result);
            }

            [Test]
            public void IgnoresEmptyDocuments()
            {
                // Given
                MemoryStream emptyStream = new MemoryStream(new byte[] { });
                IDocument[] inputs =
                {
                    Context.GetDocument("Test",
                        new MetadataItems {
                            new MetadataItem(Keys.RelativeFilePath, new FilePath("Subfolder/write-test"))
                        }),
                    Context.GetDocument(string.Empty,
                        new MetadataItems {
                            new MetadataItem(Keys.RelativeFilePath, new FilePath("Subfolder/empty-test")),
                        }),
                    Context.GetDocument((string)null,
                        new MetadataItems {
                            new MetadataItem(Keys.RelativeFilePath, new FilePath("Subfolder/null-test"))
                        }),
                    Context.GetDocument(emptyStream,
                        new MetadataItems {
                            new MetadataItem(Keys.RelativeFilePath, new FilePath(@"Subfolder/stream-test"))
                        })
                    };
                WriteFiles writeFiles = new WriteFiles();

                // When
                IEnumerable<IDocument> outputs = writeFiles.Execute(inputs, Context).ToList();

                // Then
                Assert.AreEqual(4, outputs.Count());
                Assert.IsTrue(Context.FileSystem.GetOutputFile("Subfolder/write-test").Exists);
                Assert.IsFalse(Context.FileSystem.GetOutputFile("output/Subfolder/empty-test").Exists);
                Assert.IsFalse(Context.FileSystem.GetOutputFile("output/Subfolder/null-test").Exists);
                Assert.IsFalse(Context.FileSystem.GetOutputFile("output/Subfolder/stream-test").Exists);
            }
        }
    }
}
