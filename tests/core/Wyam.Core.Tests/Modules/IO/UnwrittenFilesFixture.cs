using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;
using Wyam.Testing.IO;

namespace Wyam.Core.Tests.Modules.IO
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class UnwrittenFilesFixture : BaseFixture
    {
        [Test]
        public void DoesNotOutputExistingFiles()
        {
            // Given
            TestExecutionContext context = GetContext();
            IDocument[] inputs =
            {
                new TestDocument(new MetadataItems
                {
                    { Keys.RelativeFilePath, new FilePath("test.md") }
                })
            };
            UnwrittenFiles unwrittenFiles = new UnwrittenFiles();

            // When
            IEnumerable<IDocument> outputs = unwrittenFiles.Execute(inputs, context).ToList();

            // Then
            Assert.AreEqual(0, outputs.Count());
        }

        [Test]
        public void DoesNotOutputExistingFilesWithDifferentExtension()
        {
            // Given
            TestExecutionContext context = GetContext();
            IDocument[] inputs =
            {
                new TestDocument(new MetadataItems
                {
                    { Keys.RelativeFilePath, new FilePath("test.txt") }
                })
            };
            UnwrittenFiles unwrittenFiles = new UnwrittenFiles(".md");

            // When
            IEnumerable<IDocument> outputs = unwrittenFiles.Execute(inputs, context).ToList();

            // Then
            Assert.AreEqual(0, outputs.Count());
        }

        [Test]
        public void ShouldOutputNonExistingFiles()
        {
            // Given
            TestExecutionContext context = GetContext();
            IDocument[] inputs =
            {
                new TestDocument("Test", new MetadataItems
                {
                    { Keys.RelativeFilePath, new FilePath("foo.txt") }
                })
            };
            UnwrittenFiles unwrittenFiles = new UnwrittenFiles();

            // When
            IEnumerable<IDocument> outputs = unwrittenFiles.Execute(inputs, context).ToList();

            // Then
            Assert.AreEqual(1, outputs.Count());
            Assert.AreEqual("Test", outputs.First().Content);
        }

        [Test]
        public void ShouldOutputNonExistingFilesWithDifferentExtension()
        {
            // Given
            TestExecutionContext context = GetContext();
            IDocument[] inputs =
            {
                new TestDocument("Test", new MetadataItems
                {
                    { Keys.RelativeFilePath, new FilePath("test.md") }
                })
            };
            UnwrittenFiles unwrittenFiles = new UnwrittenFiles(".txt");

            // When
            IEnumerable<IDocument> outputs = unwrittenFiles.Execute(inputs, context).ToList();

            // Then
            Assert.AreEqual(1, outputs.Count());
            Assert.AreEqual("Test", outputs.First().Content);
        }

        private TestExecutionContext GetContext() => new TestExecutionContext
        {
            FileSystem = new TestFileSystem
            {
                FileProvider = GetFileProvider(),
                RootPath = "/"
            }
        };

        private TestFileProvider GetFileProvider()
        {
            TestFileProvider fileProvider = new TestFileProvider();

            fileProvider.AddDirectory("/");
            fileProvider.AddDirectory("/output");

            fileProvider.AddFile("/output/test.md");

            return fileProvider;
        }
    }
}
