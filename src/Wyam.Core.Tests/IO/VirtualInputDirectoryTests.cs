using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.IO;
using Wyam.Core.IO;
using Wyam.Testing;

namespace Wyam.Core.Tests.IO
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class VirtualInputDirectoryTests : BaseFixture
    {
        public class ConstructorTests : VirtualInputDirectoryTests
        {
            [Test]
            public void ThrowsForNullFileSystem()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new VirtualInputDirectory(null, new DirectoryPath("A")));
            }

            [Test]
            public void ThrowsForNullDirectoryPath()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new VirtualInputDirectory(new FileSystem(), null));
            }

            [Test]
            public void ThrowsForNonRelativePath()
            {
                // Given, When, Then
                Assert.Throws<ArgumentException>(() => new VirtualInputDirectory(new FileSystem(), new DirectoryPath("/A")));
            }
        }

        public class GetDirectoriesTests : VirtualInputDirectoryTests
        {
            [Test]
            [TestCase(".", SearchOption.AllDirectories, new [] { "c", "d", "a", "a/b" })]
            [TestCase(".", SearchOption.TopDirectoryOnly, new[] { "c", "d", "a" })]
            [TestCase("a", SearchOption.AllDirectories, new[] { "b" })]
            [TestCase("a", SearchOption.TopDirectoryOnly, new [] { "b" })]
            public void GetsDirectories(string virtualPath, SearchOption searchOption, string[] expectedPaths)
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b");
                fileSystem.InputPaths.Add("alt::/foo");
                fileSystem.FileProviders.Add(string.Empty, GetFileProviderA());
                fileSystem.FileProviders.Add("alt", GetFileProviderB());
                VirtualInputDirectory directory = new VirtualInputDirectory(fileSystem, virtualPath);

                // When
                IEnumerable<IDirectory> directories = directory.GetDirectories(searchOption);

                // Then
                CollectionAssert.AreEquivalent(expectedPaths, directories.Select(x => x.Path.FullPath));
            }
        }

        // TODO: GetFilesTests

        public class GetFileTests : VirtualInputDirectoryTests
        {
            [Test]
            [TestCase(".", "c/foo.txt", "/a/b/c/foo.txt", true)]
            [TestCase(".", "baz.txt", "/foo/baz.txt", true)]
            [TestCase("c", "foo.txt", "/a/b/c/foo.txt", true)]
            [TestCase("c", "baz.txt", "/foo/c/baz.txt", true)]
            [TestCase("c", "bar.txt", "/foo/c/bar.txt", false)]
            [TestCase("x/y/z", "bar.txt", "/foo/x/y/z/bar.txt", false)]
            public void GetsInputFile(string virtualPath, string filePath, string expectedPath, bool expectedExists)
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b");
                fileSystem.InputPaths.Add("alt::/foo");
                fileSystem.FileProviders.Add(string.Empty, GetFileProviderA());
                fileSystem.FileProviders.Add("alt", GetFileProviderB());
                VirtualInputDirectory directory = new VirtualInputDirectory(fileSystem, virtualPath);

                // When
                IFile file = directory.GetFile(filePath);

                // Then
                Assert.AreEqual(expectedPath, file.Path.FullPath);
                Assert.AreEqual(expectedExists, file.Exists);
            }
        }

        private IFileProvider GetFileProviderA()
        {
            string[] directories =
            {
                "/a",
                "/a/b",
                "/a/b/c",
                "/a/b/d",
                "/a/x",
                "/a/y",
                "/a/y/z"
            };
            string[] files =
            {
                "/a/b/c/foo.txt",
                "/a/b/c/baz.txt",
                "/a/b/d/baz.txt",
                "/a/x/bar.txt"
            };
            IFileProvider fileProvider = Substitute.For<IFileProvider>();
            fileProvider.GetDirectory(Arg.Any<DirectoryPath>())
                .Returns(x =>
                {
                    string path = ((DirectoryPath)x[0]).FullPath;
                    return GetDirectory(path, directories.Contains(path), directories);
                });
            fileProvider.GetFile(Arg.Any<FilePath>())
                .Returns(x =>
                {
                    string path = ((FilePath)x[0]).FullPath;
                    return GetFile(path, files.Contains(path));
                });
            return fileProvider;
        }

        private IFileProvider GetFileProviderB()
        {
            string[] directories =
            {
                "/foo",
                "/foo/a",
                "/foo/a/b",
                "/foo/c",
                "/bar",
            };
            string[] files =
            {
                "/foo/baz.txt",
                "/foo/c/baz.txt",
                "/bar/baz.txt"
            };
            IFileProvider fileProvider = Substitute.For<IFileProvider>();
            fileProvider.GetDirectory(Arg.Any<DirectoryPath>())
                .Returns(x =>
                {
                    string path = ((DirectoryPath)x[0]).FullPath;
                    return GetDirectory(path, directories.Contains(path), directories);
                });
            fileProvider.GetFile(Arg.Any<FilePath>())
                .Returns(x =>
                {
                    string path = ((FilePath)x[0]).FullPath;
                    return GetFile(path, files.Contains(path));
                });
            return fileProvider;
        }

        private IFile GetFile(string path, bool exists)
        {
            IFile file = Substitute.For<IFile>();
            file.Path.Returns(new FilePath(path));
            file.Exists.Returns(exists);
            return file;
        }

        private IDirectory GetDirectory(string path, bool exists, string[] directories)
        {
            IDirectory directory = Substitute.For<IDirectory>();
            directory.Path.Returns(new DirectoryPath(path));
            directory.GetDirectories(SearchOption.AllDirectories)
                .Returns(directories
                    .Where(x => x.StartsWith(path) && path != x)
                    .Select(x => GetDirectory(x, true, directories)));
            directory.GetDirectories(SearchOption.TopDirectoryOnly)
                .Returns(directories
                    .Where(x => x.StartsWith(path) && path.Count(c => c == '/') == x.Count(c => c == '/') - 1)
                    .Select(x => GetDirectory(x, true, directories)));
            directory.Exists.Returns(exists);
            return directory;
        }
    }
}
