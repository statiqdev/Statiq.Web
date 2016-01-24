using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common;
using Wyam.Common.IO;
using Wyam.Testing;

namespace Wyam.Common.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class PathHelperFixture : TraceListenerFixture
    {
        [TestCase(@"C:\A\B\", @"C:\A\B\", @"")]
        [TestCase(@"C:\A\B\", @"C:\A\B\C", @"C")]
        [TestCase(@"\A\B\", @"\A\B\C", @"C")]
        [TestCase(@"A\B\", @"A\B\C", @"C")]
        public void GetRelativePathReturnsCorrectPath(string fromPath, string toPath, string expectedPath)
        {
            // Given

            // When
            string calculatedPath = PathHelper.GetRelativePath(fromPath, toPath);

            // Then
            Assert.AreEqual(expectedPath, calculatedPath);
        }

        [TestCase(@"C:\A\B\", @"..\*.txt", @"C:\A\*.txt")]
        [TestCase(@"C:\A\B\", @".\*.txt", @"C:\A\B\*.txt")]
        [TestCase(@"C:\A\B\", @"*.*", @"C:\A\B\*.*")]
        [TestCase(@"C:\A\B\", @"..\..\*.txt", @"C:\*.txt")]
        public void GetCombinedFullPath(string path1, string path2, string expectedPath)
        {
            // Given

            // When
            string calculatedPath = PathHelper.CombineToFullPath(path1, path2);

            // Then
            Assert.AreEqual(expectedPath, calculatedPath);
        }
    }
}
