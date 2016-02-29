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
    public class DirectoryPathTests : BaseFixture
    {
        public class GetDirectoryNameMethodTests : DirectoryPathTests
        {
            [Test]
            [TestCase("C:/Data", "Data")]
            [TestCase("C:/Data/Work", "Work")]
            [TestCase("C:/Data/Work/file.txt", "file.txt")]
            public void ShouldReturnDirectoryName(string directoryPath, string name)
            {
                // Given
                DirectoryPath path = new DirectoryPath(directoryPath);

                // When
                string result = path.GetDirectoryName();

                // Then
                Assert.AreEqual(name, result);
            }
        }

        public class GetFilePathMethodTests : DirectoryPathTests
        {
            [Test]
            public void ShouldThrowIfPathIsNull()
            {
                // Given
                DirectoryPath path = new DirectoryPath("assets");

                // When
                TestDelegate test = () => path.GetFilePath(null);

                // Then
                Assert.Throws<ArgumentNullException>(test);
            }

            [Test]
#if !UNIX
            [TestCase("c:/assets/shaders/", "simple.frag", "c:/assets/shaders/simple.frag")]
            [TestCase("c:/", "simple.frag", "c:/simple.frag")]
            [TestCase("c:/assets/shaders/", "test/simple.frag", "c:/assets/shaders/simple.frag")]
            [TestCase("c:/", "test/simple.frag", "c:/simple.frag")]
#endif
            [TestCase("assets/shaders", "simple.frag", "assets/shaders/simple.frag")]
            [TestCase("assets/shaders/", "simple.frag", "assets/shaders/simple.frag")]
            [TestCase("/assets/shaders/", "simple.frag", "/assets/shaders/simple.frag")]
            [TestCase("assets/shaders", "test/simple.frag", "assets/shaders/simple.frag")]
            [TestCase("assets/shaders/", "test/simple.frag", "assets/shaders/simple.frag")]
            [TestCase("/assets/shaders/", "test/simple.frag", "/assets/shaders/simple.frag")]
            public void ShouldCombinePaths(string first, string second, string expected)
            {
                // Given
                DirectoryPath path = new DirectoryPath(first);

                // When
                FilePath result = path.GetFilePath(new FilePath(second));

                // Then
                Assert.AreEqual(expected, result.FullPath);
            }
        }

        public class CombineFileMethodTests : DirectoryPathTests
        {
            [Test]
            public void ShouldThrowIfPathIsNull()
            {
                // Given
                DirectoryPath path = new DirectoryPath("assets");

                // When
                TestDelegate test = () => path.CombineFile(null);

                // Then
                Assert.Throws<ArgumentNullException>(test);
            }

            [Test]
#if !UNIX
            [TestCase("c:/assets/shaders/", "simple.frag", "c:/assets/shaders/simple.frag", "first")]
            [TestCase("c:/", "simple.frag", "c:/simple.frag", "first")]
            [TestCase("c:/assets/shaders/", "test/simple.frag", "c:/assets/shaders/test/simple.frag", "first")]
            [TestCase("c:/", "test/simple.frag", "c:/test/simple.frag", "first")]
            [TestCase("c:/", "c:/test/simple.frag", "c:/test/simple.frag", "second")]
#endif
            [TestCase("assets/shaders", "simple.frag", "assets/shaders/simple.frag", "first")]
            [TestCase("assets/shaders/", "simple.frag", "assets/shaders/simple.frag", "first")]
            [TestCase("/assets/shaders/", "simple.frag", "/assets/shaders/simple.frag", "first")]
            [TestCase("assets/shaders", "test/simple.frag", "assets/shaders/test/simple.frag", "first")]
            [TestCase("assets/shaders/", "test/simple.frag", "assets/shaders/test/simple.frag", "first")]
            [TestCase("/assets/shaders/", "test/simple.frag", "/assets/shaders/test/simple.frag", "first")]
            [TestCase("assets", "/other/asset.txt", "/other/asset.txt", "second")]
            public void ShouldCombinePaths(string first, string second, string expected, string expectedProvider)
            {
                // Given
                DirectoryPath path = new DirectoryPath("first", first);

                // When
                FilePath result = path.CombineFile(new FilePath("second", second));

                // Then
                Assert.AreEqual(expected, result.FullPath);
                Assert.AreEqual(expectedProvider, result.Provider);
            }
        }

        public class CombineMethodTests : DirectoryPathTests
        {
            [Test]
#if !UNIX
            [TestCase("c:/assets/shaders/", "simple", "c:/assets/shaders/simple", "first")]
            [TestCase("c:/", "simple", "c:/simple", "first")]
            [TestCase("c:/assets/shaders/", "c:/simple", "c:/simple", "second")]
#endif
            [TestCase("assets/shaders", "simple", "assets/shaders/simple", "first")]
            [TestCase("assets/shaders/", "simple", "assets/shaders/simple", "first")]
            [TestCase("/assets/shaders/", "simple", "/assets/shaders/simple", "first")]
            [TestCase("assets", "/other/assets", "/other/assets", "second")]
            public void ShouldCombinePaths(string first, string second, string expected, string expectedProvider)
            {
                // Given
                DirectoryPath path = new DirectoryPath("first", first);

                // When
                var result = path.Combine(new DirectoryPath("second", second));

                // Then
                Assert.AreEqual(expected, result.FullPath);
                Assert.AreEqual(expectedProvider, result.Provider);
            }

            [Test]
            public void ShouldThrowIfPathIsNull()
            {
                // Given
                DirectoryPath path = new DirectoryPath("assets");

                // When
                TestDelegate test = () => path.Combine(null);

                // Then
                Assert.Throws<ArgumentNullException>(test);
            }
        }
    }
}
