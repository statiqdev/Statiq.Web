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
    public class PathTests : BaseFixture
    {
        public class CollapseMethodTests : PathTests
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
    }
}
