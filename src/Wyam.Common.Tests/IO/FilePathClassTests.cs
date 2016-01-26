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
    public class FilePathClassTests : BaseFixture
    {
        public class HasExtensionPropertyTests : FilePathClassTests
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

        public class GetExtensionMethodTests : FilePathClassTests
        {
            [Test]
            [TestCase("assets/shaders/basic.frag", ".frag")]
            [TestCase("assets/shaders/basic.frag/test.vert", ".vert")]
            [TestCase("assets/shaders/basic", null)]
            [TestCase("assets/shaders/basic.frag/test", null)]
            public void CanGetExtension(string fullPath, string expected)
            {
                // Given, When
                FilePath result = new FilePath(fullPath);
                string extension = result.GetExtension();

                // Then
                Assert.AreEqual(expected, extension);
            }
        }

        public class GetDirectoryMethodTests : FilePathClassTests
        {
            [Test]
            public void CanGetDirectoryForFilePath()
            {
                // Given, When
                FilePath path = new FilePath("temp/hello.txt");
                DirectoryPath directory = path.GetDirectory();

                // Then
                Assert.AreEqual("temp", directory.FullPath);
            }

            [Test]
            public void CanGetDirectoryForFilePathInRoot()
            {
                // Given, When
                FilePath path = new FilePath("hello.txt");
                DirectoryPath directory = path.GetDirectory();

                // Then
                Assert.AreEqual(string.Empty, directory.FullPath);
            }
        }

        public class ChangeExtensionMethodTests : FilePathClassTests
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

        public class AppendExtensionMethodTests : FilePathClassTests
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

        public class GetFilenameMethodTests : FilePathClassTests
        {
            [Test]
            public void Can_Get_Filename_From_Path()
            {
                // Given
                FilePath path = new FilePath("/input/test.txt");

                // When
                FilePath result = path.GetFilename();

                // Then
                Assert.AreEqual("test.txt", result.FullPath);
            }
        }

        public class GetFilenameWithoutExtensionMethodTests : FilePathClassTests
        {
            [Test]
            [TestCase("/input/test.txt", "test")]
            [TestCase("/input/test", "test")]
            public void Should_Return_Filename_Without_Extension_From_Path(string fullPath, string expected)
            {
                // Given
                FilePath path = new FilePath(fullPath);

                // When
                FilePath result = path.GetFilenameWithoutExtension();

                // Then
                Assert.AreEqual(expected, result.FullPath);
            }
        }
    }
}
