using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Core.Execution;
using Wyam.Core.Modules.IO;
using Wyam.Testing;
using Wyam.Testing.IO;

namespace Wyam.Core.Tests.Modules.IO
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class IncludeFixture : BaseFixture
    {
        private TestFileProvider FileProvider { get; set; }
        private Engine Engine { get; set; }
        private Pipeline Pipeline { get; set; }
        private IExecutionContext Context { get; set; }

        [SetUp]
        public void SetUp()
        {
            FileProvider = GetFileProvider();
            Engine = new Engine();
            Engine.FileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, FileProvider);
            Engine.FileSystem.RootPath = "/";
            Engine.FileSystem.InputPaths.Clear();
            Engine.FileSystem.InputPaths.Add("/TestFiles/Input");
            Pipeline = new Pipeline("Pipeline", null);
            Context = new ExecutionContext(Engine, Pipeline);
        }

        private TestFileProvider GetFileProvider()
        {
            TestFileProvider fileProvider = new TestFileProvider();

            fileProvider.AddDirectory("/");
            fileProvider.AddDirectory("/TestFiles");
            fileProvider.AddDirectory("/TestFiles/Input");
            fileProvider.AddDirectory("/TestFiles/Input/Subfolder");

            fileProvider.AddFile("/TestFiles/test-above-input.txt", "test");
            fileProvider.AddFile("/TestFiles/Input/test-a.txt", "aaa");
            fileProvider.AddFile("/TestFiles/Input/test-b.txt", "bbb");
            fileProvider.AddFile("/TestFiles/Input/Subfolder/test-c.txt", "ccc");

            return fileProvider;
        }

        public class ExecuteTests : IncludeFixture
        {
            [Test]
            public void ThrowForNoSourceWhenIncludingRelativePath()
            {
                // Given
                IDocument[] documents =
                {
                    Context.GetDocument("foo ^\"test-a.txt\" bar")
                };
                Include include = new Include();

                // When, Then
                Assert.Throws<AggregateException>(() => include.Execute(documents, Context).ToList());
            }

            [Test]
            public void DoesNotThrowForNoSourceWhenIncludingAbsolutePath()
            {
                // Given
                IDocument[] documents =
                {
                    Context.GetDocument("foo ^\"/TestFiles/Input/test-a.txt\" bar")
                };
                Include include = new Include();

                // When, Then
                List<IDocument> results = include.Execute(documents, Context).ToList();

                // Then
                Assert.AreEqual("foo aaa bar", results.Single().Content);
            }

            [Test]
            public void IncludeOnFirstCharacter()
            {
                // Given
                IDocument[] documents =
                {
                    Context.GetDocument(new FilePath("/TestFiles/Input/test.txt"), "^\"test-a.txt\"foo")
                };
                Include include = new Include();

                // When
                List<IDocument> results = include.Execute(documents, Context).ToList();

                // Then
                Assert.AreEqual("aaafoo", results.Single().Content);
            }

            [Test]
            public void DoesNotIncludeOnFirstCharacterIfEscaped()
            {
                // Given
                IDocument[] documents =
                {
                    Context.GetDocument(new FilePath("/TestFiles/Input/test.txt"), "\\^\"test-a.txt\"foo")
                };
                Include include = new Include();

                // When
                List<IDocument> results = include.Execute(documents, Context).ToList();

                // Then
                Assert.AreEqual("^\"test-a.txt\"foo", results.Single().Content);
            }

            [Test]
            public void MultipleEscapeCharacters()
            {
                // Given
                IDocument[] documents =
                {
                    Context.GetDocument(new FilePath("/TestFiles/Input/test.txt"), "\\\\\\^\"test-a.txt\"foo")
                };
                Include include = new Include();

                // When
                List<IDocument> results = include.Execute(documents, Context).ToList();

                // Then
                Assert.AreEqual("\\\\^\"test-a.txt\"foo", results.Single().Content);
            }

            [Test]
            public void MultipleIncludes()
            {
                // Given
                IDocument[] documents =
                {
                    Context.GetDocument(
                        new FilePath("/TestFiles/Input/test.txt"), 
                        "x ^\"test-a.txt\" y ^\"test-b.txt\" z")
                };
                Include include = new Include();

                // When
                List<IDocument> results = include.Execute(documents, Context).ToList();

                // Then
                Assert.AreEqual("x aaa y bbb z", results.Single().Content);
            }

            [Test]
            public void MultipleAdjacentIncludes()
            {
                // Given
                IDocument[] documents =
                {
                    Context.GetDocument(
                        new FilePath("/TestFiles/Input/test.txt"),
                        "x ^\"test-a.txt\"^\"test-b.txt\" z")
                };
                Include include = new Include();

                // When
                List<IDocument> results = include.Execute(documents, Context).ToList();

                // Then
                Assert.AreEqual("x aaabbb z", results.Single().Content);
            }

            [Test]
            public void FileNotFoundRemovesIncludeStatement()
            {
                // Given
                ThrowOnTraceEventType(System.Diagnostics.TraceEventType.Error);
                IDocument[] documents =
                {
                    Context.GetDocument(
                        new FilePath("/TestFiles/Input/test.txt"),
                        "x ^\"test-c.txt\" y")
                };
                Include include = new Include();

                // When
                List<IDocument> results = include.Execute(documents, Context).ToList();

                // Then
                Assert.AreEqual("x  y", results.Single().Content);
            }

            [Test]
            public void IncludingRelativePath()
            {
                // Given
                IDocument[] documents =
                {
                    Context.GetDocument(
                        new FilePath("/TestFiles/Input/test.txt"),
                        "x ^\"Subfolder/test-c.txt\" y")
                };
                Include include = new Include();

                // When
                List<IDocument> results = include.Execute(documents, Context).ToList();

                // Then
                Assert.AreEqual("x ccc y", results.Single().Content);
            }

            [Test]
            public void IncludingRelativePathOutsideInput()
            {
                // Given
                IDocument[] documents =
                {
                    Context.GetDocument(
                        new FilePath("/TestFiles/Input/test.txt"),
                        "x ^\"../test-above-input.txt\" y")
                };
                Include include = new Include();

                // When
                List<IDocument> results = include.Execute(documents, Context).ToList();

                // Then
                Assert.AreEqual("x test y", results.Single().Content);
            }


            [Test]
            public void IncludingAbsolutePath()
            {
                // Given
                IDocument[] documents =
                {
                    Context.GetDocument(
                        new FilePath("/TestFiles/Input/test.txt"),
                        "x ^\"/TestFiles/test-above-input.txt\" y")
                };
                Include include = new Include();

                // When
                List<IDocument> results = include.Execute(documents, Context).ToList();

                // Then
                Assert.AreEqual("x test y", results.Single().Content);
            }

            [Test]
            public void NestedInclude()
            {
                // Given
                FileProvider.AddFile(
                    "/TestFiles/Input/test-outer.txt", 
                    "3 ^\"test-a.txt\" 4");
                IDocument[] documents =
                {
                    Context.GetDocument(
                        new FilePath("/TestFiles/Input/test.txt"),
                        "1 ^\"test-outer.txt\" 2")
                };
                Include include = new Include();

                // When
                List<IDocument> results = include.Execute(documents, Context).ToList();

                // Then
                Assert.AreEqual("1 3 aaa 4 2", results.Single().Content);
            }

            [Test]
            public void NestedIncludeWithoutRecursion()
            {
                // Given
                FileProvider.AddFile(
                    "/TestFiles/Input/test-outer.txt",
                    "3 ^\"test-a.txt\" 4");
                IDocument[] documents =
                {
                    Context.GetDocument(
                        new FilePath("/TestFiles/Input/test.txt"),
                        "1 ^\"test-outer.txt\" 2")
                };
                Include include = new Include().WithRecursion(false);

                // When
                List<IDocument> results = include.Execute(documents, Context).ToList();

                // Then
                Assert.AreEqual("1 3 ^\"test-a.txt\" 4 2", results.Single().Content);
            }

            [Test]
            public void NestedIncludeWithInnerEscape()
            {
                // Given
                FileProvider.AddFile(
                    "/TestFiles/Input/test-outer.txt",
                    "3 \\^\"test-a.txt\" 4");
                IDocument[] documents =
                {
                    Context.GetDocument(
                        new FilePath("/TestFiles/Input/test.txt"),
                        "1 ^\"test-outer.txt\" 2")
                };
                Include include = new Include().WithRecursion(false);

                // When
                List<IDocument> results = include.Execute(documents, Context).ToList();

                // Then
                Assert.AreEqual("1 3 \\^\"test-a.txt\" 4 2", results.Single().Content);
            }

            [Test]
            public void MultipleNestedInclude()
            {
                // Given
                FileProvider.AddFile(
                    "/TestFiles/Input/test-outer.txt",
                    "3 ^\"test-inner.txt\" 4");
                FileProvider.AddFile(
                    "/TestFiles/Input/test-inner.txt",
                    "5 ^\"test-a.txt\" 6");
                IDocument[] documents =
                {
                    Context.GetDocument(
                        new FilePath("/TestFiles/Input/test.txt"),
                        "1 ^\"test-outer.txt\" 2")
                };
                Include include = new Include();

                // When
                List<IDocument> results = include.Execute(documents, Context).ToList();

                // Then
                Assert.AreEqual("1 3 5 aaa 6 4 2", results.Single().Content);
            }
        }
    }
}
