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
using Wyam.Testing.IO;

namespace Wyam.Core.Tests.IO
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class VirtualInputDirectoryFixture : BaseFixture
    {
        public class ConstructorTests : VirtualInputDirectoryFixture
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

        public class GetDirectoriesTests : VirtualInputDirectoryFixture
        {
            [Test]
            [TestCase(".", SearchOption.AllDirectories, new [] { "c", "c/1", "d", "a", "a/b" })]
            [TestCase(".", SearchOption.TopDirectoryOnly, new[] { "c", "d", "a" })]
            public void RootVirtualDirectoryDoesNotIncludeSelf(string virtualPath, SearchOption searchOption, string[] expectedPaths)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IEnumerable<IDirectory> directories = directory.GetDirectories(searchOption);

                // Then
                CollectionAssert.AreEquivalent(expectedPaths, directories.Select(x => x.Path.FullPath));
            }

            [Test]
            [TestCase("a", SearchOption.AllDirectories, new[] { "a/b" })]
            [TestCase("a", SearchOption.TopDirectoryOnly, new[] { "a/b" })]
            public void NonRootVirtualDirectoryIncludesSelf(string virtualPath, SearchOption searchOption, string[] expectedPaths)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IEnumerable<IDirectory> directories = directory.GetDirectories(searchOption);

                // Then
                CollectionAssert.AreEquivalent(expectedPaths, directories.Select(x => x.Path.FullPath));
            }
        }

        public class GetFilesTests : VirtualInputDirectoryFixture
        {
            [Test]
            [TestCase(".", SearchOption.AllDirectories, new[] { "/a/b/c/foo.txt", "/a/b/c/1/2.txt", "/a/b/d/baz.txt", "/foo/baz.txt", "/foo/c/baz.txt" })]
            [TestCase(".", SearchOption.TopDirectoryOnly, new[] { "/foo/baz.txt" })]
            [TestCase("c", SearchOption.AllDirectories, new[] { "/a/b/c/foo.txt", "/a/b/c/1/2.txt", "/foo/c/baz.txt" })]
            [TestCase("c", SearchOption.TopDirectoryOnly, new [] { "/a/b/c/foo.txt", "/foo/c/baz.txt" })]
            public void GetsFiles(string virtualPath, SearchOption searchOption, string[] expectedPaths)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IEnumerable<IFile> files = directory.GetFiles(searchOption);

                // Then
                CollectionAssert.AreEquivalent(expectedPaths, files.Select(x => x.Path.FullPath));
            }

        }

        public class GetFileTests : VirtualInputDirectoryFixture
        {
            [Test]
            [TestCase(".", "c/foo.txt", "/a/b/c/foo.txt", true)]
            [TestCase(".", "baz.txt", "/foo/baz.txt", true)]
            [TestCase("c", "foo.txt", "/a/b/c/foo.txt", true)]
            [TestCase("c", "1/2.txt", "/a/b/c/1/2.txt", true)]
            [TestCase("c", "1/3.txt", "/foo/c/1/3.txt", false)]
            [TestCase("c", "baz.txt", "/foo/c/baz.txt", true)]
            [TestCase("c", "bar.txt", "/foo/c/bar.txt", false)]
            [TestCase("x/y/z", "bar.txt", "/foo/x/y/z/bar.txt", false)]
            public void GetsInputFile(string virtualPath, string filePath, string expectedPath, bool expectedExists)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IFile file = directory.GetFile(filePath);

                // Then
                Assert.AreEqual(expectedPath, file.Path.FullPath);
                Assert.AreEqual(expectedExists, file.Exists);
            }

            [Test]
            public void GetsInputFileAboveInputDirectory()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b/d");
                fileSystem.InputPaths.Add("alt:///foo");
                fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, GetFileProviderA());
                fileSystem.FileProviders.Add("alt", GetFileProviderB());
                VirtualInputDirectory directory = new VirtualInputDirectory(fileSystem, ".");

                // When
                IFile file = directory.GetFile("../c/foo.txt");

                // Then
                Assert.AreEqual("/a/b/c/foo.txt", file.Path.FullPath);
            }

            [Test]
            public void ThrowsForNullPath()
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(".");

                // When, Then
                Assert.Throws<ArgumentNullException>(() => directory.GetFile(null));
            }

            [Test]
            public void ThrowsForAbsolutePath()
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(".");
                FilePath filePath = "/a/test.txt";

                // When, Then
                Assert.Throws<ArgumentException>(() => directory.GetFile(filePath));
            }
        }

        public class GetDirectoryTests : VirtualInputDirectoryFixture
        {
            [TestCase("a/b", "..", "a")]
            [TestCase("a/b/", "..", "a")]
            [TestCase("a/b/../c", "..", "a")]
            [TestCase(".", "..", ".")]
            [TestCase("a", "..", "a")]
            [TestCase("a/b", "c", "a/b/c")]
            public void ShouldReturnDirectory(string virtualPath, string path, string expected)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IDirectory result = directory.GetDirectory(path);

                // Then
                Assert.AreEqual(expected, result.Path.Collapse().FullPath);
            }

            [Test]
            public void ThrowsForNullPath()
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(".");

                // When, Then
                Assert.Throws<ArgumentNullException>(() => directory.GetDirectory(null));
            }

            [Test]
            public void ThrowsForAbsolutePath()
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(".");
                DirectoryPath directoryPath = "/a/b";

                // When, Then
                Assert.Throws<ArgumentException>(() => directory.GetDirectory(directoryPath));
            }
        }

        public class GetParentTests : VirtualInputDirectoryFixture
        {
            [TestCase("a/b", "a")]
            [TestCase("a/b/", "a")]
            [TestCase(".", null)]
            [TestCase("a", null)]
            public void ShouldReturnParentDirectory(string virtualPath, string expected)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IDirectory result = directory.Parent;

                // Then
                Assert.AreEqual(expected, result?.Path.Collapse().FullPath);
            }

        }

        public class ExistsTests : VirtualInputDirectoryFixture
        {
            [TestCase(".")]
            [TestCase("c")]
            [TestCase("c/1")]
            [TestCase("a/b")]
            public void ShouldReturnTrueForExistingPaths(string virtualPath)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When, Then
                Assert.IsTrue(directory.Exists);
            }

            [TestCase("x")]
            [TestCase("bar")]
            [TestCase("baz")]
            [TestCase("a/b/c")]
            [TestCase("q/w/e")]
            public void ShouldReturnFalseForNonExistingPaths(string virtualPath)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When, Then
                Assert.IsFalse(directory.Exists);
            }
        }

        public class CreateTests : VirtualInputDirectoryFixture
        {
            [Test]
            public void ShouldThrow()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                VirtualInputDirectory directory = new VirtualInputDirectory(fileSystem, ".");

                // When, Then
                Assert.Throws<NotSupportedException>(() => directory.Create());
            }
        }

        public class DeleteTests : VirtualInputDirectoryFixture
        {
            [Test]
            public void ShouldThrow()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                VirtualInputDirectory directory = new VirtualInputDirectory(fileSystem, ".");

                // When, Then
                Assert.Throws<NotSupportedException>(() => directory.Delete(false));
            }
        }

        private VirtualInputDirectory GetVirtualInputDirectory(string path)
        {
            FileSystem fileSystem = new FileSystem();
            fileSystem.RootPath = "/a";
            fileSystem.InputPaths.Add("b");
            fileSystem.InputPaths.Add("alt:///foo");
            fileSystem.FileProviders.Add(NormalizedPath.DefaultFileProvider.Scheme, GetFileProviderA());
            fileSystem.FileProviders.Add("alt", GetFileProviderB());
            return new VirtualInputDirectory(fileSystem, path);
        }

        private IFileProvider GetFileProviderA()
        {
            TestFileProvider fileProvider = new TestFileProvider();

            fileProvider.AddDirectory("/a");
            fileProvider.AddDirectory("/a/b");
            fileProvider.AddDirectory("/a/b/c");
            fileProvider.AddDirectory("/a/b/c/1");
            fileProvider.AddDirectory("/a/b/d");
            fileProvider.AddDirectory("/a/x");
            fileProvider.AddDirectory("/a/y");
            fileProvider.AddDirectory("/a/y/z");

            fileProvider.AddFile("/a/b/c/foo.txt");
            fileProvider.AddFile("/a/b/c/baz.txt");
            fileProvider.AddFile("/a/b/c/1/2.txt");
            fileProvider.AddFile("/a/b/d/baz.txt");
            fileProvider.AddFile("/a/x/bar.txt");

            return fileProvider;
        }

        private IFileProvider GetFileProviderB()
        {
            TestFileProvider fileProvider = new TestFileProvider();

            fileProvider.AddDirectory("/foo");
            fileProvider.AddDirectory("/foo/a");
            fileProvider.AddDirectory("/foo/a/b");
            fileProvider.AddDirectory("/foo/c");
            fileProvider.AddDirectory("/bar");

            fileProvider.AddFile("/foo/baz.txt");
            fileProvider.AddFile("/foo/c/baz.txt");
            fileProvider.AddFile("/bar/baz.txt");

            return fileProvider;
        }
    }
}
