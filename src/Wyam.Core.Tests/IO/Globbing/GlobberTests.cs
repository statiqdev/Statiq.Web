using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.IO;
using Wyam.Core.IO.Globbing;
using Wyam.Testing;
using Wyam.Testing.IO;

namespace Wyam.Core.Tests.IO.Globbing
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class GlobberTests : BaseFixture
    {
        public class GetFilesMethodTests : GlobberTests
        {
            [Test]
            [TestCase("/a", new[] { "b/c/foo.txt" }, new[] { "/a/b/c/foo.txt" })]
            [TestCase("/a", new[] { "/b/c/foo.txt" }, new [] { "/a/b/c/foo.txt" })]
            [TestCase("/a", new[] { "**/foo.txt" }, new[] { "/a/b/c/foo.txt" })]
            [TestCase("/a", new[] { "**/baz.txt" }, new[] { "/a/b/c/baz.txt", "/a/b/d/baz.txt" })]
            [TestCase("/a/b/d", new[] { "**/baz.txt" }, new[] { "/a/b/d/baz.txt" })]
            [TestCase("/a", new[] { "**/c/baz.txt" }, new[] { "/a/b/c/baz.txt" })]
            [TestCase("/a", new[] { "**/c/**/baz.txt" }, new[] { "/a/b/c/baz.txt" })]
            [TestCase("/a", new[] { "**/c/*/baz.txt" }, new string[] { })]
            [TestCase("/a", new[] { "**/foo.txt", "**/baz.txt" }, new[] { "/a/b/c/foo.txt", "/a/b/c/baz.txt", "/a/b/d/baz.txt" })]
            [TestCase("/a", new[] { "**/foo.txt", "**/c/baz.txt" }, new[] { "/a/b/c/foo.txt", "/a/b/c/baz.txt" })]
            [TestCase("/a", new[] { "**/baz.txt", "!**/d/*" }, new[] { "/a/b/c/baz.txt" })]
            public void ShouldReturnMatchedFiles(string directoryPath, string[] patterns, string[] resultPaths)
            {
                // Given
                IFileProvider fileProvider = GetFileProvider();
                IDirectory directory = fileProvider.GetDirectory(directoryPath);

                // When
                IEnumerable<IFile> matches = Globber.GetFiles(directory, patterns);

                // Then
                CollectionAssert.AreEquivalent(resultPaths, matches.Select(x => x.Path.FullPath));
            }
        }

        private IFileProvider GetFileProvider()
        {
            TestFileProvider fileProvider = new TestFileProvider();
            fileProvider.Directories.AddRange(new[]
            {
                "/a",
                "/a/b",
                "/a/b/c",
                "/a/b/c/1",
                "/a/b/d",
                "/a/x",
                "/a/y",
                "/a/y/z"
            });
            fileProvider.Files.AddRange(new[]
            {
                "/a/b/c/foo.txt",
                "/a/b/c/baz.txt",
                "/a/b/c/1/2.txt",
                "/a/b/d/baz.txt",
                "/a/x/bar.txt"
            });
            return fileProvider;
        }
    }
}
