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
    public class NormalizedPathTests : BaseFixture
    {
        private class TestPath : NormalizedPath
        {
            public TestPath(string path) : base(path)
            {
            }

            public TestPath(string provider, string path) : base(provider, path)
            {
            }
        }

        public class ConstructorTests : NormalizedPathTests
        {
            [Test]
            public void ShouldThrowIfPathIsNull()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new TestPath(null));
            }

            [Test]
            public void ShouldThrowIfProviderIsSpecifiedForRelativePath()
            {
                // Given, When, Then
                Assert.Throws<ArgumentException>(() => new TestPath("foo", "Hello/World"));
            }

            [Test]
            [TestCase("")]
            [TestCase("\t ")]
            public void ShouldThrowIfPathIsEmpty(string fullPath)
            {
                // Given, When, Then
                Assert.Throws<ArgumentException>(() => new TestPath(fullPath));
            }

            [Test]
            public void CurrentDirectoryReturnsDot()
            {
                // Given, When
                TestPath path = new TestPath("./");

                // Then
                Assert.AreEqual(".", path.FullPath);
            }

            [Test]
            public void WillNormalizePathSeparators()
            {
                // Given, When
                TestPath path = new TestPath("shaders\\basic");

                // Then
                Assert.AreEqual("shaders/basic", path.FullPath);
            }

            [Test]
            public void WillTrimWhiteSpaceFromPath()
            {
                // Given, When
                TestPath path = new TestPath(" shaders/basic ");

                // Then
                Assert.AreEqual("shaders/basic", path.FullPath);
            }

            [Test]
            public void WillNotRemoveWhiteSpaceWithinPath()
            {
                // Given, When
                TestPath path = new TestPath("my awesome shaders/basic");

                // Then
                Assert.AreEqual("my awesome shaders/basic", path.FullPath);
            }
            
            [Test]
            [TestCase("/Hello/World/", "/Hello/World")]
            [TestCase("\\Hello\\World\\", "/Hello/World")]
            [TestCase("file.txt/", "file.txt")]
            [TestCase("file.txt\\", "file.txt")]
            [TestCase("Temp/file.txt/", "Temp/file.txt")]
            [TestCase("Temp\\file.txt\\", "Temp/file.txt")]
            [TestCase("http://www.foo.bar/", "http://www.foo.bar")]
            [TestCase("http://www.foo.bar/test/page.html/", "http://www.foo.bar/test/page.html")]
            public void ShouldRemoveTrailingSlashes(string value, string expected)
            {
                // Given, When
                TestPath path = new TestPath(value);

                // Then
                Assert.AreEqual(expected, path.FullPath);
            }

            [Test]
            [TestCase("\\")]
            [TestCase("/")]
            public void ShouldNotRemoveSingleTrailingSlash(string value)
            {
                // Given, When
                TestPath path = new TestPath(value);

                // Then
                Assert.AreEqual("/", path.FullPath);
            }

            [Test]
            [TestCase("./Hello/World/", "Hello/World")]
            [TestCase(".\\Hello/World/", "Hello/World")]
            [TestCase("./file.txt", "file.txt")]
            [TestCase("./Temp/file.txt", "Temp/file.txt")]
            public void ShouldRemoveRelativePrefix(string value, string expected)
            {
                // Given, When
                TestPath path = new TestPath(value);

                // Then
                Assert.AreEqual(expected, path.FullPath);
            }

            [Test]
            [TestCase("\\")]
            [TestCase("/")]
            public void ShouldNotRemoveOnlyRelativePart(string value)
            {
                // Given, When
                TestPath path = new TestPath(value);

                // Then
                Assert.AreEqual("/", path.FullPath);
            }
        }

        public class ProviderPropertyTests : NormalizedPathTests
        {
            [Test]
            [TestCase("foo", "/Hello/World", "foo")]
            [TestCase("", "/Hello/World", "")]
            [TestCase(null, "/Hello/World", "")]
            [TestCase(null, "Hello/World", null)]
            [TestCase("", "Hello/World", null)]
            public void ShouldReturnProvider(string provider, string pathName, string expectedProvider)
            {
                // Given, W
                TestPath path = new TestPath(provider, pathName);

                // Then
                Assert.AreEqual(expectedProvider, path.Provider);

            }
        }

        public class SegmentsPropertyTests : NormalizedPathTests
        {
            [Test]
            [TestCase("Hello/World")]
            [TestCase("/Hello/World")]
            [TestCase("/Hello/World/")]
            [TestCase("./Hello/World/")]
            public void ShouldReturnSegmentsOfPath(string pathName)
            {
                // Given, When
                TestPath path = new TestPath(pathName);

                // Then
                Assert.AreEqual(2, path.Segments.Length);
                Assert.AreEqual("Hello", path.Segments[0]);
                Assert.AreEqual("World", path.Segments[1]);
            }
        }

        public class FullPathPropertyTests : NormalizedPathTests
        {
            [Test]
            public void ShouldReturnFullPath()
            {
                // Given, When
                const string expected = "shaders/basic";
                TestPath path = new TestPath(expected);

                // Then
                Assert.AreEqual(expected, path.FullPath);
            }
        }

        public class IsRelativePropertyTests : NormalizedPathTests
        {
            [Test]
            [TestCase("assets/shaders", true)]
            [TestCase("assets/shaders/basic.frag", true)]
            [TestCase("/assets/shaders", false)]
            [TestCase("/assets/shaders/basic.frag", false)]
            public void ShouldReturnWhetherOrNotAPathIsRelative(string fullPath, bool expected)
            {
                // Given, When
                var path = new TestPath(fullPath);

                // Then
                Assert.AreEqual(expected, path.IsRelative);
            }

#if !UNIX
            [Test]
            [TestCase("c:/assets/shaders", false)]
            [TestCase("c:/assets/shaders/basic.frag", false)]
            [TestCase("c:/", false)]
            [TestCase("c:", false)]
            public void ShouldReturnWhetherOrNotAPathIsRelativeOnWindows(string fullPath, bool expected)
            {
                // Given, When
                var path = new TestPath(fullPath);

                // Then
                Assert.AreEqual(expected, path.IsRelative);
            }
#endif
        }
        
        public class ToStringMethodTests : NormalizedPathTests
        {
            [Test]
            public void Should_Return_The_Full_Path()
            {
                // Given, When
                var path = new TestPath("temp/hello");

                // Then
                Assert.AreEqual("temp/hello", path.ToString());
            }
        }

        public class CollapseMethodTests : NormalizedPathTests
        {
            [Test]
            public void ShouldThrowIfPathIsNull()
            {
                // Given, When
                TestDelegate test = () => NormalizedPath.Collapse(null);

                // Then
                Assert.Throws<ArgumentNullException>(test);
            }

            [Test]
            [TestCase("hello/temp/test/../../world", "hello/world")]
            [TestCase("hello/temp/../temp2/../world", "hello/world")]
            [TestCase("/hello/temp/test/../../world", "/hello/world")]
            [TestCase("/hello/../../../../../../temp", "/temp")]  // Stop collapsing when root is reached
            [TestCase(".", ".")]
            [TestCase("/.", ".")]
            [TestCase("./a", "a")]
            [TestCase("./..", ".")]
            [TestCase("a/./b", "a/b")]
            [TestCase("/a/./b", "/a/b")]
            [TestCase("a/b/.", "a/b")]
            [TestCase("/a/b/.", "/a/b")]
            [TestCase("/./a/b", "/a/b")]
#if !UNIX
            [TestCase("c:/hello/temp/test/../../world", "c:/hello/world")]
            [TestCase("c:/../../../../../../temp", "c:/temp")]
#endif
            public void ShouldCollapseDirectoryPath(string fullPath, string expected)
            {
                // Given
                DirectoryPath directoryPath = new DirectoryPath(fullPath);

                // When
                string path = NormalizedPath.Collapse(directoryPath);

                // Then
                Assert.AreEqual(expected, path);
            }

            [Test]
            [TestCase("/a/b/c/../d/baz.txt", "/a/b/d/baz.txt")]
#if !UNIX
            [TestCase("c:/a/b/c/../d/baz.txt", "c:/a/b/d/baz.txt")]
#endif
            public void ShouldCollapseFilePath(string fullPath, string expected)
            {
                // Given
                FilePath filePath = new FilePath(fullPath);

                // When
                string path = NormalizedPath.Collapse(filePath);

                // Then
                Assert.AreEqual(expected, path);
            }
        }

        public class GetProviderAndPathMethodTests : NormalizedPathTests
        {
            [Test]
            [TestCase("C:/a/b", null, "C:/a/b")]
            [TestCase(@"C:\a\b", null, @"C:\a\b")]
            [TestCase(@"::C::\a\b", null, @"C::\a\b")]
            [TestCase(@"provider::C::\a\b", "provider", @"C::\a\b")]
            [TestCase("/a/b", null, "/a/b")]
            [TestCase("provider::/a/b", "provider", "/a/b")]
            public void ShouldParseProvider(string fullPath, string provider, string path)
            {
                // Given, When
                Tuple<string, string> result = NormalizedPath.GetProviderAndPath(fullPath);

                // Then
                Assert.AreEqual(provider, result.Item1);
                Assert.AreEqual(path, result.Item2);
            }
        }

        public class EqualsMethodTests : NormalizedPathTests
        {
            [Test]
            [TestCase(true)]
            [TestCase(false)]
            public void SameAssetInstancesIsConsideredEqual(bool isCaseSensitive)
            {
                // Given, When
                FilePath path = new FilePath("shaders/basic.vert");

                // Then
                Assert.True(path.Equals(path));
            }

            [Test]
            [TestCase(true)]
            [TestCase(false)]
            public void PathsAreConsideredInequalIfAnyIsNull(bool isCaseSensitive)
            {
                // Given, When
                bool result = new FilePath("test.txt").Equals(null);

                // Then
                Assert.False(result);
            }

            [Test]
            [TestCase(true)]
            [TestCase(false)]
            public void SamePathsAreConsideredEqual(bool isCaseSensitive)
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("shaders/basic.vert");

                // Then
                Assert.True(first.Equals(second));
                Assert.True(second.Equals(first));
            }

            [Test]
            public void DifferentPathsAreNotConsideredEqual()
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("shaders/basic.frag");

                // Then
                Assert.False(first.Equals(second));
                Assert.False(second.Equals(first));
            }

            [Test]
            public void SamePathsButDifferentCasingAreNotConsideredEqual()
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("SHADERS/BASIC.VERT");

                // Then
                Assert.False(first.Equals(second));
                Assert.False(second.Equals(first));
            }

            [Test]
            public void SamePathsWithDifferentProvidersAreNotConsideredEqual()
            {
                // Given, When
                FilePath first = new FilePath("foo", "/shaders/basic.vert");
                FilePath second = new FilePath("bar", "/shaders/basic.vert");

                // Then
                Assert.False(first.Equals(second));
                Assert.False(second.Equals(first));
            }
        }

        public class GetHashCodeMethodTests : NormalizedPathTests
        {
            [Test]
            public void SamePathsGetSameHashCode()
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("shaders/basic.vert");

                // Then
                Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
            }

            [Test]
            public void DifferentPathsGetDifferentHashCodes()
            {
                // Given, When
                var first = new FilePath("shaders/basic.vert");
                var second = new FilePath("shaders/basic.frag");

                // Then
                Assert.AreNotEqual(first.GetHashCode(), second.GetHashCode());
            }

            [Test]
            public void SamePathsButDifferentCasingDoNotGetSameHashCode()
            {
                // Given, When
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("SHADERS/BASIC.VERT");

                // Then
                Assert.AreNotEqual(first.GetHashCode(), second.GetHashCode());
            }
        }

        public class ToLinkMethodTests : NormalizedPathTests
        {
            [TestCase(".", "/")]
            [TestCase("/foo/bar/index.html", "/foo/bar/index.html")]
            [TestCase("/foo/bar/index.htm", "/foo/bar/index.htm")]
            [TestCase("/foo/bar/baz.html", "/foo/bar/baz.html")]
            [TestCase("/index.html", "/index.html")]
            [TestCase("index.html", "/index.html")]
            [TestCase("/foo.html", "/foo.html")]
            [TestCase("foo.html", "/foo.html")]
            [TestCase("C:/bar/foo.html", "/C:/bar/foo.html")]
            [TestCase("C:/bar/foo.html", "/C:/bar/foo.html")]
            public void ShouldReturnLinkForFilePath(string path, string expected)
            {
                // Given
                FilePath filePath = new FilePath(path);

                // When
                string link = filePath.ToLink("/", false, false);

                // Then
                Assert.AreEqual(expected, link);
            }

            [TestCase("", "/foo/bar/abc.html", "/foo/bar/abc.html")]
            [TestCase("", "foo/bar/abc.html", "/foo/bar/abc.html")]
            [TestCase(null, "/foo/bar/abc.html", "/foo/bar/abc.html")]
            [TestCase(null, "foo/bar/abc.html", "foo/bar/abc.html")]
            [TestCase("baz", "/foo/bar/abc.html", "baz/foo/bar/abc.html")]
            [TestCase("baz/", "/foo/bar/abc.html", "baz/foo/bar/abc.html")]
            [TestCase("http://www.google.com", "/foo/bar/abc.html", "http://www.google.com/foo/bar/abc.html")]
            [TestCase("http://www.google.com/", "/foo/bar/abc.html", "http://www.google.com/foo/bar/abc.html")]
            [TestCase("http://www.google.com/xyz", "/foo/bar/abc.html", "http://www.google.com/xyz/foo/bar/abc.html")]
            [TestCase("http://www.google.com/xyz/", "/foo/bar/abc.html", "http://www.google.com/xyz/foo/bar/abc.html")]
            public void ShouldJoinRootForFilePath(string root, string path, string expected)
            {
                // Given
                FilePath filePath = new FilePath(path);

                // When
                string link = filePath.ToLink(root, false, false);

                // Then
                Assert.AreEqual(expected, link);
            }

            [TestCase("/foo/bar/index.html", "/foo/bar")]
            [TestCase("/foo/bar/index.htm", "/foo/bar")]
            [TestCase("/foo/bar/index.xyz", "/foo/bar/index.xyz")]
            [TestCase("/index.html", "/")]
            [TestCase("/index.htm", "/")]
            [TestCase("index.html", "/")]
            [TestCase("index.htm", "/")]
            [TestCase("index.xyz", "/index.xyz")]
            [TestCase("/foo/bar/baz.html", "/foo/bar/baz.html")]
            public void ShouldHideIndexPagesForFilePath(string path, string expected)
            {
                // Given
                FilePath filePath = new FilePath(path);

                // When
                string link = filePath.ToLink("/", true, false);

                // Then
                Assert.AreEqual(expected, link);
            }

            [TestCase("/foo/bar/abc.html", "/foo/bar/abc")]
            [TestCase("/foo/bar/abc.htm", "/foo/bar/abc")]
            [TestCase("/foo/bar/abc.xyz", "/foo/bar/abc.xyz")]
            [TestCase("/abc.html", "/abc")]
            [TestCase("/abc.htm", "/abc")]
            [TestCase("abc.html", "/abc")]
            [TestCase("abc.htm", "/abc")]
            [TestCase("/foo/bar/index.html", "/foo/bar/index")]
            [TestCase("/foo/bar/index.htm", "/foo/bar/index")]
            public void ShouldHideWebExtensionsForFilePath(string path, string expected)
            {
                // Given
                FilePath filePath = new FilePath(path);

                // When
                string link = filePath.ToLink("/", false, true);

                // Then
                Assert.AreEqual(expected, link);
            }

            [TestCase("/", ".", "/")]
            [TestCase(null, ".", "/")]
            [TestCase("/", "foo/bar", "/foo/bar")]
            [TestCase("/", "/foo/bar", "/foo/bar")]
            [TestCase("/", "/foo/baz/../bar", "/foo/bar")]
            [TestCase("", "/foo/bar", "/foo/bar")]
            [TestCase(null, "/foo/bar", "/foo/bar")]
            [TestCase("baz", "/foo/bar", "baz/foo/bar")]
            [TestCase("baz/", "/foo/bar", "baz/foo/bar")]
            [TestCase("http://www.google.com", "/foo/bar", "http://www.google.com/foo/bar")]
            [TestCase("http://www.google.com/", "/foo/bar", "http://www.google.com/foo/bar")]
            [TestCase("http://www.google.com/xyz", "/foo/bar", "http://www.google.com/xyz/foo/bar")]
            [TestCase("http://www.google.com/xyz/", "/foo/bar", "http://www.google.com/xyz/foo/bar")]
            public void ShouldReturnLinkForDirectoryPath(string root, string path, string expected)
            {
                // Given
                DirectoryPath directoryPath = new DirectoryPath(path);

                // When
                string link = directoryPath.ToLink(root);

                // Then
                Assert.AreEqual(expected, link);
            }
        }
    }
}
