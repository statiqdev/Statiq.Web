using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;

namespace Wyam.Core.Tests
{
    [TestFixture]
    public class MetadataFixture
    {
        [Test]
        public void CanSetStaticMetadata()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);

            // When
            metadata.Set("A", "a");

            // Then
            Assert.AreEqual("a", metadata["A"]);
        }
        
        [Test]
        public void CanSetDynamicMetadata()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            dynamic dyn = metadata;

            // When
            dyn.A = "a";

            // Then
            Assert.AreEqual("a", metadata["A"]);
        }

        [Test]
        public void IsReadOnlyDoesNotAllowEditsForStaticMetadata()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);

            // When
            metadata.IsReadOnly = true;

            // Then
            Assert.Throws<InvalidOperationException>(() => metadata["A"] = "a");
        }
        
        [Test]
        public void IsReadOnlyDoesNotAllowEditsForDynamicMetadata()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            dynamic dyn = metadata;

            // When
            metadata.IsReadOnly = true;

            // Then
            Assert.Throws<InvalidOperationException>(() => dyn.A = "a");
        }

        [Test]
        public void GetMissingKeyReturnsNullForStaticMetadata()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);

            // When
            object value = metadata.Get("A");

            // Then
            Assert.IsNull(value);
        }

        [Test]
        public void TryGetMissingKeyReturnsNullForStaticMetadata()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);

            // When
            object value;
            bool found = metadata.TryGet("A", out value);

            // Then
            Assert.IsFalse(found);
            Assert.IsNull(value);
        }

        [Test]
        public void MissingKeyReturnsNullForDynamicMetadata()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            dynamic dyn = metadata;

            // When
            object value = dyn.A;

            // Then
            Assert.IsNull(value);
        }

        [Test]
        public void ContainsReturnsTrueForValidValue()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            metadata.Set("A", "a");

            // When
            bool contains = metadata.Contains("A");

            // Then
            Assert.IsTrue(contains);
        }

        [Test]
        public void ContainsReturnsFalseForInvalidValue()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            metadata.Set("A", "a");

            // When
            bool contains = metadata.Contains("B");

            // Then
            Assert.IsFalse(contains);
        }

        [Test]
        public void TryGetReturnsTrueForValidValue()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            metadata.Set("A", "a");

            // When
            object value;
            bool contains = metadata.TryGet("A", out value);

            // Then
            Assert.IsTrue(contains);
            Assert.AreEqual("a", value);
        }        

        [Test]
        public void TryGetReturnsFalseForInvalidValue()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            metadata.Set("A", "a");

            // When
            object value;
            bool contains = metadata.TryGet("B", out value);

            // Then
            Assert.IsFalse(contains);
            Assert.AreEqual(null, value);
        }

        [Test]
        public void IndexerReturnsValidValue()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            metadata["A"] = "a";

            // When
            object value = metadata["A"];

            // Then
            Assert.AreEqual("a", value);
        } 

        [Test]
        public void IndexerReturnsNullForInvalidValue()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            metadata["A"] = "a";

            // When
            object value = metadata["B"];

            // Then
            Assert.AreEqual(null, value);
        } 

        [Test]
        public void CloneContainsPreviousValues()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            metadata.Set("A", "a");
            Metadata clone = metadata.Clone();
            clone.Set("B", "b");

            // When
            object value = clone.Get("A");

            // Then
            Assert.AreEqual("a", value);
        }

        [Test]
        public void ClonedDoesNotContainNewValues()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            metadata.Set("A", "a");
            Metadata clone = metadata.Clone();
            clone.Set("B", "b");      

            // When
            object value = metadata.Get("B");

            // Then
            Assert.AreEqual(null, value);
        }

        [Test]
        public void CloneContainsNewValues()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            metadata.Set("A", "a");
            Metadata clone = metadata.Clone();
            clone.Set("B", "b");      

            // When
            object value = clone.Get("B");

            // Then
            Assert.AreEqual("b", value);
        }

        [Test]
        public void CloneReplacesValue()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            metadata.Set("A", "a");
            Metadata clone = metadata.Clone();
            clone.Set("A", "b");      

            // When
            object value = metadata.Get("A");
            object cloneValue = clone.Get("A");

            // Then
            Assert.AreEqual("a", value);
            Assert.AreEqual("b", cloneValue);
        }
    }
}
