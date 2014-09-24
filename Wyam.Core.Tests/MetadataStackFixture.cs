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
    public class MetadataStackFixture
    {
        [Test]
        public void LockedStackDoesNotAllowEdits()
        {
            // Given
            Engine engine = new Engine();
            MetadataStack metadataStack = new MetadataStack(engine);
            dynamic meta = metadataStack;
            meta.A = "a";
            meta.B = "b";

            // When
            metadataStack.Locked = true;

            // Then
            Assert.Throws<RuntimeBinderException>(() => meta.C = "c");  // Set
            Assert.AreEqual("b", meta.B);  // Pre-lock Get
            object c;
            Assert.Throws<RuntimeBinderException>(() => c = meta.C);  // Get
        }
    }
}
