using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Modules.IO;
using Wyam.Core.Execution;
using Wyam.Testing;
using Wyam.Testing.IO;

namespace Wyam.Core.Tests.Modules.IO
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class UnwrittenFilesFixture : BaseFixture
    {
        private Engine Engine { get; set; }
        private ExecutionPipeline Pipeline { get; set; }
        private IExecutionContext Context { get; set; }

        [SetUp]
        public void SetUp()
        {
            Engine = new Engine();
            Engine.FileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, GetFileProvider());
            Engine.FileSystem.RootPath = "/";
            Pipeline = new ExecutionPipeline("Pipeline", (IModuleList)null);
            Context = new ExecutionContext(Engine, Pipeline);
        }

        private IFileProvider GetFileProvider()
        {
            TestFileProvider fileProvider = new TestFileProvider();

            fileProvider.AddDirectory("/");
            fileProvider.AddDirectory("/output");

            fileProvider.AddFile("/output/test.md");

            return fileProvider;
        }

        [Test]
        public void DoesNotOutputExistingFiles()
        {
            // Given
            Engine.Settings[Keys.RelativeFilePath] = new FilePath("test.md");
            IDocument[] inputs = new[] { Context.GetDocument() };
            UnwrittenFiles unwrittenFiles = new UnwrittenFiles();

            // When
            IEnumerable<IDocument> outputs = unwrittenFiles.Execute(inputs, Context).ToList();

            // Then
            Assert.AreEqual(0, outputs.Count());
        }

        [Test]
        public void DoesNotOutputExistingFilesWithDifferentExtension()
        {
            // Given
            Engine.Settings[Keys.RelativeFilePath] = new FilePath("test.txt");
            IDocument[] inputs = new[] { Context.GetDocument() };
            UnwrittenFiles unwrittenFiles = new UnwrittenFiles(".md");

            // When
            IEnumerable<IDocument> outputs = unwrittenFiles.Execute(inputs, Context).ToList();

            // Then
            Assert.AreEqual(0, outputs.Count());
        }

        [Test]
        public void ShouldOutputNonExistingFiles()
        {
            // Given
            Engine.Settings[Keys.RelativeFilePath] = new FilePath("foo.txt");
            IDocument[] inputs = new[] { Context.GetDocument("Test") };
            UnwrittenFiles unwrittenFiles = new UnwrittenFiles();

            // When
            IEnumerable<IDocument> outputs = unwrittenFiles.Execute(inputs, Context).ToList();

            // Then
            Assert.AreEqual(1, outputs.Count());
            Assert.AreEqual("Test", outputs.First().Content);
        }

        [Test]
        public void ShouldOutputNonExistingFilesWithDifferentExtension()
        {
            // Given
            Engine.Settings[Keys.RelativeFilePath] = new FilePath("test.md");
            IDocument[] inputs = new[] { Context.GetDocument("Test") };
            UnwrittenFiles unwrittenFiles = new UnwrittenFiles(".txt");

            // When
            IEnumerable<IDocument> outputs = unwrittenFiles.Execute(inputs, Context).ToList();

            // Then
            Assert.AreEqual(1, outputs.Count());
            Assert.AreEqual("Test", outputs.First().Content);
        }
    }
}
