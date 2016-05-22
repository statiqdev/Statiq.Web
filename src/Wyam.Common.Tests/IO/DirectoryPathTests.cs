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
        public class NamePropertyTests : DirectoryPathTests
        {
            [Test]
            [TestCase("/a/b", "b")]
            [TestCase("/a/b/", "b")]
            [TestCase("/a/b/../c", "c")]
            [TestCase("/a/b/..", "..")]
            [TestCase("/a", "a")]
            [TestCase("/", "/")]
#if !UNIX
            [TestCase("C:/", "C:")]
            [TestCase("C:", "C:")]
            [TestCase("C:/Data", "Data")]
            [TestCase("C:/Data/Work", "Work")]
            [TestCase("C:/Data/Work/file.txt", "file.txt")]
#endif
            public void ShouldReturnDirectoryName(string directoryPath, string name)
            {
                // Given
                DirectoryPath path = new DirectoryPath(directoryPath);

                // When
                string result = path.Name;

                // Then
                Assert.AreEqual(name, result);
            }
        }

        public class ParentPropertyTests : FilePathTests
        {
            [Test]
            [TestCase("/a/b", "/a")]
            [TestCase("/a/b/", "/a")]
            [TestCase("/a/b/../c", "/a/b/..")]
            [TestCase("/a", "/")]
#if !UNIX
            [TestCase("C:/a/b", "C:/a")]
            [TestCase("C:/a", "C:/")]
#endif
            public void ReturnsParent(string directoryPath, string expected)
            {
                // Given
                DirectoryPath path = new DirectoryPath(directoryPath);

                // When
                DirectoryPath parent = path.Parent;

                // Then
                Assert.AreEqual(expected, parent.FullPath);
            }

            [TestCase(".")]
            [TestCase("/")]
            [TestCase("a")]
#if !UNIX
            [TestCase("C:")]
#endif
            public void RootDirectoryReturnsNullParent(string directoryPath)
            {
                // Given
                DirectoryPath path = new DirectoryPath(directoryPath);

                // When
                DirectoryPath parent = path.Parent;

                // Then
                Assert.IsNull(parent);
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
            [TestCase("c:/", "c:/simple.frag", "c:/simple.frag")]
            [TestCase("c:/", "c:/test/simple.frag", "c:/simple.frag")]
            [TestCase("c:/assets/shaders/", "test/simple.frag", "c:/assets/shaders/simple.frag")]
            [TestCase("c:/", "test/simple.frag", "c:/simple.frag")]
#endif
            [TestCase("assets/shaders", "simple.frag", "assets/shaders/simple.frag")]
            [TestCase("assets/shaders/", "simple.frag", "assets/shaders/simple.frag")]
            [TestCase("/assets/shaders/", "simple.frag", "/assets/shaders/simple.frag")]
            [TestCase("assets/shaders", "test/simple.frag", "assets/shaders/simple.frag")]
            [TestCase("assets/shaders", "/test/simple.frag", "assets/shaders/simple.frag")]
            [TestCase("assets/shaders/", "test/simple.frag", "assets/shaders/simple.frag")]
            [TestCase("assets/shaders/", "/test/simple.frag", "assets/shaders/simple.frag")]
            [TestCase("/assets/shaders/", "test/simple.frag", "/assets/shaders/simple.frag")]
            [TestCase("/assets/shaders/", "/test/simple.frag", "/assets/shaders/simple.frag")]
            public void ShouldCombinePaths(string first, string second, string expected)
            {
                // Given
                DirectoryPath path = new DirectoryPath(first);

                // When
                FilePath result = path.GetFilePath(new FilePath(second));

                // Then
                Assert.AreEqual(expected, result.FullPath);
            }

            [Test]
#if !UNIX
            [TestCase("first:///", "c:/assets/shaders/", null, "simple.frag")]
            [TestCase("first:///", "c:/", "second:///", "c:/simple.frag")]
#endif
            [TestCase(null, "assets/shaders", null, "simple.frag")]
            [TestCase("first:///", "/assets/shaders/", null, "simple.frag")]
            [TestCase(null, "assets/shaders", "second:///", "/simple.frag")]
            [TestCase("first:///", "/assets/shaders/", "second:///", "/simple.frag")]
            public void FileProviderFromDirectoryPathIsUsed(string firstProvider, string firstPath, string secondProvider, string secondPath)
            {
                // Given
                DirectoryPath path = new DirectoryPath(firstProvider == null ? null : new Uri(firstProvider), firstPath);

                // When
                FilePath result = path.GetFilePath(new FilePath(secondProvider == null ? null : new Uri(secondProvider), secondPath));

                // Then
                Assert.AreEqual(firstProvider == null ? null : new Uri(firstProvider), result.FileProvider);
            }
        }

        public class RootRelativePropertyTests : FilePathTests
        {
            [TestCase(@"\a\b\c", "a/b/c")]
            [TestCase("/a/b/c", "a/b/c")]
            [TestCase("a/b/c", "a/b/c")]
            [TestCase(@"a\b\c", "a/b/c")]
            [TestCase("foo.txt", "foo.txt")]
            [TestCase("foo", "foo")]
#if !UNIX
            [TestCase(@"c:\a\b\c", "a/b/c")]
            [TestCase("c:/a/b/c", "a/b/c")]
#endif
            public void ShouldReturnRootRelativePath(string fullPath, string expected)
            {
                // Given
                DirectoryPath path = new DirectoryPath(fullPath);

                // When
                DirectoryPath rootRelative = path.RootRelative;

                // Then
                Assert.AreEqual(expected, rootRelative.FullPath);
            }

            [TestCase(@"\a\b\c")]
            [TestCase("/a/b/c")]
            [TestCase("a/b/c")]
            [TestCase(@"a\b\c")]
            [TestCase("foo.txt")]
            [TestCase("foo")]
#if !UNIX
            [TestCase(@"c:\a\b\c")]
            [TestCase("c:/a/b/c")]
#endif
            public void ShouldReturnSelfForExplicitRelativePath(string fullPath)
            {
                // Given
                DirectoryPath path = new DirectoryPath(fullPath, PathKind.Relative);

                // When
                DirectoryPath rootRelative = path.RootRelative;

                // Then
                Assert.AreEqual(path.FullPath, rootRelative.FullPath);
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
            [TestCase("c:/assets/shaders/", "simple.frag", "c:/assets/shaders/simple.frag")]
            [TestCase("c:/", "simple.frag", "c:/simple.frag")]
            [TestCase("c:/assets/shaders/", "test/simple.frag", "c:/assets/shaders/test/simple.frag")]
            [TestCase("c:/", "test/simple.frag", "c:/test/simple.frag")]
            [TestCase("c:/", "c:/test/simple.frag", "c:/test/simple.frag")]
#endif
            [TestCase("assets/shaders", "simple.frag", "assets/shaders/simple.frag")]
            [TestCase("assets/shaders/", "simple.frag", "assets/shaders/simple.frag")]
            [TestCase("/assets/shaders/", "simple.frag", "/assets/shaders/simple.frag")]
            [TestCase("assets/shaders", "test/simple.frag", "assets/shaders/test/simple.frag")]
            [TestCase("assets/shaders/", "test/simple.frag", "assets/shaders/test/simple.frag")]
            [TestCase("/assets/shaders/", "test/simple.frag", "/assets/shaders/test/simple.frag")]
            [TestCase("assets", "/other/asset.txt", "/other/asset.txt")]
            [TestCase(".", "asset.txt", "asset.txt")]
            [TestCase(".", "other/asset.txt", "other/asset.txt")]
            [TestCase(".", "/other/asset.txt", "/other/asset.txt")]
            public void ShouldCombinePaths(string first, string second, string expected)
            {
                // Given
                DirectoryPath path = new DirectoryPath(first);

                // When
                FilePath result = path.CombineFile(new FilePath(second));

                // Then
                Assert.AreEqual(expected, result.FullPath);
            }

            [Test]
#if !UNIX
            [TestCase("c:/assets/shaders/", "simple.frag")]
#endif
            [TestCase("/assets/shaders/", "simple.frag")]
            public void CombiningWithRelativePathKeepsFirstProvider(string first, string second)
            {
                // Given
                DirectoryPath path = new DirectoryPath(new Uri("foo:///"), first);

                // When
                FilePath result = path.CombineFile(new FilePath(second));

                // Then
                Assert.AreEqual(new Uri("foo:///"), result.FileProvider);
            }

            [Test]
#if !UNIX
            [TestCase("c:/assets/shaders/", "c:/simple.frag")]
#endif
            [TestCase("/assets/shaders/", "/simple.frag")]
            public void CombiningWithAbsolutePathKeepsSecondProvider(string first, string second)
            {
                // Given
                DirectoryPath path = new DirectoryPath(new Uri("first:///"), first);

                // When
                FilePath result = path.CombineFile(new FilePath(new Uri("second:///"), second));

                // Then
                Assert.AreEqual(new Uri("second:///"), result.FileProvider);
            }
        }

        public class CombineMethodTests : DirectoryPathTests
        {
            [Test]
#if !UNIX
            [TestCase("c:/assets/shaders/", "simple", "c:/assets/shaders/simple")]
            [TestCase("c:/", "simple", "c:/simple")]
            [TestCase("c:/assets/shaders/", "c:/simple", "c:/simple")]
#endif
            [TestCase("assets/shaders", "simple", "assets/shaders/simple")]
            [TestCase("assets/shaders/", "simple", "assets/shaders/simple")]
            [TestCase("/assets/shaders/", "simple", "/assets/shaders/simple")]
            [TestCase("assets", "/other/assets", "/other/assets")]
            public void ShouldCombinePaths(string first, string second, string expected)
            {
                // Given
                DirectoryPath path = new DirectoryPath(first);

                // When
                DirectoryPath result = path.Combine(new DirectoryPath(second));

                // Then
                Assert.AreEqual(expected, result.FullPath);
            }

            [Test]
#if !UNIX
            [TestCase("c:/assets/shaders/", "simple")]
#endif
            [TestCase("/assets/shaders/", "simple")]
            public void CombiningWithRelativePathKeepsFirstProvider(string first, string second)
            {
                // Given
                DirectoryPath path = new DirectoryPath(new Uri("foo:///"), first);

                // When
                DirectoryPath result = path.Combine(new DirectoryPath(second));

                // Then
                Assert.AreEqual(new Uri("foo:///"), result.FileProvider);
            }

            [Test]
#if !UNIX
            [TestCase("c:/assets/shaders/", "c:/simple")]
#endif
            [TestCase("/assets/shaders/", "/simpl")]
            public void CombiningWithAbsolutePathKeepsSecondProvider(string first, string second)
            {
                // Given
                DirectoryPath path = new DirectoryPath(new Uri("first:///"), first);

                // When
                DirectoryPath result = path.Combine(new DirectoryPath(new Uri("second:///"), second));

                // Then
                Assert.AreEqual(new Uri("second:///"), result.FileProvider);
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

        public class CollapseMethodTests : FilePathTests
        {
            [Test]
            [TestCase("/a/b/c/../d", "/a/b/d")]
#if !UNIX
            [TestCase("c:/a/b/c/../d", "c:/a/b/d")]
#endif
            public void ShouldCollapse(string fullPath, string expected)
            {
                // Given
                DirectoryPath directoryPath = new DirectoryPath(fullPath);

                // When
                DirectoryPath path = directoryPath.Collapse();

                // Then
                Assert.AreEqual(expected, path.FullPath);
            }

            [Test]
            public void CollapseRetainsProvider()
            {
                // Given
                DirectoryPath directoryPath = new DirectoryPath(new Uri("foo:///"), "/a/b/../c");

                // When
                DirectoryPath path = directoryPath.Collapse();

                // Then
                Assert.AreEqual("/a/c", path.FullPath);
                Assert.AreEqual(new Uri("foo:///"), path.FileProvider);
            }
        }
    }
}
