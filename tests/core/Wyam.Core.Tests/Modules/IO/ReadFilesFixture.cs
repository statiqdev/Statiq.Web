using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Documents;
using Wyam.Core.IO;
using Wyam.Core.Modules.IO;
using Wyam.Core.Execution;
using Wyam.Testing;
using Wyam.Testing.IO;

namespace Wyam.Core.Tests.Modules.IO
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ReadFilesFixture : BaseFixture
    {
        private Engine Engine { get; set; }
        private ExecutionPipeline Pipeline { get; set; }
        private IExecutionContext Context { get; set; }
        private IDocument[] Inputs { get; set; }

        [SetUp]
        public void SetUp()
        {
            Engine = new Engine();
            Engine.FileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, GetFileProvider());
            Engine.FileSystem.RootPath = "/";
            Engine.FileSystem.InputPaths.Clear();
            Engine.FileSystem.InputPaths.Add("/TestFiles/Input");
            Pipeline = new ExecutionPipeline("Pipeline", (IModuleList)null);
            Context = new ExecutionContext(Engine, Pipeline);
            Inputs = new[] { Context.GetDocument() };
        }

        private IFileProvider GetFileProvider()
        {
            TestFileProvider fileProvider = new TestFileProvider();

            fileProvider.AddDirectory("/");
            fileProvider.AddDirectory("/TestFiles");
            fileProvider.AddDirectory("/TestFiles/Input");
            fileProvider.AddDirectory("/TestFiles/Input/Subfolder");

            fileProvider.AddFile("/TestFiles/test-above-input.txt", "test");
            fileProvider.AddFile("/TestFiles/Input/markdown-x.md", "xxx");
            fileProvider.AddFile("/TestFiles/Input/test-a.txt", "aaa");
            fileProvider.AddFile("/TestFiles/Input/test-b.txt", "bbb");
            fileProvider.AddFile("/TestFiles/Input/Subfolder/markdown-y.md", "yyy");
            fileProvider.AddFile("/TestFiles/Input/Subfolder/test-c.txt", "ccc");
            fileProvider.AddFile("/TestFiles/Input/.dotfile", "dotfile");

            return fileProvider;
        }

        public class ConstructorTests : ReadFilesFixture
        {
            [Test]
            public void ThrowsOnNullPathFunction()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new ReadFiles((DocumentConfig)null));
            }

            [Test]
            public void ThrowsOnNullPatterns()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new ReadFiles((string[])null));
            }
        }

        public class ExecuteTests : ReadFilesFixture
        {
            [TestCase("*.foo", 0)]
            [TestCase("**/*.foo", 0)]
            [TestCase("*.txt", 2)]
            [TestCase("**/*.txt", 3)]
            [TestCase("*.md", 1)]
            [TestCase("**/*.md", 2)]
            [TestCase("*.*", 4)]
            [TestCase("**/*.*", 6)]
            public void PatternFindsCorrectFiles(string pattern, int expectedCount)
            {
                // Given
                ReadFiles readFiles = new ReadFiles(pattern);

                // When
                IEnumerable<IDocument> documents = readFiles.Execute(Inputs, Context).ToList();

                // Then
                Assert.AreEqual(expectedCount, documents.Count());
            }

            [Test]
            public void PatternWorksWithSubpath()
            {
                // Given
                ReadFiles readFiles = new ReadFiles("Subfolder/*.txt");

                // When
                IEnumerable<IDocument> documents = readFiles.Execute(Inputs, Context);

                // Then
                Assert.AreEqual(1, documents.Count());
            }

            [Test]
            public void PatternWorksWithSingleFile()
            {
                // Given
                ReadFiles readFiles = new ReadFiles("test-a.txt");

                // When
                IEnumerable<IDocument> documents = readFiles.Execute(Inputs, Context).ToList();

                // Then
                Assert.AreEqual(1, documents.Count());
            }

            [Test]
            public void ShouldReturnNullBasePathsForDotFiles()
            {
                // Given
                ReadFiles readFiles = new ReadFiles(".dotfile");

                // When
                IDocument document = readFiles.Execute(Inputs, Context).ToList().First();

                // Then
                Assert.IsNull(document[Keys.SourceFileBase]);
                Assert.IsNull(document[Keys.SourceFilePathBase]);
                Assert.IsNull(document[Keys.RelativeFilePathBase]);
            }

            [Test]
            public void GetsCorrectContent()
            {
                // Given
                ReadFiles readFiles = new ReadFiles("test-a.txt");

                // When
                IDocument document = readFiles.Execute(Inputs, Context).ToList().First();

                // Then
                Assert.AreEqual("aaa", document.Content);
            }

            [TestCase(Keys.SourceFileBase, "test-c")]
            [TestCase(Keys.SourceFileName, "test-c.txt")]
            [TestCase(Keys.SourceFilePath, "/TestFiles/Input/Subfolder/test-c.txt")]
            [TestCase(Keys.SourceFilePathBase, "/TestFiles/Input/Subfolder/test-c")]
            [TestCase(Keys.RelativeFilePath, "Subfolder/test-c.txt")]
            [TestCase(Keys.RelativeFilePathBase, "Subfolder/test-c")]
            public void ShouldSetFilePathMetadata(string key, string expected)
            {
                // Given
                ReadFiles readFiles = new ReadFiles("**/test-c.txt");

                // When
                IDocument output = readFiles.Execute(Inputs, Context).ToList().First();

                // Then
                object result = output[key];
                Assert.IsInstanceOf<FilePath>(result);
                Assert.AreEqual(expected, ((FilePath)result).FullPath);
            }

            [TestCase(Keys.SourceFileRoot, "/TestFiles/Input")]
            [TestCase(Keys.SourceFileDir, "/TestFiles/Input/Subfolder")]
            [TestCase(Keys.RelativeFileDir, "Subfolder")]
            public void ShouldSetDirectoryPathMetadata(string key, string expected)
            {
                // Given
                ReadFiles readFiles = new ReadFiles("**/test-c.txt");

                // When
                IDocument output = readFiles.Execute(Inputs, Context).ToList().First();

                // Then
                object result = output[key];
                Assert.IsInstanceOf<DirectoryPath>(result);
                Assert.AreEqual(expected, ((DirectoryPath)result).FullPath);
            }

            [TestCase(Keys.SourceFileExt, ".txt")]
            public void ShouldSetStringMetadata(string key, string expected)
            {
                // Given
                ReadFiles readFiles = new ReadFiles("**/test-c.txt");

                // When
                IDocument output = readFiles.Execute(Inputs, Context).ToList().First();

                // Then
                object result = output[key];
                Assert.IsInstanceOf<string>(result);
                Assert.AreEqual(expected, result);
            }

            [Test]
            public void WorksWithMultipleExtensions()
            {
                // Given
                ReadFiles readFiles = new ReadFiles("**/*.{txt,md}");

                // When
                IEnumerable<IDocument> documents = readFiles.Execute(Inputs, Context).ToList();

                // Then
                Assert.AreEqual(5, documents.Count());
            }

            [Test]
            public void PredicateShouldReturnMatchingFiles()
            {
                // Given
                ReadFiles readFiles = new ReadFiles("**/*").Where(x => x.Path.FullPath.Contains("test"));

                // When
                IEnumerable<IDocument> documents = readFiles.Execute(Inputs, Context).ToList();

                // Then
                Assert.AreEqual(3, documents.Count());
            }
        }
    }
}
