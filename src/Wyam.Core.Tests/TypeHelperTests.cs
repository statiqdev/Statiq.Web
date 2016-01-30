using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Testing;

namespace Wyam.Core.Tests
{
    [TestFixture]
    public class TypeHelperTests : BaseFixture
    {
        [Test]
        public void ArrayConvertsToArray()
        {
            // Given
            Array value = new[] {1, 2, 3};

            // When
            Array result;
            TypeHelper.TryConvert(value, out result);

            // Then
            CollectionAssert.AreEqual(value, result);
        }

        [Test]
        public void ConvertsEnumerableToIReadOnlyList()
        {
            // Given
            Array value = new[] { 1, 2, 3 };

            // When
            IReadOnlyList<int> result;
            TypeHelper.TryConvert(value, out result);

            // Then
            CollectionAssert.AreEqual(value, result);
        }

        [Test]
        public void ConvertsEnumerableToIList()
        {
            // Given
            Array value = new[] { 1, 2, 3 };

            // When
            IList<int> result;
            TypeHelper.TryConvert(value, out result);

            // Then
            CollectionAssert.AreEqual(value, result);
        }

        [Test]
        public void ConvertsEnumerableToList()
        {
            // Given
            Array value = new[] {1, 2, 3};

            // When
            List<int> result;
            TypeHelper.TryConvert(value, out result);

            // Then
            CollectionAssert.AreEqual(value, result);
        }
        
        [Test]
        public void ConvertsEnumerableToArray()
        {
            // Given
            List<int> value = new List<int> { 1, 2, 3 };

            // When
            int[] result;
            TypeHelper.TryConvert(value, out result);

            // Then
            CollectionAssert.AreEqual(value, result);
        }

        [Test]
        public void ConvertsEnumerableToIEnumerable()
        {
            // Given
            Array value = new [] { 1.0, 2.0 };

            // When
            IEnumerable<int> result;
            TypeHelper.TryConvert(value, out result);

            // Then
            CollectionAssert.AreEqual(value, result);
        }

        [Test]
        public void ConvertsSingleArrayToItemOfIReadOnlyList()
        {
            // Given
            Array value = new[] { 1, 2, 3 };

            // When
            IReadOnlyList<Array> result;
            TypeHelper.TryConvert(value, out result);

            // Then
            CollectionAssert.AreEquivalent(new[] { value }, result);
        }

        [Test]
        public void ConvertsSingleArrayToItemOfIList()
        {
            // Given
            Array value = new[] { 1, 2, 3 };

            // When
            IList<Array> result;
            TypeHelper.TryConvert(value, out result);

            // Then
            CollectionAssert.AreEquivalent(new[] { value }, result);
        }

        [Test]
        public void ConvertsSingleArrayToItemOfList()
        {
            // Given
            Array value = new[] { 1, 2, 3 };

            // When
            List<Array> result;
            TypeHelper.TryConvert(value, out result);

            // Then
            CollectionAssert.AreEquivalent(new[] { value }, result);
        }

        [Test]
        public void ConvertsSingleEnumerableToItemOfArray()
        {
            // Given
            List<int> value = new List<int> { 1, 2, 3 };

            // When
            List<int>[] result;
            TypeHelper.TryConvert(value, out result);

            // Then
            CollectionAssert.AreEquivalent(new[] { value }, result);
        }

        [Test]
        public void ConvertsSingleEnumerableToItemOfArrayClass()
        {
            // Given
            List<int> value = new List<int> { 1, 2, 3 };

            // When
            Array result;
            TypeHelper.TryConvert(value, out result);

            // Then
            CollectionAssert.AreEquivalent(new[] { value }, result);
        }

        [Test]
        public void ConvertsSingleEnumerableToItemOfIEnumerable()
        {
            // Given
            Array value = new[] { 1, 2, 3 };

            // When
            IEnumerable<Array> result;
            TypeHelper.TryConvert(value, out result);

            // Then
            CollectionAssert.AreEquivalent(new [] { value }, result);
        }

        [Test]
        public void ConvertsArrayOfEnumerablesToItemsInList()
        {
            // Given
            Array value = new[]
            {
                new List<int> { 1, 2, 3 },
                new List<int> { 4, 5, 6 }
            };

            // When
            IList<IEnumerable<int>> result;
            TypeHelper.TryConvert(value, out result);

            // Then
            CollectionAssert.AreEquivalent(value, result);
        }

        [Test]
        public void ConvertsArrayOfEnumerablesToItemInListOfArrays()
        {
            // Given
            Array value = new[]
            {
                new List<int> { 1, 2, 3 },
                new List<int> { 4, 5, 6 }
            };

            // When
            IList<IList<IEnumerable<int>>> result;
            TypeHelper.TryConvert(value, out result);

            // Then
            CollectionAssert.AreEquivalent(new [] { value }, result);
            CollectionAssert.AreEquivalent((IEnumerable)value.GetValue(0), result[0][0]);
            CollectionAssert.AreEquivalent((IEnumerable)value.GetValue(1), result[0][1]);
        }
    }
}
