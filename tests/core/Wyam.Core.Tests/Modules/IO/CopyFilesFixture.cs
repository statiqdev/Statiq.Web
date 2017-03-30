using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Core.Documents;
using Wyam.Core.Modules.IO;
using Wyam.Core.Execution;
using Wyam.Testing;
using Wyam.Testing.IO;
using ExecutionContext = Wyam.Core.Execution.ExecutionContext;

namespace Wyam.Core.Tests.Modules.IO
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class CopyFilesFixture : BaseFixture
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

            return fileProvider;
        }

        public class ConstructorTests : CopyFilesFixture
        {
            [Test]
            public void ThrowsOnNullPathFunction()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new CopyFiles((DocumentConfig)null));
            }

            [Test]
            public void ThrowsOnNullPatterns()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new CopyFiles((string[])null));
            }
        }

        public class ExecuteTests : CopyFilesFixture
        {
            [Test]
            public void RecursivePatternCopiesFiles()
            {
                // Given
                CopyFiles copyFiles = new CopyFiles("**/*.txt");

                // When
                copyFiles.Execute(Inputs, Context).ToList();

                // Then
                Assert.IsTrue(Engine.FileSystem.GetOutputFile("test-a.txt").Exists);
                Assert.IsTrue(Engine.FileSystem.GetOutputFile("test-b.txt").Exists);
                Assert.IsTrue(Engine.FileSystem.GetOutputFile("Subfolder/test-c.txt").Exists);
                Assert.IsTrue(Engine.FileSystem.GetOutputDirectory("Subfolder").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("markdown-x.md").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("Subfolder/markdown-y.md").Exists);
            }

            [Test]
            public void CopyFilesInTopDirectoryOnly()
            {
                // Given
                CopyFiles copyFiles = new CopyFiles("*.txt");

                // When
                copyFiles.Execute(Inputs, Context).ToList();

                // Then
                Assert.IsTrue(Engine.FileSystem.GetOutputFile("test-a.txt").Exists);
                Assert.IsTrue(Engine.FileSystem.GetOutputFile("test-b.txt").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("Subfolder/test-c.txt").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputDirectory("Subfolder").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("markdown-x.md").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("Subfolder/markdown-y.md").Exists);
            }

            [Test]
            public void CopyFilesInSubfolderOnly()
            {
                // Given
                CopyFiles copyFiles = new CopyFiles("Subfolder/*.txt");

                // When
                copyFiles.Execute(Inputs, Context).ToList();

                // Then
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("test-a.txt").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("test-b.txt").Exists);
                Assert.IsTrue(Engine.FileSystem.GetOutputFile("Subfolder/test-c.txt").Exists);
                Assert.IsTrue(Engine.FileSystem.GetOutputDirectory("Subfolder").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("markdown-x.md").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("Subfolder/markdown-y.md").Exists);
            }

            [Test]
            public void CopyFilesAboveInputPath()
            {
                // Given
                CopyFiles copyFiles = new CopyFiles("../*.txt");

                // When
                copyFiles.Execute(Inputs, Context).ToList();

                // Then
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("test-a.txt").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("test-b.txt").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("Subfolder/test-c.txt").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputDirectory("Subfolder").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("markdown-x.md").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("Subfolder/markdown-y.md").Exists);
                Assert.IsTrue(Engine.FileSystem.GetOutputFile("test-above-input.txt").Exists);
            }

            [Test]
            public void CopyFilesAboveInputPathWithOthers()
            {
                // Given
                CopyFiles copyFiles = new CopyFiles("../**/*.txt");

                // When
                copyFiles.Execute(Inputs, Context).ToList();

                // Then
                Assert.IsTrue(Engine.FileSystem.GetOutputFile("test-a.txt").Exists);
                Assert.IsTrue(Engine.FileSystem.GetOutputFile("test-b.txt").Exists);
                Assert.IsTrue(Engine.FileSystem.GetOutputFile("Subfolder/test-c.txt").Exists);
                Assert.IsTrue(Engine.FileSystem.GetOutputDirectory("Subfolder").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("markdown-x.md").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("Subfolder/markdown-y.md").Exists);
                Assert.IsTrue(Engine.FileSystem.GetOutputFile("test-above-input.txt").Exists);
            }

            [Test]
            public void CopyFolderFromAbsolutePath()
            {
                // Given
                CopyFiles copyFiles = new CopyFiles("/TestFiles/Input/**/*.txt");

                // When
                copyFiles.Execute(Inputs, Context).ToList();

                // Then
                Assert.IsTrue(Engine.FileSystem.GetOutputFile("test-a.txt").Exists);
                Assert.IsTrue(Engine.FileSystem.GetOutputFile("test-b.txt").Exists);
                Assert.IsTrue(Engine.FileSystem.GetOutputFile("Subfolder/test-c.txt").Exists);
                Assert.IsTrue(Engine.FileSystem.GetOutputDirectory("Subfolder").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("markdown-x.md").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("Subfolder/markdown-y.md").Exists);
            }

            [Test]
            public void CopyNonExistingFolder()
            {
                // Given
                CopyFiles copyFiles = new CopyFiles("NonExisting/**/*.txt");

                // When
                copyFiles.Execute(Inputs, Context).ToList();

                // Then
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("test-a.txt").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("test-b.txt").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("Subfolder/test-c.txt").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputDirectory("Subfolder").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("markdown-x.md").Exists);
                Assert.IsFalse(Engine.FileSystem.GetOutputFile("Subfolder/markdown-y.md").Exists);
            }

            [Test]
            public void ShouldSetMetadata()
            {
                // Given
                CopyFiles copyFiles = new CopyFiles("**/test-a.txt");

                // When
                copyFiles.Execute(Inputs, Context).ToList();
            }

            [TestCase(Keys.SourceFilePath, "/TestFiles/Input/test-a.txt")]
            [TestCase(Keys.DestinationFilePath, "/output/test-a.txt")]
            public void ShouldSetFilePathMetadata(string key, string expected)
            {
                // Given
                CopyFiles copyFiles = new CopyFiles("**/test-a.txt");

                // When
                IDocument output = copyFiles.Execute(Inputs, Context).ToList().First();

                // Then
                object result = output[key];
                Assert.IsInstanceOf<FilePath>(result);
                Assert.AreEqual(expected, ((FilePath)result).FullPath);
            }
        }
    }
}
