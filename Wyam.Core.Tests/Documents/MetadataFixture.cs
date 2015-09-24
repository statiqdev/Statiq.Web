using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Core;
using Wyam.Core.Documents;

namespace Wyam.Core.Tests.Documents
{
    [TestFixture]
    public class MetadataFixture
    {
        [Test]
        public void CanCloneWithNewValues()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
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
            engine.Trace.AddListener(new TestTraceListener());
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
            engine.Trace.AddListener(new TestTraceListener());
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
            engine.Trace.AddListener(new TestTraceListener());
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
            engine.Trace.AddListener(new TestTraceListener());
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
            engine.Trace.AddListener(new TestTraceListener());
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
            engine.Trace.AddListener(new TestTraceListener());
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
            engine.Trace.AddListener(new TestTraceListener());
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
            engine.Trace.AddListener(new TestTraceListener());
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
            engine.Trace.AddListener(new TestTraceListener());
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
            engine.Trace.AddListener(new TestTraceListener());
            engine.Metadata["A"] = "a";
            Metadata metadata = new Metadata(engine);

            // When
            Metadata clone = metadata.Clone(new Dictionary<string, object> { { "A", "b" } });

            // Then
            Assert.AreEqual("a", metadata["A"]);
            Assert.AreEqual("b", clone["A"]);
        }

        [Test]
        public void GetWithMetadataValueReturnsCorrectResult()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.Metadata["A"] = new SimpleMetadataValue { Value = "a" };
            Metadata metadata = new Metadata(engine);

            // When
            object value = metadata.Get("A");

            // Then
            Assert.AreEqual("a", value);
        }

        [Test]
        public void IndexerWithMetadataValueReturnsCorrectResult()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.Metadata["A"] = new SimpleMetadataValue { Value = "a" };
            Metadata metadata = new Metadata(engine);

            // When
            object value = metadata["A"];

            // Then
            Assert.AreEqual("a", value);
        }

        [Test]
        public void TryGetValueWithMetadataValueReturnsCorrectResult()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.Metadata["A"] = new SimpleMetadataValue { Value = "a" };
            Metadata metadata = new Metadata(engine);

            // When
            object value;
            bool contains = metadata.TryGetValue("A", out value);

            // Then
            Assert.IsTrue(contains);
            Assert.AreEqual("a", value);
        }

        [Test]
        public void GetWithDerivedMetadataValueReturnsCorrectResult()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.Metadata["X"] = "x";
            engine.Metadata["A"] = new DerivedMetadataValue { Key = "X" };
            Metadata metadata = new Metadata(engine);

            // When
            object value = metadata.Get("A");

            // Then
            Assert.AreEqual("x", value);
        }

        [Test]
        public void GetWithMetadataValueCalledForEachRequest()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            SimpleMetadataValue metadataValue = new SimpleMetadataValue { Value = "a" };
            engine.Metadata["A"] = metadataValue;
            Metadata metadata = new Metadata(engine);

            // When
            object value = metadata.Get("A");
            value = metadata.Get("A");
            value = metadata.Get("A");

            // Then
            Assert.AreEqual("a", value);
            Assert.AreEqual(3, metadataValue.Calls);
        }

        [Test]
        public void EnumeratingMetadataValuesReturnsCorrectResults()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            engine.Metadata["A"] = new SimpleMetadataValue { Value = "a" };
            engine.Metadata["B"] = new SimpleMetadataValue { Value = "b" };
            engine.Metadata["C"] = new SimpleMetadataValue { Value = "c" };
            Metadata metadata = new Metadata(engine);

            // When
            object[] values = metadata.Select(x => x.Value).ToArray();

            // Then
            CollectionAssert.AreEquivalent(new [] { "a", "b", "c" }, values);
        }

        private class SimpleMetadataValue : IMetadataValue
        {
            public string Value { get; set; }
            public int Calls { get; set; }

            public object Get(string key, IMetadata metadata)
            {
                Calls++;
                return Value;
            }
        }

        private class DerivedMetadataValue : IMetadataValue
        {
            public string Key { get; set; }

            public object Get(string key, IMetadata metadata)
            {
                return metadata[Key];
            }
        }
    }
}
