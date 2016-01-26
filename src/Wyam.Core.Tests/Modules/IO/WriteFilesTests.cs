using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Pipelines;
using Wyam.Core.Documents;
using Wyam.Core.Modules.IO;
using Wyam.Core.Pipelines;
using Wyam.Testing;
using ExecutionContext = Wyam.Core.Pipelines.ExecutionContext;

namespace Wyam.Core.Tests.Modules.IO
{
    [TestFixture]
    public class WriteFilesTests : BaseFixture
    {
        [SetUp]
        public void SetUp()
        {
            if (Directory.Exists(@"TestFiles\Output\"))
            {
                int c = 0;
                while (true)
                {
                    try
                    {
                        Directory.Delete(@"TestFiles\Output\", true);
                        break;
                    }
                    catch (System.IO.IOException)
                    {
                        Thread.Sleep(1000);
                        if (c++ < 4)
                        {
                            continue;
                        }
                        throw;
                    }
                }
            }
        }

        public class ExecuteMethodTests : WriteFilesTests
        {
            [Test]
            public void ExtensionWithDotWritesFile()
            {
                // Given
                Engine engine = new Engine();
                engine.OutputFolder = @"TestFiles\Output\";
                engine.InitialMetadata[Keys.RelativeFilePath] = @"Subfolder/write-test.abc";
                Pipeline pipeline = new Pipeline("Pipeline", null);
                IDocument[] inputs = { new Document(engine.InitialMetadata, pipeline).Clone("Test") };
                IExecutionContext context = new ExecutionContext(engine, pipeline);
                WriteFiles writeFiles = new WriteFiles(".txt");

                // When
                IEnumerable<IDocument> outputs = writeFiles.Execute(inputs, context).ToList();
                foreach (IDocument document in inputs.Concat(outputs))
                {
                    ((IDisposable)document).Dispose();
                }

                // Then
                Assert.IsTrue(File.Exists(@"TestFiles\Output\Subfolder\write-test.txt"));
                Assert.AreEqual("Test", File.ReadAllText(@"TestFiles\Output\Subfolder\write-test.txt"));
            }

            [Test]
            public void ExtensionWithoutDotWritesFile()
            {
                // Given
                Engine engine = new Engine();
                engine.OutputFolder = @"TestFiles\Output\";
                engine.InitialMetadata[Keys.RelativeFilePath] = @"Subfolder/write-test.abc";
                Pipeline pipeline = new Pipeline("Pipeline", null);
                IDocument[] inputs = { new Document(engine.InitialMetadata, pipeline).Clone("Test") };
                IExecutionContext context = new ExecutionContext(engine, pipeline);
                WriteFiles writeFiles = new WriteFiles("txt");

                // When
                IEnumerable<IDocument> outputs = writeFiles.Execute(inputs, context).ToList();
                foreach (IDocument document in inputs.Concat(outputs))
                {
                    ((IDisposable)document).Dispose();
                }

                // Then
                Assert.IsTrue(File.Exists(@"TestFiles\Output\Subfolder\write-test.txt"));
                Assert.AreEqual("Test", File.ReadAllText(@"TestFiles\Output\Subfolder\write-test.txt"));
            }

            [Test]
            public void ExecuteReturnsSameContent()
            {
                // Given
                Engine engine = new Engine();
                engine.OutputFolder = @"TestFiles\Output\";
                engine.InitialMetadata[Keys.SourceFileRoot] = @"TestFiles/Input";
                engine.InitialMetadata[Keys.SourceFileDir] = @"TestFiles/Input/Subfolder";
                engine.InitialMetadata[Keys.SourceFileBase] = @"write-test";
                Pipeline pipeline = new Pipeline("Pipeline", null);
                IDocument[] inputs = { new Document(engine.InitialMetadata, pipeline).Clone("Test") };
                IExecutionContext context = new ExecutionContext(engine, pipeline);
                WriteFiles writeFiles = new WriteFiles((x, y) => null);

                // When
                IDocument output = writeFiles.Execute(inputs, context).First();

                // Then
                Assert.AreEqual("Test", output.Content);
                ((IDisposable)output).Dispose();
            }

            [TestCase(Keys.DestinationFileBase, @"write-test")]
            [TestCase(Keys.DestinationFileExt, @".txt")]
            [TestCase(Keys.DestinationFileName, @"write-test.txt")]
            [TestCase(Keys.DestinationFileDir, @"TestFiles\Output\Subfolder")]
            [TestCase(Keys.DestinationFilePath, @"TestFiles\Output\Subfolder\write-test.txt")]
            [TestCase(Keys.DestinationFilePathBase, @"TestFiles\Output\Subfolder\write-test")]
            [TestCase(Keys.RelativeFilePath, @"Subfolder\write-test.txt")]
            [TestCase(Keys.RelativeFilePathBase, @"Subfolder\write-test")]
            [TestCase(Keys.RelativeFileDir, @"Subfolder")]
            public void WriteFilesSetsMetadata(string key, string expectedEnding)
            {
                // Given
                Engine engine = new Engine();
                engine.OutputFolder = @"TestFiles\Output\";
                engine.InitialMetadata[Keys.RelativeFilePath] = @"Subfolder/write-test.abc";
                Pipeline pipeline = new Pipeline("Pipeline", null);
                IDocument[] inputs = { new Document(engine.InitialMetadata, pipeline).Clone("Test") };
                IExecutionContext context = new ExecutionContext(engine, pipeline);
                WriteFiles writeFiles = new WriteFiles("txt");

                // When
                IDocument output = writeFiles.Execute(inputs, context).First();
                foreach (IDocument document in inputs)
                {
                    ((IDisposable)document).Dispose();
                }

                // Then
                Assert.That(output.Metadata[key], Does.EndWith(expectedEnding));
                ((IDisposable)output).Dispose();
            }

            [Test]
            public void RelativePathsAreConsistentBeforeAndAfterWriteFiles()
            {
                // Given
                Engine engine = new Engine();
                engine.RootFolder = TestContext.CurrentContext.TestDirectory;
                engine.InputFolder = @"TestFiles\Input";
                engine.OutputFolder = @"TestFiles\Output\";
                Pipeline pipeline = new Pipeline("Pipeline", null);
                IDocument[] inputs = { new Document(engine.InitialMetadata, pipeline) };
                IExecutionContext context = new ExecutionContext(engine, pipeline);
                ReadFiles readFiles = new ReadFiles(@"test-c.txt");
                WriteFiles writeFiles = new WriteFiles("txt");

                // When
                IDocument readFilesDocument = readFiles.Execute(inputs, context).First();
                object readFilesRelativeFilePath = readFilesDocument.Metadata[Keys.RelativeFilePath];
                object readFilesRelativeFilePathBase = readFilesDocument.Metadata[Keys.RelativeFilePathBase];
                object readFilesRelativeFileDir = readFilesDocument.Metadata[Keys.RelativeFileDir];
                IDocument writeFilesDocument = writeFiles.Execute(new [] { readFilesDocument }, context).First();
                object writeFilesRelativeFilePath = writeFilesDocument.Metadata[Keys.RelativeFilePath];
                object writeFilesRelativeFilePathBase = writeFilesDocument.Metadata[Keys.RelativeFilePathBase];
                object writeFilesRelativeFileDir = writeFilesDocument.Metadata[Keys.RelativeFileDir];
                foreach (IDocument document in inputs)
                {
                    ((IDisposable)document).Dispose();
                }

                // Then
                Assert.AreEqual(@"Subfolder\test-c.txt", readFilesRelativeFilePath);
                Assert.AreEqual(@"Subfolder\test-c", readFilesRelativeFilePathBase);
                Assert.AreEqual(@"Subfolder", readFilesRelativeFileDir);
                Assert.AreEqual(@"Subfolder\test-c.txt", writeFilesRelativeFilePath);
                Assert.AreEqual(@"Subfolder\test-c", writeFilesRelativeFilePathBase);
                Assert.AreEqual(@"Subfolder", writeFilesRelativeFileDir);
                ((IDisposable)readFilesDocument).Dispose();
                ((IDisposable)writeFilesDocument).Dispose();
            }


            [Test]
            public void IgnoresEmptyDocuments()
            {
                // Given
                Engine engine = new Engine();
                engine.OutputFolder = @"TestFiles\Output\";
                Pipeline pipeline = new Pipeline("Pipeline", null);
                MemoryStream emptyStream = new MemoryStream(new byte[] { });
                IDocument[] inputs =
                {
                    new Document(engine.InitialMetadata, pipeline).Clone("Test", 
                        new [] {
                            new MetadataItem(Keys.RelativeFilePath, @"Subfolder/write-test")
                        }),
                    new Document(engine.InitialMetadata, pipeline).Clone(string.Empty,
                        new [] {
                            new MetadataItem(Keys.RelativeFilePath, @"Subfolder/empty-test"), 
                        }),
                    new Document(engine.InitialMetadata, pipeline).Clone(null,
                        new [] {
                            new MetadataItem(Keys.RelativeFilePath, @"Subfolder/null-test")
                        }),
                    new Document(engine.InitialMetadata, pipeline).Clone(emptyStream,
                        new [] {
                            new MetadataItem(Keys.RelativeFilePath, @"Subfolder/stream-test")
                        })
                };
                IExecutionContext context = new ExecutionContext(engine, pipeline);
                WriteFiles writeFiles = new WriteFiles();

                // When
                IEnumerable<IDocument> outputs = writeFiles.Execute(inputs, context).ToList();
                foreach (IDocument document in inputs.Concat(outputs))
                {
                    ((IDisposable)document).Dispose();
                }

                // Then
                Assert.AreEqual(4, outputs.Count());
                Assert.IsTrue(File.Exists(@"TestFiles\Output\Subfolder\write-test"));
                Assert.IsFalse(File.Exists(@"TestFiles\Output\Subfolder\empty-test"));
                Assert.IsFalse(File.Exists(@"TestFiles\Output\Subfolder\null-test"));
                Assert.IsFalse(File.Exists(@"TestFiles\Output\Subfolder\stream-test"));
            }
        }
    }
}
