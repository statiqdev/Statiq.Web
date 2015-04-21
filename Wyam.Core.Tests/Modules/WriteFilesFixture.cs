using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Core.Modules;

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
        public void NoOutputPathWritesNoFiles()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            IPipelineContext context = new PipelineContext(engine, metadata, null);
            WriteFiles writeFiles = new WriteFiles(".txt");

            // When
            IEnumerable<IPipelineContext> contexts = writeFiles.Prepare(context);
            int count = contexts.Count();

            // Then
            Assert.AreEqual(0, count);
        }

        [Test]
        public void ExtensionWithDotWritesFile()
        {
            // Given
            Engine engine = new Engine();
            engine.Metadata["OutputPath"] = @"TestFiles\Output\";
            engine.Metadata["FileRoot"] = @"TestFiles\Input";
            engine.Metadata["FileDir"] = @"TestFiles\Input\Subfolder";
            engine.Metadata["FileBase"] = @"write-test";
            Metadata metadata = new Metadata(engine);
            IPipelineContext context = new PipelineContext(engine, metadata, null);
            WriteFiles writeFiles = new WriteFiles(".txt");

            // When
            context = writeFiles.Prepare(context).First();
            string content = writeFiles.Execute(context, "Test");

            // Then
            Assert.IsTrue(File.Exists(@"TestFiles\Output\Subfolder\write-test.txt"));
            Assert.AreEqual("Test", File.ReadAllText(@"TestFiles\Output\Subfolder\write-test.txt"));
        }

        [Test]
        public void ExtensionWithoutDotWritesFile()
        {
            // Given
            Engine engine = new Engine();
            engine.Metadata["OutputPath"] = @"TestFiles\Output\";
            engine.Metadata["FileRoot"] = @"TestFiles\Input";
            engine.Metadata["FileDir"] = @"TestFiles\Input\Subfolder";
            engine.Metadata["FileBase"] = @"write-test";
            Metadata metadata = new Metadata(engine);
            IPipelineContext context = new PipelineContext(engine, metadata, null);
            WriteFiles writeFiles = new WriteFiles("txt");

            // When
            context = writeFiles.Prepare(context).First();
            string content = writeFiles.Execute(context, "Test");

            // Then
            Assert.IsTrue(File.Exists(@"TestFiles\Output\Subfolder\write-test.txt"));
            Assert.AreEqual("Test", File.ReadAllText(@"TestFiles\Output\Subfolder\write-test.txt"));
        }

        [Test]
        public void ExecuteReturnsSameContent()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            IPipelineContext context = new PipelineContext(engine, metadata, null);
            WriteFiles writeFiles = new WriteFiles(x => string.Empty);

            // When
            string content = writeFiles.Execute(context, "Test");

            // Then
            Assert.AreEqual("Test", content);
        }
    }
}
