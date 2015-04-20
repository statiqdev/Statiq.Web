using System.IO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core.Modules;

namespace Wyam.Core.Tests.Modules
{
    [TestFixture]
    public class ReadFilesFixture
    {        
        [Test]
        public void NoInputPathFindsNoFiles()
        {
            // Given
            TestPipelineBuilder builder = new TestPipelineBuilder();
            builder.ReadFiles("*.txt");
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            IPipelineContext context = new PipelineContext(engine, metadata, null);

            // When
            IEnumerable<IPipelineContext> contexts = builder.Module.Prepare(context);
            int count = contexts.Count();

            // Then
            Assert.AreEqual(0, count);
        }

        [Test]
        public void ThrowsOnNullPathFunction()
        {
            // Given
            TestPipelineBuilder builder = new TestPipelineBuilder();

            // When

            // Then
            Assert.Throws<ArgumentNullException>(() => builder.ReadFiles((Func<dynamic, string>)null));
        }

        [Test]
        public void ThrowsOnNullExtension()
        {
            // Given
            TestPipelineBuilder builder = new TestPipelineBuilder();

            // When

            // Then
            Assert.Throws<ArgumentNullException>(() => builder.ReadFiles((string)null));
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
            TestPipelineBuilder builder = new TestPipelineBuilder();
            builder.ReadFiles(searchPattern, searchOption);
            Engine engine = new Engine();
            engine.Metadata["InputPath"] = @"TestFiles\Input\";
            Metadata metadata = new Metadata(engine);
            IPipelineContext context = new PipelineContext(engine, metadata, null);

            // When
            IEnumerable<IPipelineContext> contexts = builder.Module.Prepare(context);
            int count = contexts.Count();
            
            // Then
            Assert.AreEqual(expectedCount, count);
        }

        [Test]
        public void SearchPatternWorksWithoutInputPathTrailingSlash()
        {
            // Given
            TestPipelineBuilder builder = new TestPipelineBuilder();
            builder.ReadFiles("*.txt");
            Engine engine = new Engine();
            engine.Metadata["InputPath"] = @"TestFiles\Input";
            Metadata metadata = new Metadata(engine);
            IPipelineContext context = new PipelineContext(engine, metadata, null);

            // When
            IEnumerable<IPipelineContext> contexts = builder.Module.Prepare(context);
            int count = contexts.Count();
            
            // Then
            Assert.AreEqual(3, count);
        }

        [Test]
        public void SearchPatternWorksWithInputPathTrailingSlash()
        {
            // Given
            TestPipelineBuilder builder = new TestPipelineBuilder();
            builder.ReadFiles("*.txt");
            Engine engine = new Engine();
            engine.Metadata["InputPath"] = @"TestFiles\Input";
            Metadata metadata = new Metadata(engine);
            IPipelineContext context = new PipelineContext(engine, metadata, null);

            // When
            IEnumerable<IPipelineContext> contexts = builder.Module.Prepare(context);
            int count = contexts.Count();
            
            // Then
            Assert.AreEqual(3, count);
        }

        [Test]
        public void SearchPatternWorksWithSubpath()
        {
            // Given
            TestPipelineBuilder builder = new TestPipelineBuilder();
            builder.ReadFiles(@"Subfolder\*.txt");
            Engine engine = new Engine();
            engine.Metadata["InputPath"] = @"TestFiles\Input";
            Metadata metadata = new Metadata(engine);
            IPipelineContext context = new PipelineContext(engine, metadata, null);

            // When
            IEnumerable<IPipelineContext> contexts = builder.Module.Prepare(context);
            int count = contexts.Count();
            
            // Then
            Assert.AreEqual(1, count);
        }

        [Test]
        public void SearchPatternWorksWithSingleFile()
        {
            // Given
            TestPipelineBuilder builder = new TestPipelineBuilder();
            builder.ReadFiles(@"test-a.txt");
            Engine engine = new Engine();
            engine.Metadata["InputPath"] = @"TestFiles\Input";
            Metadata metadata = new Metadata(engine);
            IPipelineContext context = new PipelineContext(engine, metadata, null);

            // When
            IEnumerable<IPipelineContext> contexts = builder.Module.Prepare(context);
            int count = contexts.Count();
            
            // Then
            Assert.AreEqual(1, count);
        }

        [Test]
        public void ExecuteGetsCorrectContent()
        {
            // Given
            TestPipelineBuilder builder = new TestPipelineBuilder();
            builder.ReadFiles(@"test-a.txt");
            Engine engine = new Engine();
            engine.Metadata["InputPath"] = @"TestFiles\Input";
            Metadata metadata = new Metadata(engine);
            IPipelineContext context = new PipelineContext(engine, metadata, null);

            // When
            context = builder.Module.Prepare(context).First();
            string content = builder.Module.Execute(context, null);
            
            // Then
            Assert.AreEqual("aaa", content);
        }

        [TestCase("FileRoot", @"TestFiles\Input")]
        [TestCase("FileBase", @"test-c")]
        [TestCase("FileExt", @".txt")]
        [TestCase("FileName", @"test-c.txt")]
        [TestCase("FileDir", @"TestFiles\Input\Subfolder")]
        [TestCase("FilePath", @"TestFiles\Input\Subfolder\test-c.txt")]
        public void PrepareSetsMetadata(string key, string expectedEnding)
        {
            // Given
            TestPipelineBuilder builder = new TestPipelineBuilder();
            builder.ReadFiles(@"test-c.txt");
            Engine engine = new Engine();
            engine.Metadata["InputPath"] = @"TestFiles\Input";
            Metadata metadata = new Metadata(engine);
            IPipelineContext context = new PipelineContext(engine, metadata, null);

            // When
            context = builder.Module.Prepare(context).First();
            
            // Then
            Assert.That(context.Metadata[key], Is.StringEnding(expectedEnding));
        }
    }
}
