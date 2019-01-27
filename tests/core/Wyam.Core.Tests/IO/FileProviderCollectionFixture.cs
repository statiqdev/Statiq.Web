using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.IO;
using Wyam.Core.IO;
using Wyam.Testing;
using Wyam.Testing.IO;

namespace Wyam.Core.Tests.IO
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class FileProviderCollectionFixture : BaseFixture
    {
        public class ConstructorTests : FileProviderCollectionFixture
        {
            [Test]
            public void SetsDefaultProvider()
            {
                // Given, When
                IFileProvider defaultProvider = new TestFileProvider();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // Then
                CollectionAssert.AreEquivalent(
                    new Dictionary<string, IFileProvider>
                {
                    { NormalizedPath.DefaultFileProvider.Scheme, defaultProvider }
                }, collection.Providers);
            }

            [Test]
            public void ThrowsForNullDefaultProvider()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new FileProviderCollection(null));
            }
        }

        public class AddTests : FileProviderCollectionFixture
        {
            [Test]
            public void AddsProvider()
            {
                // Given
                IFileProvider defaultProvider = new TestFileProvider();
                IFileProvider newProvider = new TestFileProvider();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When
                collection.Add("foo", newProvider);

                // Then
                CollectionAssert.AreEquivalent(
                    new Dictionary<string, IFileProvider>
                {
                    { NormalizedPath.DefaultFileProvider.Scheme, defaultProvider },
                    { "foo", newProvider }
                }, collection.Providers);
            }

            [Test]
            public void AddsDuplicateProvider()
            {
                // Given
                IFileProvider defaultProvider = new TestFileProvider();
                IFileProvider oldProvider = new TestFileProvider();
                IFileProvider newProvider = new TestFileProvider();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When
                collection.Add("foo", oldProvider);
                collection.Add("foo", newProvider);

                // Then
                CollectionAssert.AreEquivalent(
                    new Dictionary<string, IFileProvider>
                {
                    { NormalizedPath.DefaultFileProvider.Scheme, defaultProvider },
                    { "foo", newProvider }
                }, collection.Providers);
            }

            [Test]
            public void AddsNewDefaultProvider()
            {
                // Given
                IFileProvider defaultProvider = new TestFileProvider();
                IFileProvider newProvider = new TestFileProvider();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When
                collection.Add(NormalizedPath.DefaultFileProvider.Scheme, newProvider);

                // Then
                CollectionAssert.AreEquivalent(
                    new Dictionary<string, IFileProvider>
                {
                    { NormalizedPath.DefaultFileProvider.Scheme, newProvider }
                }, collection.Providers);
            }

            [Test]
            public void ThrowsForNullName()
            {
                // Given
                IFileProvider defaultProvider = new TestFileProvider();
                IFileProvider newProvider = new TestFileProvider();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When, Then
                Assert.Throws<ArgumentNullException>(() => collection.Add(null, newProvider));
            }

            [Test]
            public void ThrowsForNullProvider()
            {
                // Given
                IFileProvider defaultProvider = new TestFileProvider();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When, Then
                Assert.Throws<ArgumentNullException>(() => collection.Add("foo", null));
            }
        }

        public class RemoveTests : FileProviderCollectionFixture
        {
            [Test]
            public void RemovesExistingProvider()
            {
                // Given
                IFileProvider defaultProvider = new TestFileProvider();
                IFileProvider newProvider = new TestFileProvider();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);
                collection.Add("foo", newProvider);

                // When
                bool result = collection.Remove("foo");

                // Then
                Assert.IsTrue(result);
                CollectionAssert.AreEquivalent(
                    new Dictionary<string, IFileProvider>
                {
                    { NormalizedPath.DefaultFileProvider.Scheme, defaultProvider }
                }, collection.Providers);
            }

            [Test]
            public void ReturnsFalseForNonExistingProvider()
            {
                // Given
                IFileProvider defaultProvider = new TestFileProvider();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When
                bool result = collection.Remove("foo");

                // Then
                Assert.IsFalse(result);
                CollectionAssert.AreEquivalent(
                    new Dictionary<string, IFileProvider>
                {
                    { NormalizedPath.DefaultFileProvider.Scheme, defaultProvider }
                }, collection.Providers);
            }

            [Test]
            public void ThrowsForNullName()
            {
                // Given
                IFileProvider defaultProvider = new TestFileProvider();
                IFileProvider newProvider = new TestFileProvider();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);
                collection.Add("foo", newProvider);

                // When, Then
                Assert.Throws<ArgumentNullException>(() => collection.Remove(null));
            }

            [Test]
            public void ThrowsForDefaultProvider()
            {
                // Given
                IFileProvider defaultProvider = new TestFileProvider();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When, Then
                Assert.Throws<ArgumentException>(() => collection.Remove(string.Empty));
            }
        }

        public class GetTests : FileProviderCollectionFixture
        {
            [Test]
            public void ReturnsProvider()
            {
                // Given
                IFileProvider defaultProvider = new TestFileProvider();
                IFileProvider newProvider = new TestFileProvider();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);
                collection.Add("foo", newProvider);

                // When
                IFileProvider result = collection.Get("foo");

                // Then
                Assert.AreEqual(newProvider, result);
            }

            [Test]
            public void ReturnsDefaultProvider()
            {
                // Given
                IFileProvider defaultProvider = new TestFileProvider();
                IFileProvider newProvider = new TestFileProvider();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);
                collection.Add("foo", newProvider);

                // When
                IFileProvider result = collection.Get(NormalizedPath.DefaultFileProvider.Scheme);

                // Then
                Assert.AreEqual(defaultProvider, result);
            }

            [Test]
            public void ThrowsForNullName()
            {
                // Given
                IFileProvider defaultProvider = new TestFileProvider();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When, Then
                Assert.Throws<ArgumentNullException>(() => collection.Get(null));
            }

            [Test]
            public void ThrowsForNotFound()
            {
                // Given
                IFileProvider defaultProvider = new TestFileProvider();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When, Then
                Assert.Throws<KeyNotFoundException>(() => collection.Get("foo"));
            }
        }

        public class TryGetTests : FileProviderCollectionFixture
        {
            [Test]
            public void ReturnsTrueForProvider()
            {
                // Given
                IFileProvider defaultProvider = new TestFileProvider();
                IFileProvider newProvider = new TestFileProvider();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);
                collection.Add("foo", newProvider);

                // When
                IFileProvider providerResult;
                bool result = collection.TryGet("foo", out providerResult);

                // Then
                Assert.AreEqual(newProvider, providerResult);
                Assert.IsTrue(result);
            }

            [Test]
            public void ReturnsTrueForDefaultProvider()
            {
                // Given
                IFileProvider defaultProvider = new TestFileProvider();
                IFileProvider newProvider = new TestFileProvider();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);
                collection.Add("foo", newProvider);

                // When
                IFileProvider providerResult;
                bool result = collection.TryGet(NormalizedPath.DefaultFileProvider.Scheme, out providerResult);

                // Then
                Assert.AreEqual(defaultProvider, providerResult);
                Assert.IsTrue(result);
            }

            [Test]
            public void ReturnsFalseIfNotFound()
            {
                // Given
                IFileProvider defaultProvider = new TestFileProvider();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When
                IFileProvider providerResult;
                bool result = collection.TryGet("foo", out providerResult);

                // Then
                Assert.IsFalse(result);
            }

            [Test]
            public void ThrowsForNullName()
            {
                // Given
                IFileProvider defaultProvider = new TestFileProvider();
                FileProviderCollection collection = new FileProviderCollection(defaultProvider);

                // When, Then
                IFileProvider providerResult;
                Assert.Throws<ArgumentNullException>(() => collection.TryGet(null, out providerResult));
            }
        }
    }
}
