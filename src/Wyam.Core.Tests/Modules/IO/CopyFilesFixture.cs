using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Pipelines;
using Wyam.Core.Documents;
using Wyam.Core.Modules.IO;
using Wyam.Core.Pipelines;
using ExecutionContext = Wyam.Core.Pipelines.ExecutionContext;

namespace Wyam.Core.Tests.Modules.IO
{
    [TestFixture]
    public class CopyFilesFixture
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

            Directory.CreateDirectory(@"TestFiles\Output\");
        }

        [Test]
        public void CopyWithSearchPatternRecursive()
        {
            // Given
            Engine engine = new Engine();
            Trace.AddListener(new TestTraceListener());
            engine.RootFolder = TestContext.CurrentContext.TestDirectory;
            engine.InputFolder= @"TestFiles\Input\";
            engine.OutputFolder = @"TestFiles\Output\";

            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(engine, pipeline).Clone("Test") };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            CopyFiles copyFiles = new CopyFiles("*.txt");

            // When
            IEnumerable<IDocument> outputs = copyFiles.Execute(inputs, context).ToList();
            foreach (IDocument document in inputs.Concat(outputs))
            {
                ((IDisposable)document).Dispose();
            }

            // Then
            Assert.IsTrue(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\Output\test-a.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\Output\test-b.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\Output\Subfolder\test-c.txt")));
            Assert.IsFalse(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\Output\markdown-x.md")));
            Assert.IsFalse(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\Output\Subfolder\markdown-y.md")));
        }

        [Test]
        public void CopyWithSearchPatternTopDirectoryOnly()
        {
            // Given
            Engine engine = new Engine();
            Trace.AddListener(new TestTraceListener());
            engine.RootFolder = TestContext.CurrentContext.TestDirectory;
            engine.InputFolder = @"TestFiles\Input\";
            engine.OutputFolder = @"TestFiles\Output\";
            engine.CleanOutputFolder();

            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(engine, pipeline).Clone("Test") };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            CopyFiles copyFiles = new CopyFiles("*.txt").FromTopDirectoryOnly();

            // When
            IEnumerable<IDocument> outputs = copyFiles.Execute(inputs, context).ToList();
            foreach (IDocument document in inputs.Concat(outputs))
            {
                ((IDisposable)document).Dispose();
            }

            // Then
            Assert.IsTrue(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\Output\test-a.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\Output\test-b.txt")));
            Assert.IsFalse(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\Output\Subfolder\test-c.txt")));
        }

        [Test]
        public void CopySubFolder()
        {
            // Given
            Engine engine = new Engine();
            Trace.AddListener(new TestTraceListener());
            engine.RootFolder = TestContext.CurrentContext.TestDirectory;
            engine.InputFolder = @"TestFiles\Input\";
            engine.OutputFolder = @"TestFiles\Output\";
            engine.CleanOutputFolder();

            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(engine, pipeline).Clone("Test") };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            CopyFiles copyFiles = new CopyFiles("Subfolder\\*.txt").FromTopDirectoryOnly();

            // When
            IEnumerable<IDocument> outputs = copyFiles.Execute(inputs, context).ToList();
            foreach (IDocument document in inputs.Concat(outputs))
            {
                ((IDisposable)document).Dispose();
            }

            // Then
            Assert.IsFalse(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\Output\test-a.txt")));
            Assert.IsFalse(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\Output\test-b.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\Output\Subfolder\test-c.txt")));
        }

        [Test]
        public void CopyFolderFromAboveInputPath()
        {
            // Given
            Engine engine = new Engine();
            Trace.AddListener(new TestTraceListener());
            engine.RootFolder = TestContext.CurrentContext.TestDirectory;
            engine.InputFolder = @"TestFiles\Input\";
            engine.OutputFolder = @"TestFiles\Output\";
            engine.CleanOutputFolder();

            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(engine, pipeline).Clone("Test") };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            CopyFiles copyFiles = new CopyFiles("../*.txt").FromTopDirectoryOnly();

            // When
            IEnumerable<IDocument> outputs = copyFiles.Execute(inputs, context).ToList();
            foreach (IDocument document in inputs.Concat(outputs))
            {
                ((IDisposable)document).Dispose();
            }

            // Then
            Assert.IsFalse(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\Output\test-a.txt")));
            Assert.IsFalse(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\Output\test-b.txt")));
            Assert.IsFalse(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\Output\Subfolder\test-c.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\Output\test-above-input.txt")));
        }

        [Test]
        public void CopyFolderFromAbsolutePath()
        {
            // Given
            Engine engine = new Engine();
            Trace.AddListener(new TestTraceListener());
            engine.RootFolder = TestContext.CurrentContext.TestDirectory;
            engine.InputFolder = @"TestFiles\Input\";
            engine.OutputFolder = @"TestFiles\Output\";
            engine.CleanOutputFolder();

            string absoluteInputPath = Path.GetFullPath(engine.InputFolder);

            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(engine, pipeline).Clone("Test") };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            CopyFiles copyFiles = new CopyFiles(Path.Combine(absoluteInputPath, "*.txt")).FromTopDirectoryOnly();

            // When
            IEnumerable<IDocument> outputs = copyFiles.Execute(inputs, context).ToList();
            foreach (IDocument document in inputs.Concat(outputs))
            {
                ((IDisposable)document).Dispose();
            }

            // Then
            Assert.IsTrue(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\Output\test-a.txt")));
            Assert.IsTrue(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\Output\test-b.txt")));
            Assert.IsFalse(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\Output\Subfolder\test-c.txt")));
            Assert.IsFalse(File.Exists(Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\Output\test-above-input.txt")));
        }

        [Test]
        public void CopyNonExistentFolder()
        {
            // Given
            Engine engine = new Engine();
            Trace.AddListener(new TestTraceListener());
            engine.RootFolder = TestContext.CurrentContext.TestDirectory;
            engine.InputFolder = @"TestFiles\Input\";
            engine.OutputFolder = @"TestFiles\Output\";
            engine.CleanOutputFolder();

            Pipeline pipeline = new Pipeline("Pipeline", engine, null);
            IDocument[] inputs = { new Document(engine, pipeline).Clone("Test") };
            IExecutionContext context = new ExecutionContext(engine, pipeline);
            CopyFiles copyFiles = new CopyFiles("NonExistent\\*.txt");

            // When
            IEnumerable<IDocument> outputs = copyFiles.Execute(inputs, context).ToList();
            foreach (IDocument document in inputs.Concat(outputs))
            {
                ((IDisposable)document).Dispose();
            }

            // Then
            // No exception should be thrown
        }
    }
}
