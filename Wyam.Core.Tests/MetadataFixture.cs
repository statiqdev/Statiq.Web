using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;

namespace Wyam.Core.Tests
{
    [TestFixture]
    public class MetadataFixture
    {
        [Test]
        public void CanCloneWithNewValues()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);

            // When
            metadata = metadata.Clone(new [] { new KeyValuePair<string, object>("A", "a") });

            // Then
            Assert.AreEqual("a", metadata["A"]);
        }

        [Test]
        public void IndexerMissingKeyThrowsKeyNotFoundException()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);

            // When
            TestDelegate test = () =>
            {
                object value = metadata["A"];
            };

            // Then
            Assert.Throws<KeyNotFoundException>(test);
        }

        [Test]
        public void IndexerNullKeyThrowsKeyNotFoundException()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);

            // When
            TestDelegate test = () =>
            {
                object value = metadata[null];
            };

            // Then
            Assert.Throws<ArgumentNullException>(test);
        }
        
        [Test]
        public void ContainsKeyReturnsTrueForValidValue()
        {
            // Given
            Engine engine = new Engine();
            engine.Metadata["A"] = "a";
            Metadata metadata = new Metadata(engine);

            // When
            bool contains = metadata.ContainsKey("A");

            // Then
            Assert.IsTrue(contains);
        }

        [Test]
        public void ContainsReturnsFalseForInvalidValue()
        {
            // Given
            Engine engine = new Engine();
            engine.Metadata["A"] = "a";
            Metadata metadata = new Metadata(engine);

            // When
            bool contains = metadata.ContainsKey("B");

            // Then
            Assert.IsFalse(contains);
        }

        [Test]
        public void TryGetValueReturnsTrueForValidValue()
        {
            // Given
            Engine engine = new Engine();
            engine.Metadata["A"] = "a";
            Metadata metadata = new Metadata(engine);

            // When
            object value;
            bool contains = metadata.TryGetValue("A", out value);

            // Then
            Assert.IsTrue(contains);
            Assert.AreEqual("a", value);
        }        

        [Test]
        public void TryGetValueReturnsFalseForInvalidValue()
        {
            // Given
            Engine engine = new Engine();
            engine.Metadata["A"] = "a";
            Metadata metadata = new Metadata(engine);

            // When
            object value;
            bool contains = metadata.TryGetValue("B", out value);

            // Then
            Assert.IsFalse(contains);
            Assert.AreEqual(null, value);
        }

        [Test]
        public void CloneContainsPreviousValues()
        {
            // Given
            Engine engine = new Engine();
            engine.Metadata["A"] = "a";
            Metadata metadata = new Metadata(engine);

            // When
            Metadata clone = metadata.Clone(new Dictionary<string, object> { { "B", "b" } });

            // Then
            Assert.AreEqual("a", clone["A"]);
        }

        [Test]
        public void ClonedMetadataDoesNotContainNewValues()
        {
            // Given
            Engine engine = new Engine();
            engine.Metadata["A"] = "a";
            Metadata metadata = new Metadata(engine);

            // When
            Metadata clone = metadata.Clone(new Dictionary<string, object> { { "B", "b" } });

            // Then
            Assert.IsFalse(metadata.ContainsKey("B"));
        }

        [Test]
        public void CloneContainsNewValues()
        {
            // Given
            Engine engine = new Engine();
            engine.Metadata["A"] = "a";
            Metadata metadata = new Metadata(engine);

            // When
            Metadata clone = metadata.Clone(new Dictionary<string, object> { { "B", "b" } });

            // Then
            Assert.AreEqual("b", clone["B"]);
        }

        [Test]
        public void CloneReplacesValue()
        {
            // Given
            Engine engine = new Engine();
            engine.Metadata["A"] = "a";
            Metadata metadata = new Metadata(engine);

            // When
            Metadata clone = metadata.Clone(new Dictionary<string, object> { { "A", "b" } });

            // Then
            Assert.AreEqual("a", metadata["A"]);
            Assert.AreEqual("b", clone["A"]);
        }
    }
}
