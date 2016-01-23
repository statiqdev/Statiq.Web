using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Pipelines;
using Wyam.Core.Documents;
using Wyam.Core.Modules.IO;
using Wyam.Core.Pipelines;
using Wyam.Testing;

namespace Wyam.Core.Tests.Modules.IO
{
    [TestFixture]
    public class ReadFilesFixture : TraceListenerFixture
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
            engine.RootFolder = TestContext.CurrentContext.TestDirectory;
            engine.InputFolder = @"TestFiles\Input\";
            Pipeline pipeline = new Pipeline("Pipeline", null);
            IDocument[] inputs = { new Document(engine.InitialMetadata, pipeline) };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            ReadFiles readFiles = new ReadFiles(searchPattern).WithSearchOption(searchOption);

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
            engine.RootFolder = TestContext.CurrentContext.TestDirectory;
            engine.InputFolder = @"TestFiles\Input";
            Pipeline pipeline = new Pipeline("Pipeline", null);
            IDocument[] inputs = { new Document(engine.InitialMetadata, pipeline) };
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
            engine.RootFolder = TestContext.CurrentContext.TestDirectory;
            engine.InputFolder = @"TestFiles\Input";
            Pipeline pipeline = new Pipeline("Pipeline", null);
            IDocument[] inputs = { new Document(engine.InitialMetadata, pipeline) };
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
            engine.RootFolder = TestContext.CurrentContext.TestDirectory;
            engine.InputFolder = @"TestFiles\Input";
            Pipeline pipeline = new Pipeline("Pipeline", null);
            IDocument[] inputs = { new Document(engine.InitialMetadata, pipeline) };
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
            engine.RootFolder = TestContext.CurrentContext.TestDirectory;
            engine.InputFolder = @"TestFiles\Input";
            Pipeline pipeline = new Pipeline("Pipeline", null);
            IDocument[] inputs = { new Document(engine.InitialMetadata, pipeline) };
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
            engine.RootFolder = TestContext.CurrentContext.TestDirectory;
            engine.InputFolder = @"TestFiles\Input";
            Pipeline pipeline = new Pipeline("Pipeline", null);
            IDocument[] inputs = { new Document(engine.InitialMetadata, pipeline) };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            ReadFiles readFiles = new ReadFiles(@"test-a.txt");

            // When
            IDocument document = readFiles.Execute(inputs, context).First();
            
            // Then
            Assert.AreEqual("aaa", document.Content);
        }

        [TestCase(Keys.SourceFileRoot, @"TestFiles\Input")]
        [TestCase(Keys.SourceFileBase, @"test-c")]
        [TestCase(Keys.SourceFileExt, @".txt")]
        [TestCase(Keys.SourceFileName, @"test-c.txt")]
        [TestCase(Keys.SourceFileDir, @"TestFiles\Input\Subfolder")]
        [TestCase(Keys.SourceFilePath, @"TestFiles\Input\Subfolder\test-c.txt")]
        [TestCase(Keys.SourceFilePathBase, @"TestFiles\Input\Subfolder\test-c")]
        [TestCase(Keys.RelativeFilePath, @"Subfolder\test-c.txt")]
        [TestCase(Keys.RelativeFilePathBase, @"Subfolder\test-c")]
        [TestCase(Keys.RelativeFileDir, @"Subfolder")]
        public void ReadFilesSetsMetadata(string key, string expectedEnding)
        {
            // Given
            Engine engine = new Engine();
            engine.RootFolder = TestContext.CurrentContext.TestDirectory;
            engine.InputFolder = @"TestFiles\Input";
            Pipeline pipeline = new Pipeline("Pipeline", null);
            IDocument[] inputs = { new Document(engine.InitialMetadata, pipeline) };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            ReadFiles readFiles = new ReadFiles(@"test-c.txt");

            // When
            IDocument output = readFiles.Execute(inputs, context).First();
            foreach (IDocument document in inputs)
            {
                ((IDisposable)document).Dispose();
            }

            // Then
            Assert.That(output.Metadata[key], Does.EndWith(expectedEnding));
            ((IDisposable)output).Dispose();
        }

        [Test]
        public void WithExtensionsGetsCorrectFiles()
        {
            // Given
            Engine engine = new Engine();
            engine.RootFolder = TestContext.CurrentContext.TestDirectory;
            engine.InputFolder = @"TestFiles\Input";
            Pipeline pipeline = new Pipeline("Pipeline", null);
            IDocument[] inputs = { new Document(engine.InitialMetadata, pipeline) };
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
            engine.RootFolder = TestContext.CurrentContext.TestDirectory;
            engine.InputFolder = @"TestFiles\Input";
            Pipeline pipeline = new Pipeline("Pipeline", null);
            IDocument[] inputs = { new Document(engine.InitialMetadata, pipeline) };
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
            engine.RootFolder = TestContext.CurrentContext.TestDirectory;
            engine.InputFolder = @"TestFiles\Input";
            Pipeline pipeline = new Pipeline("Pipeline", null);
            IDocument[] inputs = { new Document(engine.InitialMetadata, pipeline) };
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
            engine.RootFolder = TestContext.CurrentContext.TestDirectory;
            engine.InputFolder = @"TestFiles\Input";
            Pipeline pipeline = new Pipeline("Pipeline", null);
            IDocument[] inputs = { new Document(engine.InitialMetadata, pipeline) };
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
