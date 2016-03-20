using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Pipelines;
using Wyam.Core.Documents;
using Wyam.Core.IO;
using Wyam.Core.Modules.IO;
using Wyam.Core.Pipelines;
using Wyam.Testing;
using Wyam.Testing.IO;

namespace Wyam.Core.Tests.Modules.IO
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ReadFilesTests : BaseFixture
    {
        private Engine Engine { get; set; }
        private Pipeline Pipeline { get; set; }
        private IExecutionContext Context { get; set; }
        private IDocument[] Inputs { get; set; }

        [SetUp]
        public void SetUp()
        {
            Engine = new Engine();
            Engine.FileSystem.FileProviders.Add(string.Empty, GetFileProvider());
            Engine.FileSystem.RootPath = "/";
            Engine.FileSystem.InputPaths.Clear();
            Engine.FileSystem.InputPaths.Add("/TestFiles/Input");
            Pipeline = new Pipeline("Pipeline", null);
            Context = new ExecutionContext(Engine, Pipeline);
            Inputs = new [] { Context.GetDocument() };
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

            return fileProvider;
        }

        public class ConstructorTests : ReadFilesTests
        {
            [Test]
            public void ThrowsOnNullPathFunction()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new ReadFiles((DocumentConfig) null));
            }

            [Test]
            public void ThrowsOnNullPatterns()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new ReadFiles((string[]) null));
            }
        }

        public class ExecuteMethodTests : ReadFilesTests
        {
            [TestCase("*.foo", 0)]
            [TestCase("**/*.foo", 0)]
            [TestCase("*.txt", 2)]
            [TestCase("**/*.txt", 3)]
            [TestCase("*.md", 1)]
            [TestCase("**/*.md", 2)]
            [TestCase("*.*", 3)]
            [TestCase("**/*.*", 5)]
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
            public void GetsCorrectContent()
            {
                // Given
                ReadFiles readFiles = new ReadFiles("test-a.txt");

                // When
                IDocument document = readFiles.Execute(Inputs, Context).ToList().First();

                // Then
                Assert.AreEqual("aaa", document.Content);
            }

            [TestCase(Keys.SourceFileRoot, "/TestFiles/Input")]
            [TestCase(Keys.SourceFileBase, "test-c")]
            [TestCase(Keys.SourceFileExt, ".txt")]
            [TestCase(Keys.SourceFileName, "test-c.txt")]
            [TestCase(Keys.SourceFileDir, "/TestFiles/Input/Subfolder")]
            [TestCase(Keys.SourceFilePath, "/TestFiles/Input/Subfolder/test-c.txt")]
            [TestCase(Keys.SourceFilePathBase, "/TestFiles/Input/Subfolder/test-c")]
            [TestCase(Keys.RelativeFilePath, "Subfolder/test-c.txt")]
            [TestCase(Keys.RelativeFilePathBase, "Subfolder/test-c")]
            [TestCase(Keys.RelativeFileDir, "Subfolder")]
            public void ShouldSetMetadata(string key, string expected)
            {
                // Given
                ReadFiles readFiles = new ReadFiles("**/test-c.txt");

                // When
                IDocument output = readFiles.Execute(Inputs, Context).ToList().First();

                // Then
                Assert.AreEqual(output.String(key), expected);
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
