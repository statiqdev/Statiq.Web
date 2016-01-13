using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Core.Documents;
using Wyam.Core.Meta;

namespace Wyam.Core.Tests.Documents
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class MetadataAsFixture
    {
        [Test]
        public void ConvertIntToString()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Metadata metadata = new Metadata(engine);

            // When
            metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", 1) });
            IMetadata<string> metadataAs = metadata.MetadataAs<string>();

            // Then
            Assert.AreEqual(1, metadata["A"]);
            Assert.AreEqual("1", metadataAs["A"]);
        }

        [Test]
        public void ConvertStringToInt()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Metadata metadata = new Metadata(engine);

            // When
            metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", "1") });
            IMetadata<int> metadataAs = metadata.MetadataAs<int>();

            // Then
            Assert.AreEqual("1", metadata["A"]);
            Assert.AreEqual(1, metadataAs["A"]);
        }

        [Test]
        public void ConvertIntArrayToStringArray()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Metadata metadata = new Metadata(engine);

            // When
            metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", new int[] { 1, 2, 3 }) });
            IMetadata<string[]> metadataAs = metadata.MetadataAs<string[]>();

            // Then
            CollectionAssert.AreEqual(new int[] { 1, 2, 3 }, (IEnumerable)metadata["A"]);
            CollectionAssert.AreEqual(new string[] { "1", "2", "3" }, (IEnumerable)metadataAs["A"]);
        }

        [Test]
        public void ConvertStringArrayToIntArray()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Metadata metadata = new Metadata(engine);

            // When
            metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", new string[] { "1", "2", "3" }) });
            IMetadata<int[]> metadataAs = metadata.MetadataAs<int[]>();

            // Then
            CollectionAssert.AreEqual(new string[] { "1", "2", "3" }, (IEnumerable)metadata["A"]);
            CollectionAssert.AreEqual(new int[] { 1, 2, 3 }, (IEnumerable)metadataAs["A"]);
        }

        [Test]
        public void ConvertIntArrayToStringEnumerable()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Metadata metadata = new Metadata(engine);

            // When
            metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", new int[] { 1, 2, 3 }) });
            IMetadata<IEnumerable<string>> metadataAs = metadata.MetadataAs<IEnumerable<string>>();

            // Then
            CollectionAssert.AreEqual(new int[] { 1, 2, 3 }, (IEnumerable)metadata["A"]);
            CollectionAssert.AreEqual(new string[] { "1", "2", "3" }, metadataAs["A"]);
        }

        [Test]
        public void ConvertStringEnumerableToIntArray()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Metadata metadata = new Metadata(engine);

            // When
            metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", new List<string> { "1", "2", "3" }) });
            IMetadata<int[]> metadataAs = metadata.MetadataAs<int[]>();

            // Then
            CollectionAssert.AreEqual(new string[] { "1", "2", "3" }, (IEnumerable)metadata["A"]);
            CollectionAssert.AreEqual(new int[] { 1, 2, 3 }, (IEnumerable)metadataAs["A"]);
        }

        [Test]
        public void ConvertStringToIntArray()
        {
            // Given
            Engine engine = new Engine();
            engine.Trace.AddListener(new TestTraceListener());
            Metadata metadata = new Metadata(engine);

            // When
            metadata = metadata.Clone(new[] { new KeyValuePair<string, object>("A", "1") });
            IMetadata<int[]> metadataAs = metadata.MetadataAs<int[]>();

            // Then
            Assert.AreEqual("1", metadata["A"]);
            CollectionAssert.AreEqual(new int[] { 1 }, (IEnumerable)metadataAs["A"]);
        }
    }
}
