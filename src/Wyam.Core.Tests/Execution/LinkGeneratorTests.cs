using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Core.Execution;
using Wyam.Testing;

namespace Wyam.Core.Tests.Execution
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class LinkGeneratorTests : BaseFixture
    {
        public class GetLinkMethodTests : LinkGeneratorTests
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
            [TestCase(null, "/")]
            public void ShouldReturnLinkForFilePath(string path, string expected)
            {
                // Given
                FilePath filePath = path == null ? null : new FilePath(path);

                // When
                string link = LinkGenerator.GetLink(filePath, null, null, false, false);

                // Then
                Assert.AreEqual(expected, link);
            }
            
            [TestCase(null, null, "/foo/bar/abc.html", "/foo/bar/abc.html")]
            [TestCase(null, null, "foo/bar/abc.html", "/foo/bar/abc.html")]
            [TestCase(null, "baz", "/foo/bar/abc.html", "/baz/foo/bar/abc.html")]
            [TestCase(null, "baz/", "/foo/bar/abc.html", "/baz/foo/bar/abc.html")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html", "http://www.google.com/foo/bar/abc.html")]
            [TestCase("www.google.com", null, "/foo/bar/abc.html", "http://www.google.com/foo/bar/abc.html")]
            [TestCase("www.google.com", "/xyz", "/foo/bar/abc.html", "http://www.google.com/xyz/foo/bar/abc.html")]
            [TestCase("www.google.com", "/xyz/", "/foo/bar/abc.html", "http://www.google.com/xyz/foo/bar/abc.html")]
            [TestCase(null, "baz", null, "/baz")]
            [TestCase("www.google.com", null, null, "http://www.google.com/")]
            [TestCase("www.google.com", "/xyz", null, "http://www.google.com/xyz")]
            public void ShouldJoinHostAndRootForFilePath(string host, string root, string path, string expected)
            {
                // Given
                FilePath filePath = path == null ? null : new FilePath(path);

                // When
                string link = LinkGenerator.GetLink(filePath, host, root == null ? null : new DirectoryPath(root), false, false);

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
                string link = LinkGenerator.GetLink(filePath, null, null, true, false);

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
                string link = LinkGenerator.GetLink(filePath, null, null, false, true);

                // Then
                Assert.AreEqual(expected, link);
            }

            [TestCase(null, "/", ".", "/")]
            [TestCase(null, null, ".", "/")]
            [TestCase(null, null, null, "/")]
            [TestCase(null, "/", "foo/bar", "/foo/bar")]
            [TestCase(null, "/", "/foo/bar", "/foo/bar")]
            [TestCase(null, "/", "/foo/baz/../bar", "/foo/bar")]
            [TestCase(null, null, "/foo/bar", "/foo/bar")]
            [TestCase(null, "baz", "/foo/bar", "/baz/foo/bar")]
            [TestCase(null, "/baz/", "/foo/bar", "/baz/foo/bar")]
            [TestCase("www.google.com", null, "/foo/bar", "http://www.google.com/foo/bar")]
            [TestCase("www.google.com", null, "/foo/bar", "http://www.google.com/foo/bar")]
            [TestCase("www.google.com" ,"xyz", "/foo/bar", "http://www.google.com/xyz/foo/bar")]
            [TestCase("www.google.com", "/xyz/", "/foo/bar", "http://www.google.com/xyz/foo/bar")]
            [TestCase("www.google.com", null, null, "http://www.google.com/")]
            [TestCase("www.google.com", "xyz", null, "http://www.google.com/xyz")]
            public void ShouldJoinHostAndRootForDirectoryPath(string host, string root, string path, string expected)
            {
                // Given
                DirectoryPath directoryPath = path == null ? null : new DirectoryPath(path);

                // When
                string link = LinkGenerator.GetLink(directoryPath, host, root == null ? null : new DirectoryPath(root), false, false);

                // Then
                Assert.AreEqual(expected, link);
            }
        }
    }
}
