using System.IO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core.Modules;
using Wyam.Common;
using Wyam.Core.Documents;
using Wyam.Core.Pipelines;

namespace Wyam.Core.Tests.Modules
{
    [TestFixture]
    public class ReadFilesFixture
    {        
        [Test]
        public void ThrowsOnNullPathFunction()
        {
            // Given

            // When

            // Then
            Assert.Throws<ArgumentNullException>(() => new ReadFiles((DocumentConfig)null));
        }

        [Test]
        public void ThrowsOnNullExtension()
        {
            // Given

            // When

            // Then
            Assert.Throws<ArgumentNullException>(() => new ReadFiles((string)null));
        }

        [TestCase("*.foo", SearchOption.TopDirectoryOnly, 0)]
        [TestCase("*.foo", SearchOption.AllDirectories, 0)]
        [TestCase("*.txt", SearchOption.TopDirectoryOnly, 2)]
        [TestCase("*.txt", SearchOption.AllDirectories, 3)]
        [TestCase("*.md", SearchOption.TopDirectoryOnly, 1)]
        [TestCase("*.md", SearchOption.AllDirectories, 2)]
        [TestCase("*.*", SearchOption.TopDirectoryOnly, 3)]
        [TestCase("*.*", SearchOption.AllDirectories, 5)]
        public void SearchPatternFindsCorrectFiles(string searchPattern, SearchOption searchOption, int expectedCount)
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.InputFolder = @"TestFiles\Input\";
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(metadata, pipeline) };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            ReadFiles readFiles = new ReadFiles(searchPattern).SearchOption(searchOption);

            // When
            IEnumerable<IDocument> documents = readFiles.Execute(inputs, context);
            int count = documents.Count();
            
            // Then
            Assert.AreEqual(expectedCount, count);
        }

        [Test]
        public void SearchPatternWorksWithoutInputPathTrailingSlash()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.InputFolder = @"TestFiles\Input";
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(metadata, pipeline) };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            ReadFiles readFiles = new ReadFiles("*.txt");

            // When
            IEnumerable<IDocument> documents = readFiles.Execute(inputs, context);
            int count = documents.Count();
            
            // Then
            Assert.AreEqual(3, count);
        }

        [Test]
        public void SearchPatternWorksWithInputPathTrailingSlash()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.InputFolder = @"TestFiles\Input";
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(metadata, pipeline) };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            ReadFiles readFiles = new ReadFiles("*.txt");

            // When
            IEnumerable<IDocument> documents = readFiles.Execute(inputs, context);
            int count = documents.Count();
            
            // Then
            Assert.AreEqual(3, count);
        }

        [Test]
        public void SearchPatternWorksWithSubpath()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.InputFolder = @"TestFiles\Input";
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(metadata, pipeline) };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            ReadFiles readFiles = new ReadFiles(@"Subfolder\*.txt");

            // When
            IEnumerable<IDocument> documents = readFiles.Execute(inputs, context);
            int count = documents.Count();
            
            // Then
            Assert.AreEqual(1, count);
        }

        [Test]
        public void SearchPatternWorksWithSingleFile()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.InputFolder = @"TestFiles\Input";
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(metadata, pipeline) };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            ReadFiles readFiles = new ReadFiles(@"test-a.txt");

            // When
            IEnumerable<IDocument> documents = readFiles.Execute(inputs, context);
            int count = documents.Count();
            
            // Then
            Assert.AreEqual(1, count);
        }

        [Test]
        public void ExecuteGetsCorrectContent()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.InputFolder = @"TestFiles\Input";
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(metadata, pipeline) };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            ReadFiles readFiles = new ReadFiles(@"test-a.txt");

            // When
            IDocument document = readFiles.Execute(inputs, context).First();
            
            // Then
            Assert.AreEqual("aaa", document.Content);
        }

        [TestCase("SourceFileRoot", @"TestFiles\Input")]
        [TestCase("SourceFileBase", @"test-c")]
        [TestCase("SourceFileExt", @".txt")]
        [TestCase("SourceFileName", @"test-c.txt")]
        [TestCase("SourceFileDir", @"TestFiles\Input\Subfolder")]
        [TestCase("SourceFilePath", @"TestFiles\Input\Subfolder\test-c.txt")]
        [TestCase("SourceFilePathBase", @"TestFiles\Input\Subfolder\test-c")]
        [TestCase("RelativeFilePath", @"Subfolder\test-c.txt")]
        [TestCase("RelativeFilePathBase", @"Subfolder\test-c")]
        [TestCase("RelativeFileDir", @"Subfolder")]
        public void ReadFilesSetsMetadata(string key, string expectedEnding)
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.InputFolder = @"TestFiles\Input";
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(metadata, pipeline) };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            ReadFiles readFiles = new ReadFiles(@"test-c.txt");

            // When
            IDocument document = readFiles.Execute(inputs, context).First();

            // Then
            Assert.That(document.Metadata[key], Is.StringEnding(expectedEnding));
        }

        [Test]
        public void WithExtensionsGetsCorrectFiles()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.InputFolder = @"TestFiles\Input";
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(metadata, pipeline) };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            ReadFiles readFiles = new ReadFiles("*.*").WithExtensions(".txt");

            // When
            IEnumerable<IDocument> documents = readFiles.Execute(inputs, context);
            int count = documents.Count();

            // Then
            Assert.AreEqual(3, count);
        }

        [Test]
        public void WithExtensionsWorksWithoutDotPrefix()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.InputFolder = @"TestFiles\Input";
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(metadata, pipeline) };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            ReadFiles readFiles = new ReadFiles("*.*").WithExtensions("txt", "md");

            // When
            IEnumerable<IDocument> documents = readFiles.Execute(inputs, context);
            int count = documents.Count();

            // Then
            Assert.AreEqual(5, count);
        }

        [Test]
        public void WhereGetsCorrectFiles()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.InputFolder = @"TestFiles\Input";
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(metadata, pipeline) };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            ReadFiles readFiles = new ReadFiles("*.*").Where(x => x.Contains("test"));

            // When
            IEnumerable<IDocument> documents = readFiles.Execute(inputs, context);
            int count = documents.Count();

            // Then
            Assert.AreEqual(3, count);
        }

        [Test]
        public void WhereAndWithExtensionsGetsCorrectFiles()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.InputFolder = @"TestFiles\Input";
            Metadata metadata = new Metadata(engine);
            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(metadata, pipeline) };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            ReadFiles readFiles = new ReadFiles("*.*").Where(x => x.Contains("-c")).WithExtensions("txt");

            // When
            IEnumerable<IDocument> documents = readFiles.Execute(inputs, context);
            int count = documents.Count();

            // Then
            Assert.AreEqual(1, count);
        }
    }
}
