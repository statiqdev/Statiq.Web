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
    public class PathComparerClassTests : BaseFixture
    {
        public class EqualsMethodTests : PathComparerClassTests
        {
            [Test]
            [TestCase(true)]
            [TestCase(false)]
            public void SameAssetInstancesIsConsideredEqual(bool isCaseSensitive)
            {
                // Given, When
                PathComparer comparer = new PathComparer(isCaseSensitive);
                FilePath path = new FilePath("shaders/basic.vert");

                // Then
                Assert.True(comparer.Equals(path, path));
            }

            [Test]
            [TestCase(true)]
            [TestCase(false)]
            public void TwoNullPathsAreConsideredEqual(bool isCaseSensitive)
            {
                // Given
                PathComparer comparer = new PathComparer(isCaseSensitive);

                // When
                bool result = comparer.Equals(null, null);

                // Then
                Assert.True(result);
            }

            [Test]
            [TestCase(true)]
            [TestCase(false)]
            public void PathsAreConsideredInequalIfAnyIsNull(bool isCaseSensitive)
            {
                // Given
                PathComparer comparer = new PathComparer(isCaseSensitive);

                // When
                bool result = comparer.Equals(null, new FilePath("test.txt"));

                // Then
                Assert.False(result);
            }


            [Test]
            [TestCase(true)]
            [TestCase(false)]
            public void SamePathsAreConsideredEqual(bool isCaseSensitive)
            {
                // Given, When
                PathComparer comparer = new PathComparer(isCaseSensitive);
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("shaders/basic.vert");

                // Then
                Assert.True(comparer.Equals(first, second));
                Assert.True(comparer.Equals(second, first));
            }

            [Test]
            [TestCase(true)]
            [TestCase(false)]
            public void DifferentPathsAreNotConsideredEqual(bool isCaseSensitive)
            {
                // Given, When
                PathComparer comparer = new PathComparer(isCaseSensitive);
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("shaders/basic.frag");

                // Then
                Assert.False(comparer.Equals(first, second));
                Assert.False(comparer.Equals(second, first));
            }

            [Test]
            [TestCase(true, false)]
            [TestCase(false, true)]
            public void SamePathsButDifferentCasingAreConsideredEqualDependingOnCaseSensitivity(bool isCaseSensitive, bool expected)
            {
                // Given, When
                PathComparer comparer = new PathComparer(isCaseSensitive);
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("SHADERS/BASIC.VERT");

                // Then
                Assert.AreEqual(expected, comparer.Equals(first, second));
                Assert.AreEqual(expected, comparer.Equals(second, first));
            }
        }

        public sealed class GetHashCodeMethodTests : PathComparerClassTests
        {
            [Test]
            public void ShouldThrowIfOtherPathIsNull()
            {
                // Given
                PathComparer comparer = new PathComparer(true);

                // When
                TestDelegate test = () => comparer.GetHashCode(null);

                // Then
                Assert.Throws<ArgumentNullException>(test);
            }

            [Test]
            [TestCase(true)]
            [TestCase(false)]
            public void SamePathsGetSameHashCode(bool isCaseSensitive)
            {
                // Given, When
                PathComparer comparer = new PathComparer(isCaseSensitive);
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("shaders/basic.vert");

                // Then
                Assert.AreEqual(comparer.GetHashCode(first), comparer.GetHashCode(second));
            }

            [Test]
            [TestCase(true)]
            [TestCase(false)]
            public void DifferentPathsGetDifferentHashCodes(bool isCaseSensitive)
            {
                // Given, When
                var comparer = new PathComparer(isCaseSensitive);
                var first = new FilePath("shaders/basic.vert");
                var second = new FilePath("shaders/basic.frag");

                // Then
                Assert.AreNotEqual(comparer.GetHashCode(first), comparer.GetHashCode(second));
            }

            [Test]
            [TestCase(true, false)]
            [TestCase(false, true)]
            public void SamePathsButDifferentCasingGetSameHashCodeDependingOnCaseSensitivity(bool isCaseSensitive, bool expected)
            {
                // Given, When
                PathComparer comparer = new PathComparer(isCaseSensitive);
                FilePath first = new FilePath("shaders/basic.vert");
                FilePath second = new FilePath("SHADERS/BASIC.VERT");

                // Then
                Assert.AreEqual(expected, comparer.GetHashCode(first) == comparer.GetHashCode(second));
            }
        }

        public sealed class IsCaseSensitivePropertyTests : PathComparerClassTests
        {
            [Test]
            [TestCase(true)]
            [TestCase(false)]
            public void ShouldReturnWhetherOrNotTheComparerIsCaseSensitive(bool isCaseSensitive)
            {
                // Given, When
                PathComparer comparer = new PathComparer(isCaseSensitive);

                // Then
                Assert.AreEqual(isCaseSensitive, comparer.IsCaseSensitive);
            }
        }
    }
}
