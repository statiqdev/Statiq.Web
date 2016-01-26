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
    //[TestFixture(typeof(DirectoryPath))]
    //[TestFixture(typeof(FilePath))]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class PathCollectionClassTests<TPath> : BaseFixture where TPath : Path
    {
        private readonly TPath upperCaseA;
        private readonly TPath lowerCaseA;
        private readonly TPath upperCaseB;
        private readonly TPath lowerCaseB;
        private readonly TPath upperCaseC;
        private readonly TPath lowerCaseC;

        public PathCollectionClassTests()
        {
            if (typeof(TPath) == typeof(DirectoryPath))
            {
                upperCaseA = (TPath)(Path)new DirectoryPath("A");
                lowerCaseA = (TPath)(Path)new DirectoryPath("a");
                upperCaseB = (TPath)(Path)new DirectoryPath("B");
                lowerCaseB = (TPath)(Path)new DirectoryPath("b");
                upperCaseC = (TPath)(Path)new DirectoryPath("C");
                lowerCaseC = (TPath)(Path)new DirectoryPath("c");
            }
            else if (typeof(TPath) == typeof(FilePath))
            {
                upperCaseA = (TPath)(Path)new FilePath("A.txt");
                lowerCaseA = (TPath)(Path)new FilePath("a.txt");
                upperCaseB = (TPath)(Path)new FilePath("B.txt");
                lowerCaseB = (TPath)(Path)new FilePath("b.txt");
                upperCaseC = (TPath)(Path)new FilePath("C.txt");
                lowerCaseC = (TPath)(Path)new FilePath("c.txt");
            }
            else
            {
                throw new InvalidOperationException("Need to specify test paths for generic type");
            }
        }

        public class ConstructorTests : PathCollectionClassTests<TPath>
        {
            [Test]
            public void ShouldThrowIfComparerIsNull()
            {
                // Given, When
                TestDelegate test = () => new PathCollection<TPath>(Enumerable.Empty<TPath>(), null);

                // Then
                Assert.Throws<ArgumentNullException>(test);
            }
        }

        public class CountPropertyTests : PathCollectionClassTests<TPath>
        {
            [Test]
            public void ShouldReturnTheNumberOfPathsInTheCollection()
            {
                // Given
                PathCollection<TPath> collection = new PathCollection<TPath>(
                    new [] { upperCaseA, upperCaseB },
                    new PathComparer(false));

                // When, Then
                Assert.AreEqual(2, collection.Count);
            }
        }

        public class AddMethodTests : PathCollectionClassTests<TPath>
        {
            [Test]
            public void ShouldAddPathIfNotAlreadyPresent()
            {
                // Given
                PathCollection<TPath> collection = new PathCollection<TPath>(new PathComparer(false));
                collection.Add(upperCaseB);

                // When
                collection.Add(upperCaseA);

                // Then
                Assert.AreEqual(2, collection.Count);
            }

            [Test]
            [TestCase(true, 2)]
            [TestCase(false, 1)]
            public void ShouldRespectFileSystemCaseSensitivityWhenAddingPath(bool caseSensitive, int expectedCount)
            {
                // Given
                PathCollection<TPath> collection = new PathCollection<TPath>(new PathComparer(caseSensitive));
                collection.Add(upperCaseA);

                // When
                collection.Add(lowerCaseA);

                // Then
                Assert.AreEqual(expectedCount, collection.Count);
            }
        }

        public class AddRangeMethodTests : PathCollectionClassTests<TPath>
        {
            [Test]
            public void ShouldAddPathsThatAreNotPresent()
            {
                // Given
                PathCollection<TPath> collection = new PathCollection<TPath>(
                    new[] { upperCaseA, upperCaseB }, 
                    new PathComparer(false));

                // When
                collection.AddRange(new[] { upperCaseA, upperCaseB, upperCaseC });

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
                    new [] { upperCaseA, upperCaseB }, 
                    new PathComparer(caseSensitive));

                // When
                collection.AddRange(new [] { lowerCaseA, lowerCaseB, lowerCaseC });

                // Then
                Assert.AreEqual(expectedCount, collection.Count);
            }
        }

        public class RemoveMethodTests : PathCollectionClassTests<TPath>
        {
            [Test]
            [TestCase(true, 1)]
            [TestCase(false, 0)]
            public void ShouldRespectFileSystemCaseSensitivityWhenRemovingPath(bool caseSensitive, int expectedCount)
            {
                // Given
                PathCollection<TPath> collection = new PathCollection<TPath>(new PathComparer(caseSensitive));
                collection.Add(upperCaseA);

                // When
                collection.Remove(lowerCaseA);

                // Then
                Assert.AreEqual(expectedCount, collection.Count);
            }
        }

        public class RemoveRangeMethodTests : PathCollectionClassTests<TPath>
        {
            [Test]
            [TestCase(true, 2)]
            [TestCase(false, 0)]
            public void ShouldRespectFileSystemCaseSensitivityWhenRemovingPaths(bool caseSensitive, int expectedCount)
            {
                // Given
                PathCollection<TPath> collection = new PathCollection<TPath>(
                    new [] { upperCaseA, upperCaseB }, 
                    new PathComparer(caseSensitive));

                // When
                collection.RemoveRange(new [] { lowerCaseA, lowerCaseB, lowerCaseC });

                // Then
                Assert.AreEqual(expectedCount, collection.Count);
            }
        }
    }
}
