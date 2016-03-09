using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.IO;
using Wyam.Testing;

namespace Wyam.Common.Tests.IO
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class FilePathTests : BaseFixture
    {
        public class HasExtensionPropertyTests : FilePathTests
        {
            [Test]
            [TestCase("assets/shaders/basic.txt", true)]
            [TestCase("assets/shaders/basic", false)]
            [TestCase("assets/shaders/basic/", false)]
            public void CanSeeIfAPathHasAnExtension(string fullPath, bool expected)
            {
                // Given, When
                FilePath path = new FilePath(fullPath);

                // Then
                Assert.AreEqual(expected, path.HasExtension);
            }
        }

        public class ExtensionPropertyTests : FilePathTests
        {
            [Test]
            [TestCase("assets/shaders/basic.frag", ".frag")]
            [TestCase("assets/shaders/basic.frag/test.vert", ".vert")]
            [TestCase("assets/shaders/basic", null)]
            [TestCase("assets/shaders/basic.frag/test", null)]
            public void CanGetExtension(string fullPath, string expected)
            {
                // Given
                FilePath result = new FilePath(fullPath);

                // When
                string extension = result.Extension;

                // Then
                Assert.AreEqual(expected, extension);
            }
        }

        public class DirectoryPropertyTests : FilePathTests
        {
            [Test]
            public void CanGetDirectoryForFilePath()
            {
                // Given
                FilePath path = new FilePath("temp/hello.txt");

                // When
                DirectoryPath directory = path.Directory;

                // Then
                Assert.AreEqual("temp", directory.FullPath);
            }

            [Test]
            public void CanGetDirectoryForFilePathInRoot()
            {
                // Given
                FilePath path = new FilePath("hello.txt");

                // When
                DirectoryPath directory = path.Directory;

                // Then
                Assert.AreEqual(".", directory.FullPath);
            }
        }

        public class ChangeExtensionMethodTests : FilePathTests
        {
            [Test]
            public void CanChangeExtensionOfPath()
            {
                // Given
                FilePath path = new FilePath("temp/hello.txt");

                // When
                path = path.ChangeExtension(".dat");

                // Then
                Assert.AreEqual("temp/hello.dat", path.ToString());
            }
        }

        public class AppendExtensionMethodTests : FilePathTests
        {
            [Test]
            public void ShouldThrowIfExtensionIsNull()
            {
                // Given
                FilePath path = new FilePath("temp/hello.txt");

                // When
                TestDelegate test = () => path.AppendExtension(null);

                // Then
                Assert.Throws<ArgumentNullException>(test);
            }

            [Test]
            [TestCase("dat", "temp/hello.txt.dat")]
            [TestCase(".dat", "temp/hello.txt.dat")]
            public void CanAppendExtensionToPath(string extension, string expected)
            {
                // Given
                FilePath path = new FilePath("temp/hello.txt");

                // When
                path = path.AppendExtension(extension);

                // Then
                Assert.AreEqual(expected, path.ToString());
            }
        }

        public class FileNamePropertyTests : FilePathTests
        {
            [Test]
            public void CanGetFilenameFromPath()
            {
                // Given
                FilePath path = new FilePath("/input/test.txt");

                // When
                FilePath result = path.FileName;

                // Then
                Assert.AreEqual("test.txt", result.FullPath);
            }

            [Test]
            public void NullProviderSetForReturnPath()
            {
                // Given
                FilePath path = new FilePath("foo", "/input/test.txt");

                // When
                FilePath result = path.FileName;

                // Then
                Assert.AreEqual(null, result.Provider);
            }
        }

        public class FileNameWithoutExtensionPropertyTests : FilePathTests
        {
            [Test]
            [TestCase("/input/test.txt", "test")]
            [TestCase("/input/test", "test")]
            public void ShouldReturnFilenameWithoutExtensionFromPath(string fullPath, string expected)
            {
                // Given
                FilePath path = new FilePath(fullPath);

                // When
                FilePath result = path.FileNameWithoutExtension;

                // Then
                Assert.AreEqual(expected, result.FullPath);
            }

            [Test]
            public void NullProviderSetForReturnPath()
            {
                // Given
                FilePath path = new FilePath("foo", "/input/test.txt");

                // When
                FilePath result = path.FileNameWithoutExtension;

                // Then
                Assert.AreEqual(null, result.Provider);
            }
        }
    }
}
