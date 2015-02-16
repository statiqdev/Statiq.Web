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
        // TODO: Test for setting value for static metadata
        // TODO: Test for setting value for dynamic metadata
        // TODO: Test for reading value for static metadata
        // TODO: Test for reading value for dynamic metadata
        // TODO: Tests for various static metadata methods (Get(), TryGet(), etc.)

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
    }
}
