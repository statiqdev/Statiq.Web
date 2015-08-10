using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Core.Modules;
using Wyam.Common;
using Wyam.Core.Documents;
using Wyam.Core.Pipelines;
using ExecutionContext = Wyam.Core.Pipelines.ExecutionContext;

namespace Wyam.Core.Tests.Modules
{
    [TestFixture]
    public class WriteFilesFixture
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

        [Test]
        public void ExtensionWithDotWritesFile()
        {
            // Given
            Engine engine = new Engine();
            engine.OutputFolder = @"TestFiles\Output\";
            engine.Metadata["RelativeFilePath"] = @"Subfolder/write-test.abc";
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(metadata, pipeline).Clone("Test") };
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
            engine.Metadata["RelativeFilePath"] = @"Subfolder/write-test.abc";
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(metadata, pipeline).Clone("Test") };
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
            engine.Metadata["SourceFileRoot"] = @"TestFiles/Input";
            engine.Metadata["SourceFileDir"] = @"TestFiles/Input/Subfolder";
            engine.Metadata["SourceFileBase"] = @"write-test";
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(metadata, pipeline).Clone("Test") };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            WriteFiles writeFiles = new WriteFiles(x => null);

            // When
            IDocument output = writeFiles.Execute(inputs, context).First();

            // Then
            Assert.AreEqual("Test", output.Content);
            ((IDisposable)output).Dispose();
        }

        [TestCase("DestinationFileBase", @"write-test")]
        [TestCase("DestinationFileExt", @".txt")]
        [TestCase("DestinationFileName", @"write-test.txt")]
        [TestCase("DestinationFileDir", @"TestFiles\Output\Subfolder")]
        [TestCase("DestinationFilePath", @"TestFiles\Output\Subfolder\write-test.txt")]
        [TestCase("DestinationFilePathBase", @"TestFiles\Output\Subfolder\write-test")]
        public void WriteFilesSetsMetadata(string key, string expectedEnding)
        {
            // Given
            Engine engine = new Engine();
            engine.OutputFolder = @"TestFiles\Output\";
            engine.Metadata["RelativeFilePath"] = @"Subfolder/write-test.abc";
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(metadata, pipeline).Clone("Test") };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            WriteFiles writeFiles = new WriteFiles("txt");

            // When
            IDocument output = writeFiles.Execute(inputs, context).First();
            foreach (IDocument document in inputs)
            {
                ((IDisposable)document).Dispose();
            }

            // Then
            Assert.That(output.Metadata[key], Is.StringEnding(expectedEnding));
            ((IDisposable)output).Dispose();
        }
    }
}
