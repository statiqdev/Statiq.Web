using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Wyam.Core.Meta;
using Wyam.Testing;

namespace Wyam.Core.Tests.Meta
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class TypeHelperFixture : BaseFixture
    {
        public class TryConvertTests : TypeHelperFixture
        {
            [Test]
            public void ArrayConvertsToArray()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                Array result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                CollectionAssert.AreEqual(value, result);
            }

            [Test]
            public void ConvertsEnumerableToIReadOnlyList()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                IReadOnlyList<int> result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                CollectionAssert.AreEqual(value, result);
            }

            [Test]
            public void ConvertsEnumerableToIList()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                IList<int> result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                CollectionAssert.AreEqual(value, result);
            }

            [Test]
            public void ConvertsEnumerableToList()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                List<int> result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                CollectionAssert.AreEqual(value, result);
            }

            [Test]
            public void ConvertsEnumerableToArray()
            {
                // Given
                List<int> value = new List<int> { 1, 2, 3 };

                // When
                int[] result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                CollectionAssert.AreEqual(value, result);
            }

            [Test]
            public void ConvertsEnumerableToIEnumerable()
            {
                // Given
                Array value = new[] { 1.0, 2.0 };

                // When
                IEnumerable<int> result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                CollectionAssert.AreEqual(value, result);
            }

            [Test]
            public void ConvertsSingleArrayToItemOfIReadOnlyList()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                IReadOnlyList<Array> result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                CollectionAssert.AreEquivalent(new[] { value }, result);
            }

            [Test]
            public void ConvertsSingleArrayToItemOfIList()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                IList<Array> result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                CollectionAssert.AreEquivalent(new[] { value }, result);
            }

            [Test]
            public void ConvertsSingleArrayToItemOfList()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                List<Array> result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                CollectionAssert.AreEquivalent(new[] { value }, result);
            }

            [Test]
            public void ConvertsSingleEnumerableToItemOfArray()
            {
                // Given
                List<int> value = new List<int> { 1, 2, 3 };

                // When
                List<int>[] result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                CollectionAssert.AreEquivalent(new[] { value }, result);
            }

            [Test]
            public void ConvertsSingleEnumerableToItemOfIEnumerable()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                IEnumerable<Array> result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                CollectionAssert.AreEquivalent(new[] { value }, result);
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
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
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
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                CollectionAssert.AreEquivalent(new[] { value }, result);
                CollectionAssert.AreEquivalent((IEnumerable)value.GetValue(0), result[0][0]);
                CollectionAssert.AreEquivalent((IEnumerable)value.GetValue(1), result[0][1]);
            }

            [Test]
            public void ConvertsArrayOfStringsToFirstString()
            {
                // Given
                Array value = new[] { "Red", "Green", "Blue" };

                // When
                string result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                Assert.AreEqual("Red", result);
            }

            [Test]
            public void ConvertsArrayOfIntsToFirstInt()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                int result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                Assert.AreEqual(1, result);
            }

            [Test]
            public void ConvertsArrayOfIntsToFirstString()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                string result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                Assert.AreEqual("1", result);
            }

            [Test]
            public void ConvertsArrayOfStringsToFirstInt()
            {
                // Given
                Array value = new[] { "1", "2", "3" };

                // When
                int result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                Assert.AreEqual(1, result);
            }

            [Test]
            public void ConvertsArrayOfObjectsToFirstInt()
            {
                // Given
                Array value = new object[] { "1", 2, 3.0 };

                // When
                int result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                Assert.AreEqual(1, result);
            }

            [Test]
            public void ConvertsArrayOfObjectsToFirstIntWhenFirstItemNotConvertible()
            {
                // Given
                Array value = new object[] { "a", 2, 3.0 };

                // When
                int result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                Assert.AreEqual(2, result);
            }

            [Test]
            public void ArrayOfIntConvertsToEnumerableOfString()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                IEnumerable<string> result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                CollectionAssert.AreEqual(new[] { "1", "2", "3" }, result);
            }

            [Test]
            public void ArrayOfIntConvertsToEnumerableOfObject()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                IEnumerable<object> result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                CollectionAssert.AreEqual(value, result);
            }

            [Test]
            public void StringConvertsToUri()
            {
                // Given
                const string value = "http://google.com/";

                // When
                Uri uri;
                bool convert = TypeHelper.TryConvert(value, out uri);

                // Then
                Assert.IsTrue(convert);
                Assert.AreEqual(value, uri.ToString());
            }

            [Test]
            public void StringConvertsToDateTime()
            {
                // Given
                const string value = "2016-10-17 08:00";

                // When
                DateTime dateTime;
                bool convert = TypeHelper.TryConvert(value, out dateTime);

                // Then
                Assert.IsTrue(convert);
                Assert.AreEqual(new DateTime(2016, 10, 17, 8, 0, 0), dateTime);
            }

            [Test]
            public void NullConvertsToNullable()
            {
                // Given
                object value = null;

                // When
                DateTime? result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                Assert.IsNull(result);
            }

            [Test]
            public void NullDoesNotConvertToValueType()
            {
                // Given
                object value = null;

                // When
                int result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsFalse(convert);
            }

            [Test]
            public void NullConvertsToReferenceType()
            {
                // Given
                object value = null;

                // When
                object result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                Assert.IsNull(result);
            }

            [Test]
            public void DateTimeConvertstoNullable()
            {
                // Given
                DateTime value = new DateTime(2015, 1, 1);

                // When
                DateTime? result;
                bool convert = TypeHelper.TryConvert(value, out result);

                // Then
                Assert.IsTrue(convert);
                Assert.IsTrue(result.HasValue);
                Assert.AreEqual(value, result.Value);
            }
        }
    }
}
