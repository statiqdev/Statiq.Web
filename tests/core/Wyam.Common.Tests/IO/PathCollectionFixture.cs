using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.IO;
using Wyam.Testing;

namespace Wyam.Common.Tests.IO
{
    // Resharper NUnit runner having issues with this test fixture
    // [TestFixture(typeof(DirectoryPath))]
    // [TestFixture(typeof(FilePath))]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class PathCollectionFixture<TPath> : BaseFixture
        where TPath : NormalizedPath
    {
        private readonly TPath _upperCaseA;
        private readonly TPath _lowerCaseA;
        private readonly TPath _upperCaseB;
        private readonly TPath _lowerCaseB;
        private readonly TPath _upperCaseC;
        private readonly TPath _lowerCaseC;

        public PathCollectionFixture()
        {
            if (typeof(TPath) == typeof(DirectoryPath))
            {
                _upperCaseA = (TPath)(NormalizedPath)new DirectoryPath("A");
                _lowerCaseA = (TPath)(NormalizedPath)new DirectoryPath("a");
                _upperCaseB = (TPath)(NormalizedPath)new DirectoryPath("B");
                _lowerCaseB = (TPath)(NormalizedPath)new DirectoryPath("b");
                _upperCaseC = (TPath)(NormalizedPath)new DirectoryPath("C");
                _lowerCaseC = (TPath)(NormalizedPath)new DirectoryPath("c");
            }
            else if (typeof(TPath) == typeof(FilePath))
            {
                _upperCaseA = (TPath)(NormalizedPath)new FilePath("A.txt");
                _lowerCaseA = (TPath)(NormalizedPath)new FilePath("a.txt");
                _upperCaseB = (TPath)(NormalizedPath)new FilePath("B.txt");
                _lowerCaseB = (TPath)(NormalizedPath)new FilePath("b.txt");
                _upperCaseC = (TPath)(NormalizedPath)new FilePath("C.txt");
                _lowerCaseC = (TPath)(NormalizedPath)new FilePath("c.txt");
            }
            else
            {
                throw new InvalidOperationException("Need to specify test paths for generic type");
            }
        }

        public class ConstructorTests : PathCollectionFixture<TPath>
        {
            [Test]
            public void ShouldThrowIfComparerIsNull()
            {
                // Given, When
                TestDelegate test = () => new PathCollection<TPath>(Enumerable.Empty<TPath>());

                // Then
                Assert.Throws<ArgumentNullException>(test);
            }
        }

        public class CountTests : PathCollectionFixture<TPath>
        {
            [Test]
            public void ShouldReturnTheNumberOfPathsInTheCollection()
            {
                // Given
                PathCollection<TPath> collection = new PathCollection<TPath>(new[] { _upperCaseA, _upperCaseB });

                // When, Then
                Assert.AreEqual(2, collection.Count);
            }
        }

        public class AddTests : PathCollectionFixture<TPath>
        {
            [Test]
            public void ShouldAddPathIfNotAlreadyPresent()
            {
                // Given
                PathCollection<TPath> collection = new PathCollection<TPath>();
                collection.Add(_upperCaseB);

                // When
                collection.Add(_upperCaseA);

                // Then
                Assert.AreEqual(2, collection.Count);
            }

            [Test]
            [TestCase(true, 2)]
            [TestCase(false, 1)]
            public void ShouldRespectFileSystemCaseSensitivityWhenAddingPath(bool caseSensitive, int expectedCount)
            {
                // Given
                PathCollection<TPath> collection = new PathCollection<TPath>();
                collection.Add(_upperCaseA);

                // When
                collection.Add(_lowerCaseA);

                // Then
                Assert.AreEqual(expectedCount, collection.Count);
            }
        }

        public class AddRangeTests : PathCollectionFixture<TPath>
        {
            [Test]
            public void ShouldAddPathsThatAreNotPresent()
            {
                // Given
                PathCollection<TPath> collection = new PathCollection<TPath>(
                    new[] { _upperCaseA, _upperCaseB });

                // When
                collection.AddRange(new[] { _upperCaseA, _upperCaseB, _upperCaseC });

                // Then
                Assert.AreEqual(3, collection.Count);
            }

            [Test]
            [TestCase(true, 5)]
            [TestCase(false, 3)]
            public void ShouldRespectFileSystemCaseSensitivityWhenAddingPaths(bool caseSensitive, int expectedCount)
            {
                // Given
                PathCollection<TPath> collection = new PathCollection<TPath>(
                    new[] { _upperCaseA, _upperCaseB });

                // When
                collection.AddRange(new[] { _lowerCaseA, _lowerCaseB, _lowerCaseC });

                // Then
                Assert.AreEqual(expectedCount, collection.Count);
            }
        }

        public class RemoveTests : PathCollectionFixture<TPath>
        {
            [Test]
            [TestCase(true, 1)]
            [TestCase(false, 0)]
            public void ShouldRespectFileSystemCaseSensitivityWhenRemovingPath(bool caseSensitive, int expectedCount)
            {
                // Given
                PathCollection<TPath> collection = new PathCollection<TPath>();
                collection.Add(_upperCaseA);

                // When
                collection.Remove(_lowerCaseA);

                // Then
                Assert.AreEqual(expectedCount, collection.Count);
            }
        }

        public class RemoveRangeTests : PathCollectionFixture<TPath>
        {
            [Test]
            [TestCase(true, 2)]
            [TestCase(false, 0)]
            public void ShouldRespectFileSystemCaseSensitivityWhenRemovingPaths(bool caseSensitive, int expectedCount)
            {
                // Given
                PathCollection<TPath> collection = new PathCollection<TPath>(
                    new[] { _upperCaseA, _upperCaseB });

                // When
                collection.RemoveRange(new[] { _lowerCaseA, _lowerCaseB, _lowerCaseC });

                // Then
                Assert.AreEqual(expectedCount, collection.Count);
            }
        }
    }
}
