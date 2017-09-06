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
    public class NormalizedPathFixture : BaseFixture
    {
        private class TestPath : NormalizedPath
        {
            public TestPath(string path, PathKind pathKind = PathKind.RelativeOrAbsolute)
                : base(path, pathKind)
            {
            }

            public TestPath(string fileProvider, string path, PathKind pathKind = PathKind.RelativeOrAbsolute)
                : base(fileProvider, path, pathKind)
            {
            }

            public TestPath(Uri fileProvider, string path, PathKind pathKind = PathKind.RelativeOrAbsolute)
                : base(fileProvider, path, pathKind)
            {
            }

            public TestPath(Uri path)
                : base(path)
            {
            }
        }

        public class ConstructorTests : NormalizedPathFixture
        {
            [Test]
            public void ShouldThrowIfPathIsNull()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new TestPath(null));
            }

            [Test]
            public void ShouldThrowIfStringProviderIsSpecifiedForRelativePath()
            {
                // Given, When, Then
                Assert.Throws<ArgumentException>(() => new TestPath("foo:///", "Hello/World"));
            }

            [Test]
            public void ShouldThrowIfUriProviderIsSpecifiedForRelativePath()
            {
                // Given, When, Then
                Assert.Throws<ArgumentException>(() => new TestPath(new Uri("foo:///"), "Hello/World"));
            }

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
            public void ShouldNormalizePathSeparators()
            {
                // Given, When
                TestPath path = new TestPath("shaders\\basic");

                // Then
                Assert.AreEqual("shaders/basic", path.FullPath);
            }

            [Test]
            public void ShouldTrimWhiteSpaceFromPathAndLeaveSpaces()
            {
                // Given, When
                TestPath path = new TestPath("\t\r\nshaders/basic ");

                // Then
                Assert.AreEqual("shaders/basic ", path.FullPath);
            }

            [Test]
            public void ShouldNotRemoveWhiteSpaceWithinPath()
            {
                // Given, When
                TestPath path = new TestPath("my awesome shaders/basic");

                // Then
                Assert.AreEqual("my awesome shaders/basic", path.FullPath);
            }

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
                TestPath path = new TestPath((Uri)null, value);

                // Then
                Assert.AreEqual(expected, path.FullPath);
            }

            [TestCase("\\")]
            [TestCase("/")]
            public void ShouldNotRemoveSingleTrailingSlash(string value)
            {
                // Given, When
                TestPath path = new TestPath(value);

                // Then
                Assert.AreEqual("/", path.FullPath);
            }

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

            [TestCase("\\")]
            [TestCase("/")]
            public void ShouldNotRemoveOnlyRelativePart(string value)
            {
                // Given, When
                TestPath path = new TestPath(value);

                // Then
                Assert.AreEqual("/", path.FullPath);
            }

            [Test]
            public void ShouldSetProviderIfGivenOnlyScheme()
            {
                // Given, When
                TestPath path = new TestPath("foo", "/a/b");

                // Then
                Assert.AreEqual(new Uri("foo:"), path.FileProvider);
            }

            [Test]
            public void ShouldSetNullProviderForFirstCharDelimiter()
            {
                // Given, When
                TestPath path = new TestPath("|foo://a/b/c");

                // Then
                Assert.AreEqual(null, path.FileProvider);
                Assert.AreEqual("foo://a/b/c", path.FullPath);
            }

            [Test]
            public void ShouldSplitUriPathWithDelimiter()
            {
                // Given, When
                TestPath path = new TestPath(new Uri("foo:///a/b|c/d/e"));

                // Then
                Assert.AreEqual(new Uri("foo:///a/b"), path.FileProvider);
                Assert.AreEqual("c/d/e", path.FullPath);
                Assert.IsTrue(path.IsAbsolute);
            }

            [TestCase("a/b/c")]
            [TestCase("/a/b/c")]
            public void ShouldSetUriAsRelativePathIfNoLeftPart(string path)
            {
                // Given, When
                TestPath testPath = new TestPath(new Uri(path, UriKind.Relative));

                // Then
                Assert.AreEqual(null, testPath.FileProvider);
                Assert.AreEqual(path, testPath.FullPath);
                Assert.IsTrue(testPath.IsRelative);
            }

            [Test]
            public void ExplicitNullProviderShouldStayNull()
            {
                // Given, When
                TestPath testPath = new TestPath((Uri)null, "/a/b/c", PathKind.Absolute);

                // Then
                Assert.IsNull(testPath.FileProvider);
            }

            [Test]
            public void UnspecifiedProviderShouldUseDefault()
            {
                // Given, When
                TestPath testPath = new TestPath("/a/b/c", PathKind.Absolute);

                // Then
                Assert.AreEqual(new Uri("file:///"), testPath.FileProvider);
            }

            [TestCase("foo:///")]
            [TestCase("foo://")]
            public void ShouldSetRootPathIfOnlyRootInString(string path)
            {
                // Given, When
                TestPath testPath = new TestPath(path);

                // Then
                Assert.AreEqual(new Uri(path), testPath.FileProvider);
                Assert.AreEqual("/", testPath.FullPath);
            }

            [TestCase("foo:///")]
            [TestCase("foo://")]
            public void ShouldSetRootPathIfOnlyRootInUri(string path)
            {
                // Given, When
                TestPath testPath = new TestPath(new Uri(path));

                // Then
                Assert.AreEqual(new Uri(path), testPath.FileProvider);
                Assert.AreEqual("/", testPath.FullPath);
            }

            [Test]
            public void ShouldUsePathAsFullPathIfNoPathInString()
            {
                // Given, When
                TestPath testPath = new TestPath("foo:");

                // Then
                Assert.AreEqual(null, testPath.FileProvider);
                Assert.AreEqual("foo:/", testPath.FullPath); // The slash is appended w/ assumption this is a file path
            }

            [Test]
            public void ShouldThrowIfPathInUri()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new TestPath(new Uri("foo:")));
            }
        }

        public class GetFileProviderUriTests : NormalizedPathFixture
        {
            [TestCase(null)]
            [TestCase("")]
            public void ShouldReturnNullForNullOrEmptyPath(string provider)
            {
                // Given, When
                Uri uri = NormalizedPath.GetFileProviderUri(provider);

                // Then
                Assert.IsNull(uri);
            }

            [Test]
            public void ShouldReturnUriWithSchemeForScheme()
            {
                // Given, When
                Uri uri = NormalizedPath.GetFileProviderUri("foo");

                // Then
                Assert.AreEqual(new Uri("foo:"), uri);
            }

            [TestCase("foo:")]
            [TestCase("foo:///")]
            [TestCase("foo:///a/b/c")]
            [TestCase("foo:///a/b/c?x#y")]
            public void ShouldReturnUriGivenUri(string provider)
            {
                // Given, When
                Uri uri = NormalizedPath.GetFileProviderUri(provider);

                // Then
                Assert.AreEqual(new Uri(provider), uri);
            }

            [TestCase("a/b/c")]
            [TestCase("c:/a/b/c")]
            [TestCase(@"c:\a\b\c")]
            [TestCase(":")]
            public void ThrowsExceptionForInvalidUri(string provider)
            {
                // Given, When, Then
                Assert.Throws<ArgumentException>(() => NormalizedPath.GetFileProviderUri(provider));
            }
        }

        public class FileProviderTests : NormalizedPathFixture
        {
            [TestCase("foo", "/Hello/World", "foo:")]
            [TestCase("foo:///", "/Hello/World", "foo:///")]
            [TestCase("foo://x/y", "/Hello/World", "foo://x/y")]
            [TestCase("", "/Hello/World", null)]
            [TestCase(null, "/Hello/World", null)]
            [TestCase(null, "Hello/World", null)]
            [TestCase("", "Hello/World", null)]
            public void ShouldReturnProvider(string provider, string pathName, string expectedProvider)
            {
                // Given, W
                TestPath path = new TestPath(provider, pathName);

                // Then
                Assert.AreEqual(expectedProvider == null ? null : new Uri(expectedProvider), path.FileProvider);
            }
        }

        public class SegmentsTests : NormalizedPathFixture
        {
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

        public class FullPathTests : NormalizedPathFixture
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

        public class RootTests : NormalizedPathFixture
        {
            [TestCase(@"\a\b\c", "/")]
            [TestCase("/a/b/c", "/")]
            [TestCase("a/b/c", ".")]
            [TestCase(@"a\b\c", ".")]
            [TestCase("foo.txt", ".")]
            [TestCase("foo", ".")]
#if !UNIX
            [TestCase(@"c:\a\b\c", "c:/")]
            [TestCase("c:/a/b/c", "c:/")]
#endif
            public void ShouldReturnRootPath(string fullPath, string expected)
            {
                // Given
                TestPath path = new TestPath(fullPath);

                // When
                DirectoryPath root = path.Root;

                // Then
                Assert.AreEqual(expected, root.FullPath);
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
            public void ShouldReturnDottedRootForExplicitRelativePath(string fullPath)
            {
                // Given
                TestPath path = new TestPath(fullPath, PathKind.Relative);

                // When
                DirectoryPath root = path.Root;

                // Then
                Assert.AreEqual(".", root.FullPath);
            }
        }

        public class IsRelativeTests : NormalizedPathFixture
        {
            [TestCase("assets/shaders", true)]
            [TestCase("assets/shaders/basic.frag", true)]
            [TestCase("/assets/shaders", false)]
            [TestCase("/assets/shaders/basic.frag", false)]
            public void ShouldReturnWhetherOrNotAPathIsRelative(string fullPath, bool expected)
            {
                // Given, When
                TestPath path = new TestPath(fullPath);

                // Then
                Assert.AreEqual(expected, path.IsRelative);
            }

#if !UNIX
            [TestCase("c:/assets/shaders", false)]
            [TestCase("c:/assets/shaders/basic.frag", false)]
            [TestCase("c:/", false)]
            [TestCase("c:", false)]
            public void ShouldReturnWhetherOrNotAPathIsRelativeOnWindows(string fullPath, bool expected)
            {
                // Given, When
                TestPath path = new TestPath(fullPath);

                // Then
                Assert.AreEqual(expected, path.IsRelative);
            }
#endif
        }

        public class ToStringTests : NormalizedPathFixture
        {
            [TestCase(null, "temp/hello", "temp/hello")]
            [TestCase("foo://a/b/c", "/temp/hello", "foo://a/b/c|/temp/hello")]
            [TestCase("foo:///", "/temp/hello", "foo:///temp/hello")]
            [TestCase("foo:///", "c:/temp/hello", "foo:///c:/temp/hello")]
            [TestCase("foo:", "/temp/hello", "foo:/temp/hello")]
            [TestCase("foo:", "c:/temp/hello", "foo:c:/temp/hello")]
            [TestCase(null, "/temp/hello", "/temp/hello")]
            [TestCase(null, "c:/temp/hello", "c:/temp/hello")]
            public void ShouldReturnStringRepresentation(string provider, string path, string expected)
            {
                // Given, When
                TestPath testPath = new TestPath(provider == null ? null : new Uri(provider), path);

                // Then
                Assert.AreEqual(expected, testPath.ToString());
            }
        }

        public class CollapseTests : NormalizedPathFixture
        {
            [Test]
            public void ShouldThrowIfPathIsNull()
            {
                // Given, When
                TestDelegate test = () => NormalizedPath.Collapse(null);

                // Then
                Assert.Throws<ArgumentNullException>(test);
            }

            [TestCase("hello/temp/test/../../world", "hello/world")]
            [TestCase("hello/temp/../temp2/../world", "hello/world")]
            [TestCase("/hello/temp/test/../../world", "/hello/world")]
            [TestCase("/hello/../../../../../../temp", "/temp")] // Stop collapsing when root is reached
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

        public class GetFileProviderAndPathTests : NormalizedPathFixture
        {
            [TestCase("C:/a/b", null, "C:/a/b")]
            [TestCase(@"C:\a\b", null, @"C:\a\b")]
            [TestCase(@"|C|\a\b", null, @"C|\a\b")]
            [TestCase(@"provider|C|\a\b", "provider:", @"C|\a\b")]
            [TestCase(@"provider:///|C|\a\b", "provider:///", @"C|\a\b")]
            [TestCase("/a/b", null, "/a/b")]
            [TestCase("provider|/a/b", "provider:", "/a/b")]
            [TestCase("provider:///|/a/b", "provider:///", "/a/b")]
            [TestCase("provider://x/y/z|/a/b", "provider://x/y/z", "/a/b")]
            [TestCase("|provider://x/y/z|/a/b", null, "provider://x/y/z|/a/b")]
            [TestCase("foo::/A/B?x#c", "foo:", ":/A/B?x#c")]
            public void ShouldParseProviderFromString(string fullPath, string provider, string path)
            {
                // Given, When
                Tuple<Uri, string> result = NormalizedPath.GetFileProviderAndPath(null, fullPath);

                // Then
                Assert.AreEqual(provider == null ? null : new Uri(provider), result.Item1);
                Assert.AreEqual(path, result.Item2);
            }
        }

        public class EqualsTests : NormalizedPathFixture
        {
            [TestCase(true)]
            [TestCase(false)]
            public void SameAssetInstancesIsConsideredEqual(bool isCaseSensitive)
            {
                // Given, When
                FilePath path = new FilePath("shaders/basic.vert");

                // Then
                Assert.True(path.Equals(path));
            }

            [TestCase(true)]
            [TestCase(false)]
            public void PathsAreConsideredInequalIfAnyIsNull(bool isCaseSensitive)
            {
                // Given, When
                bool result = new FilePath("test.txt").Equals(null);

                // Then
                Assert.False(result);
            }

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
                FilePath first = new FilePath(new Uri("foo:///"), "/shaders/basic.vert");
                FilePath second = new FilePath(new Uri("bar:///"), "/shaders/basic.vert");

                // Then
                Assert.False(first.Equals(second));
                Assert.False(second.Equals(first));
            }
        }

        public class GetHashCodeTests : NormalizedPathFixture
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
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("shaders/basic.frag");

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
    }
}
