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
            public void ShouldThrowIfProviderIsNull()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new TestPath(null, "/test"));
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
            public void CurrentDirectoryReturnsEmptyPath()
            {
                // Given, When
                TestPath path = new TestPath("./");

                // Then
                Assert.AreEqual(string.Empty, path.FullPath);
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
            public void ShouldRemoveTrailingSlashes(string value, string expected)
            {
                // Given, When
                TestPath path = new TestPath(value);

                // Then
                Assert.AreEqual(expected, path.FullPath);
            }
        }

        public class ProviderPropertyTests : NormalizedPathTests
        {
            public void ShouldReturnProvider(string pathName)
            {
                // Given
                TestPath path = new TestPath("Hello/World");

                // When, Then
                Assert.AreEqual(2, path.Segments.Length);
                Assert.AreEqual("Hello", path.Segments[0]);
                Assert.AreEqual("World", path.Segments[1]);

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
                // Given
                TestPath path = new TestPath(pathName);

                // When, Then
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
#if !UNIX
            [TestCase("c:/hello/temp/test/../../world", "c:/hello/world")]
            [TestCase("c:/../../../../../../temp", "c:/temp")]
#endif
            public void ShouldCollapsePath(string fullPath, string expected)
            {
                // Given
                DirectoryPath directoryPath = new DirectoryPath(fullPath);

                // When
                string path = NormalizedPath.Collapse(directoryPath);

                // Then
                Assert.AreEqual(expected, path);
            }
        }

        public class GetProviderAndPathMethodTests : NormalizedPathTests
        {
            [Test]
            [TestCase("C:/a/b", "", "C:/a/b")]
            [TestCase(@"C:\a\b", "", @"C:\a\b")]
            [TestCase(@"::C::\a\b", "", @"C::\a\b")]
            [TestCase(@"provider::C::\a\b", "provider", @"C::\a\b")]
            [TestCase("/a/b", "", "/a/b")]
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
    }
}
