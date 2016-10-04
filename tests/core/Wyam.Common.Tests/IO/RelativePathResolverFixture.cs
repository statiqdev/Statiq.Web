using System;
using NUnit.Framework;
using Wyam.Common.IO;
using Wyam.Testing;

namespace Wyam.Common.Tests.IO
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class RelativePathResolverFixture : BaseFixture
    {
        public class ResolveTests : RelativePathResolverFixture
        {
            [Test]
#if !UNIX
            [TestCase("C:/A/B/C", "C:/A/B/C", ".")]
            [TestCase("C:/", "C:/", ".")]
            [TestCase("C:/A/B/C", "C:/A/D/E", "../../D/E")]
            [TestCase("C:/A/B/C", "C:/", "../../..")]
            [TestCase("C:/A/B/C/D/E/F", "C:/A/B/C", "../../..")]
            [TestCase("C:/A/B/C", "C:/A/B/C/D/E/F", "D/E/F")]
            [TestCase("C:/A/B/C", "W:/X/Y/Z", "W:/X/Y/Z")]
            [TestCase("C:/A/B/C", "D:/A/B/C", "D:/A/B/C")]
            [TestCase("C:/A/B", "D:/E/", "D:/E")]
            [TestCase("C:/", "B:/", "B:/")]
#endif
            // Absolute
            [TestCase("/C/A/B/C", "/C/A/B/C", ".")]
            [TestCase("/C/", "/C/", ".")]
            [TestCase("/C/A/B/C", "/C/A/D/E", "../../D/E")]
            [TestCase("/C/A/B/C", "/C/", "../../..")]
            [TestCase("/C/A/B/C/D/E/F", "/C/A/B/C", "../../..")]
            [TestCase("/C/A/B/C", "/C/A/B/C/D/E/F", "D/E/F")]
            [TestCase("/C/A/B/C", "/W/X/Y/Z", "/W/X/Y/Z")]
            [TestCase("/C/A/B/C", "/D/A/B/C", "/D/A/B/C")]
            [TestCase("/C/A/B", "/D/E/", "/D/E")]
            [TestCase("/C/", "/B/", "/B")]
            [TestCase("/", "/A/B", "A/B")]
            // Relative
            [TestCase("C/A/B/C", "C/A/B/C", ".")]
            [TestCase("C/", "C/", ".")]
            [TestCase("C/A/B/C", "C/A/D/E", "../../D/E")]
            [TestCase("C/A/B/C", "C/", "../../..")]
            [TestCase("C/A/B/C/D/E/F", "C/A/B/C", "../../..")]
            [TestCase("C/A/B/C", "C/A/B/C/D/E/F", "D/E/F")]
            [TestCase("C/A/B/C", "W/X/Y/Z", "W/X/Y/Z")]
            [TestCase("C/A/B/C", "D/A/B/C", "D/A/B/C")]
            [TestCase("C/A/B", "D/E/", "D/E")]
            [TestCase("C/", "B/", "B")]
            public void ShouldReturnRelativePathWithDirectoryPath(string source, string target, string expected)
            {
                // Given
                DirectoryPath sourcePath = new DirectoryPath(source);
                DirectoryPath targetPath = new DirectoryPath(target);

                // When
                DirectoryPath relativePath = RelativePathResolver.Resolve(sourcePath, targetPath);

                // Then
                Assert.AreEqual(expected, relativePath.FullPath);
            }

            [Test]
#if !UNIX
            [TestCase("C:/A/B/C", "C:/A/B/C/hello.txt", "hello.txt")]
            [TestCase("C:/", "C:/hello.txt", "hello.txt")]
            [TestCase("C:/A/B/C", "C:/A/D/E/hello.txt", "../../D/E/hello.txt")]
            [TestCase("C:/A/B/C", "C:/hello.txt", "../../../hello.txt")]
            [TestCase("C:/A/B/C/D/E/F", "C:/A/B/C/hello.txt", "../../../hello.txt")]
            [TestCase("C:/A/B/C", "C:/A/B/C/D/E/F/hello.txt", "D/E/F/hello.txt")]
            [TestCase("C:/A/B/C", "W:/X/Y/Z/hello.txt", "W:/X/Y/Z/hello.txt")]
            [TestCase("C:/A/B/C", "D:/A/B/C/hello.txt", "D:/A/B/C/hello.txt")]
            [TestCase("C:/A/B", "D:/E/hello.txt", "D:/E/hello.txt")]
            [TestCase("C:/", "B:/hello.txt", "B:/hello.txt")]
#endif
            // Absolute
            [TestCase("/C/A/B/C", "/C/A/B/C/hello.txt", "hello.txt")]
            [TestCase("/C/", "/C/hello.txt", "hello.txt")]
            [TestCase("/C/A/B/C", "/C/A/D/E/hello.txt", "../../D/E/hello.txt")]
            [TestCase("/C/A/B/C", "/C/hello.txt", "../../../hello.txt")]
            [TestCase("/C/A/B/C/D/E/F", "/C/A/B/C/hello.txt", "../../../hello.txt")]
            [TestCase("/C/A/B/C", "/C/A/B/C/D/E/F/hello.txt", "D/E/F/hello.txt")]
            [TestCase("/C/A/B/C", "/W/X/Y/Z/hello.txt", "/W/X/Y/Z/hello.txt")]
            [TestCase("/C/A/B/C", "/D/A/B/C/hello.txt", "/D/A/B/C/hello.txt")]
            [TestCase("/C/A/B", "/D/E/hello.txt", "/D/E/hello.txt")]
            [TestCase("/C/", "/B/hello.txt", "/B/hello.txt")]
            // Relative
            [TestCase("C/A/B/C", "C/A/B/C/hello.txt", "hello.txt")]
            [TestCase("C/", "C/hello.txt", "hello.txt")]
            [TestCase("C/A/B/C", "C/A/D/E/hello.txt", "../../D/E/hello.txt")]
            [TestCase("C/A/B/C", "C/hello.txt", "../../../hello.txt")]
            [TestCase("C/A/B/C/D/E/F", "C/A/B/C/hello.txt", "../../../hello.txt")]
            [TestCase("C/A/B/C", "C/A/B/C/D/E/F/hello.txt", "D/E/F/hello.txt")]
            [TestCase("C/A/B/C", "W/X/Y/Z/hello.txt", "W/X/Y/Z/hello.txt")]
            [TestCase("C/A/B/C", "D/A/B/C/hello.txt", "D/A/B/C/hello.txt")]
            [TestCase("C/A/B", "D/E/hello.txt", "D/E/hello.txt")]
            [TestCase("C/", "B/hello.txt", "B/hello.txt")]
            public void ShouldReturnRelativePathWithFilePath(string source, string target, string expected)
            {
                // Given
                DirectoryPath sourcePath = new DirectoryPath(source);
                FilePath targetPath = new FilePath(target);

                // When
                FilePath relativePath = RelativePathResolver.Resolve(sourcePath, targetPath);

                // Then
                Assert.AreEqual(expected, relativePath.FullPath);
            }

            [Test]
            public void ShouldThrowIfSourceIsNullWithDirectoryPath()
            {
                // Given
                DirectoryPath targetPath = new DirectoryPath("/A");

                // When, Then
                Assert.Throws<ArgumentNullException>(() => RelativePathResolver.Resolve(null, targetPath));
            }

            [Test]
            public void ShouldThrowIfTargetIsNullWithDirectoryPath()
            {
                // Given
                DirectoryPath sourcePath = new DirectoryPath("/A");

                // When, Then
                Assert.Throws<ArgumentNullException>(() => RelativePathResolver.Resolve(sourcePath, (DirectoryPath)null));
            }

            [Test]
            [TestCase("/A/B", "A/B")]
            [TestCase("A/B", "/A/B")]
            public void ShouldThrowIfNotBothSameAbsoluteWithDirectoryPath(string source, string target)
            {
                // Given
                DirectoryPath sourcePath = new DirectoryPath(source);
                DirectoryPath targetPath = new DirectoryPath(target);

                // When, Then
                Assert.Throws<ArgumentException>(() => RelativePathResolver.Resolve(sourcePath, targetPath));
            }

            [Test]
            public void ShouldThrowIfSourceIsNullWithFilePath()
            {
                // Given
                FilePath targetPath = new FilePath("/A/hello.txt");

                // When, Then
                Assert.Throws<ArgumentNullException>(() => RelativePathResolver.Resolve(null, targetPath));
            }

            [Test]
            public void ShouldThrowIfTargetIsNullWithFilePath()
            {
                // Given
                DirectoryPath sourcePath = new DirectoryPath("/A");

                // When, Then
                Assert.Throws<ArgumentNullException>(() => RelativePathResolver.Resolve(sourcePath, (FilePath)null));
            }

            [Test]
            [TestCase("/A/B", "A/B/hello.txt")]
            [TestCase("A/B", "/A/B/hello.txt")]
            public void ShouldThrowIfNotBothSameAbsoluteWithFilePath(string source, string target)
            {
                // Given
                DirectoryPath sourcePath = new DirectoryPath(source);
                FilePath targetPath = new FilePath(target);

                // When, Then
                Assert.Throws<ArgumentException>(() => RelativePathResolver.Resolve(sourcePath, targetPath));
            }

            [Test]
            public void ShouldReturnTargetPathIfProvidersDontMatch()
            {
                // Given
                DirectoryPath sourcePath = new DirectoryPath("foo:///A/B");
                FilePath targetPath = new FilePath("bar", "/A/B/C/test.txt");

                // When
                FilePath resultPath = RelativePathResolver.Resolve(sourcePath, targetPath);

                // Then
                // Assert.AreEqual(targetPath, resultPath); // Uncomment and use this when NUnit is fixed
                Assert.IsTrue(targetPath.Equals(resultPath)); // Workaround for bug in NUnit with explicitly implemented interface method IEquality.Equals()
            }
        }
    }
}
