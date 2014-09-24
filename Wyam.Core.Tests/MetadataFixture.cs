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
        public void IsReadOnlyDoesNotAllowEdits()
        {
            // Given
            Engine engine = new Engine();
            Metadata metadata = new Metadata(engine);
            metadata.Set("A", "a");
            dynamic meta = metadata;
            meta.B = "b";

            // When
            metadata.IsReadOnly = true;

            // Then
            Assert.Throws<InvalidOperationException>(() => meta.C = "c");  // dynamic set
            Assert.Throws<InvalidOperationException>(() => metadata.Set("D", "d"));  // set
            Assert.AreEqual("a", meta.A);  // pre-lock dynamic get
            Assert.AreEqual("b", metadata.Get("B"));  // pre-lock get
            object c;
            Assert.Throws<KeyNotFoundException>(() => c = meta.C);  // dynamic get
            object d;
            Assert.Throws<KeyNotFoundException>(() => d = metadata.Get("D"));  // get
        }
    }
}
