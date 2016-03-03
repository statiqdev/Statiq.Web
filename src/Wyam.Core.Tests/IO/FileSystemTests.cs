using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Wyam.Common.IO;
using Wyam.Core.IO;
using Wyam.Testing;

namespace Wyam.Core.Tests.IO
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class FileSystemTests : BaseFixture
    {
        public class ConstructorTests : FileSystemTests
        {
            [Test]
            public void AddsDefaultInputPath()
            {
                // Given, When
                FileSystem fileSystem = new FileSystem();

                // Then
                CollectionAssert.AreEquivalent(new [] { "input" }, fileSystem.InputPaths.Select(x => x.FullPath));
            }     
        }

        public class RootPathPropertyTests : FileSystemTests
        {
            [Test]
            public void SetThrowsForNullValue()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

                // When, Then
                Assert.Throws<ArgumentNullException>(() => fileSystem.RootPath = null);
            }

            [Test]
            public void SetThrowsForRelativePath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

                // When, Then
                Assert.Throws<ArgumentException>(() => fileSystem.RootPath = "foo");
            }

            [Test]
            public void CanSet()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

                // When
                fileSystem.RootPath = "/foo/bar";

                // Then
                Assert.AreEqual("/foo/bar", fileSystem.RootPath.FullPath);
            }
        }

        public class OutputPathPropertyTests : FileSystemTests
        {
            [Test]
            public void SetThrowsForNullValue()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

                // When, Then
                Assert.Throws<ArgumentNullException>(() => fileSystem.OutputPath = null);
            }

            [Test]
            public void CanSet()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

                // When
                fileSystem.OutputPath = "/foo/bar";

                // Then
                Assert.AreEqual("/foo/bar", fileSystem.OutputPath.FullPath);
            }
        }

        public class GetInputFileMethodTests : FileSystemTests
        {
            [Test]
            [TestCase("foo.txt", "/a/b/c/foo.txt")]
            [TestCase("bar.txt", "/a/x/bar.txt")]
            [TestCase("baz.txt", "/a/y/baz.txt")]
            [TestCase("/a/b/c/foo.txt", "/a/b/c/foo.txt")]
            [TestCase("/z/baz.txt", "/z/baz.txt")]
            public void ReturnsInputFile(string input, string expected)
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b/c");
                fileSystem.InputPaths.Add("b/d");
                fileSystem.InputPaths.Add("x");
                fileSystem.InputPaths.Add("y");
                fileSystem.FileProviders.Add(string.Empty, GetFileProvider());

                // When
                IFile result = fileSystem.GetInputFile(input);

                // Then
                Assert.AreEqual(expected, result.Path.FullPath);
            }
        }

        public class GetInputDirectoryMethodTests : FileSystemTests
        {
            [Test]
            public void ReturnsVirtualInputDirectoryForRelativePath()
            {
                // Given 
                FileSystem fileSystem = new FileSystem();

                // When
                IDirectory result = fileSystem.GetInputDirectory("A/B/C");

                // Then
                Assert.IsInstanceOf<VirtualInputDirectory>(result);
                Assert.AreEqual("A/B/C", result.Path.FullPath);
            }

            [Test]
            public void ReturnsDirectoryForAbsolutePath()
            {
                // Given 
                FileSystem fileSystem = new FileSystem();

                // When
                IDirectory result = fileSystem.GetInputDirectory("/A/B/C");

                // Then
                Assert.AreEqual("/A/B/C", result.Path.FullPath);
            }
        }

        public class GetInputDirectoriesMethodTests : FileSystemTests
        {
            [Test]
            public void ReturnsCombinedInputDirectories()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b/c");
                fileSystem.InputPaths.Add("b/d");
                fileSystem.InputPaths.Add("x");
                fileSystem.InputPaths.Add("y");
                fileSystem.FileProviders.Add(string.Empty, GetFileProvider());

                // When
                IEnumerable<IDirectory> result = fileSystem.GetInputDirectories();

                // Then
                CollectionAssert.AreEquivalent(new []
                {
                    "/a/input",
                    "/a/b/c",
                    "/a/b/d",
                    "/a/x",
                    "/a/y"
                }, result.Select(x => x.Path.FullPath));
            }
        }

        public class GetFileProviderMethodTests : FileSystemTests
        {
            [Test]
            public void ThrowsForNullPath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

                // When, Then
                Assert.Throws<ArgumentNullException>(() => fileSystem.GetFileProvider(null));
            }

            [Test]
            public void ThrowsForRelativeDirectoryPath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                DirectoryPath relativePath = new DirectoryPath("A/B/C");

                // When, Then
                Assert.Throws<ArgumentException>(() => fileSystem.GetFileProvider(relativePath));
            }

            [Test]
            public void ThrowsForRelativeFilePath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                FilePath relativePath = new FilePath("A/B/C.txt");

                // When, Then
                Assert.Throws<ArgumentException>(() => fileSystem.GetFileProvider(relativePath));
            }

            [Test]
            public void ReturnsDefaultProviderForDirectoryPath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                IFileProvider fooProvider = Substitute.For<IFileProvider>();
                fileSystem.FileProviders.Add(string.Empty, defaultProvider);
                fileSystem.FileProviders.Add("foo", fooProvider);
                DirectoryPath path = new DirectoryPath("/a/b/c");

                // When
                IFileProvider result = fileSystem.GetFileProvider(path);

                // Then
                Assert.AreEqual(defaultProvider, result);
            }

            [Test]
            public void ReturnsDefaultProviderForFilePath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                IFileProvider fooProvider = Substitute.For<IFileProvider>();
                fileSystem.FileProviders.Add(string.Empty, defaultProvider);
                fileSystem.FileProviders.Add("foo", fooProvider);
                FilePath path = new FilePath("/a/b/c.txt");

                // When
                IFileProvider result = fileSystem.GetFileProvider(path);

                // Then
                Assert.AreEqual(defaultProvider, result);
            }

            [Test]
            public void ReturnsOtherProviderForDirectoryPath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                IFileProvider fooProvider = Substitute.For<IFileProvider>();
                fileSystem.FileProviders.Add(string.Empty, defaultProvider);
                fileSystem.FileProviders.Add("foo", fooProvider);
                DirectoryPath path = new DirectoryPath("foo", "/a/b/c");

                // When
                IFileProvider result = fileSystem.GetFileProvider(path);

                // Then
                Assert.AreEqual(fooProvider, result);
            }

            [Test]
            public void ReturnsOtherProviderForFilePath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                IFileProvider defaultProvider = Substitute.For<IFileProvider>();
                IFileProvider fooProvider = Substitute.For<IFileProvider>();
                fileSystem.FileProviders.Add(string.Empty, defaultProvider);
                fileSystem.FileProviders.Add("foo", fooProvider);
                FilePath path = new FilePath("foo", "/a/b/c.txt");

                // When
                IFileProvider result = fileSystem.GetFileProvider(path);

                // Then
                Assert.AreEqual(fooProvider, result);
            }

            [Test]
            public void ThrowsIfProviderNotFoundForDirectoryPath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                DirectoryPath path = new DirectoryPath("foo", "/a/b/c");

                // When, Then
                Assert.Throws<KeyNotFoundException>(() => fileSystem.GetFileProvider(path));
            }

            [Test]
            public void ThrowsIfProviderNotFoundForFilePath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();
                FilePath path = new FilePath("foo", "/a/b/c.txt");

                // When, Then
                Assert.Throws<KeyNotFoundException>(() => fileSystem.GetFileProvider(path));
            }
        }
        
        private IFileProvider GetFileProvider()
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
                "/a/x/bar.txt"
            };
            IFileProvider fileProvider = Substitute.For<IFileProvider>();
            fileProvider.GetDirectory(Arg.Any<DirectoryPath>())
                .Returns(x =>
                {
                    string path = ((DirectoryPath) x[0]).FullPath;
                    return GetDirectory(path, directories.Contains(path));
                });
            fileProvider.GetFile(Arg.Any<FilePath>())
                .Returns(x =>
                {
                    string path = ((FilePath) x[0]).FullPath;
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

        private IDirectory GetDirectory(string path, bool exists)
        {
            IDirectory directory = Substitute.For<IDirectory>();
            directory.Path.Returns(new DirectoryPath(path));
            directory.Exists.Returns(exists);
            return directory;
        }
    }
}
