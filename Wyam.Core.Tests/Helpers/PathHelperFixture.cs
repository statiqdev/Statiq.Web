using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common;

namespace Wyam.Core.Tests.Helpers
{
    [TestFixture]
    public class PathHelperFixture
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
    }
}
