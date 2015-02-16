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
        public void ReadFilesWithNoInputPathFindsNoFiles()
        {
            // Given
            TestPipelineBuilder builder = new TestPipelineBuilder();
            builder.ReadFiles("*.txt");
            Engine engine = new Engine();
            IPipelineContext context = new TestPipelineContext(engine, engine.Metadata, null, null);

            // When
            IEnumerable<IPipelineContext> contexts = builder.Module.Prepare(context);

            // Then
            Assert.AreEqual(0, contexts.Count());
        }

        [Test]
        public void ReadFilesThrowsOnNullPathFunction()
        {
            // Given
            TestPipelineBuilder builder = new TestPipelineBuilder();

            // When

            // Then
            Assert.Throws<ArgumentNullException>(() => builder.ReadFiles((Func<dynamic, string>)null));
        }

        [Test]
        public void ReadFilesThrowsOnNullExtension()
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
        public void ReadFilesWithSearchPatternFindsCorrectFiles(string searchPattern, SearchOption searchOption, int expectedCount)
        {
            // Given
            TestPipelineBuilder builder = new TestPipelineBuilder();
            builder.ReadFiles(searchPattern, searchOption);
            Engine engine = new Engine();
            engine.Metadata.InputPath = @"TestFiles\Input\";
            IPipelineContext context = new TestPipelineContext(engine, engine.Metadata, null, null);

            // When
            IEnumerable<IPipelineContext> contexts = builder.Module.Prepare(context);
            
            // Then
            Assert.AreEqual(expectedCount, contexts.Count());
        }

        // TODO: Add test for InputPath without trailing slash
    }
}
