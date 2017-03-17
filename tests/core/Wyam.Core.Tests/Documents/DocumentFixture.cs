using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;
using Wyam.Core.Documents;
using Wyam.Core.Meta;
using Wyam.Core.Execution;
using Wyam.Testing;

namespace Wyam.Core.Tests.Documents
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class DocumentFixture : BaseFixture
    {
        public class ConstructorTests : DocumentFixture
        {
            [Test]
            public void IdIsNotTheSameForDifferentDocuments()
            {
                // Given
                MetadataDictionary initialMetadata = new MetadataDictionary();

                // When
                Document a = new Document(initialMetadata);
                Document b = new Document(initialMetadata);

                // Then
                Assert.AreNotEqual(a.Id, b.Id);
            }
        }

        public class CloneTests : DocumentFixture
        {
            [Test]
            public void IdIsTheSameAfterClone()
            {
                // Given
                MetadataDictionary initialMetadata = new MetadataDictionary();
                Document document = new Document(initialMetadata);

                // When
                IDocument cloned = new Document(document, new MetadataItems());

                // Then
                Assert.AreEqual(document.Id, cloned.Id);
            }
        }

        public class WitoutSettingsTests : DocumentFixture
        {
            [Test]
            public void ReturnsMetadataWithoutSettings()
            {
                // Given
                MetadataDictionary initialMetadata = new MetadataDictionary();
                initialMetadata.Add("A", "a");
                Document document = new Document(initialMetadata);
                Document cloned = new Document(document, new MetadataItems {{"B", "b"}});

                // When
                string initialA = document.String("A");
                string initialB = document.String("B");
                string clonedA = cloned.String("A");
                string clonedB = cloned.String("B");
                string withoutA = cloned.WithoutSettings.String("A");
                string withoutB = cloned.WithoutSettings.String("B");

                // Then
                Assert.AreEqual("a", initialA);
                Assert.IsNull(initialB);
                Assert.AreEqual("a", clonedA);
                Assert.AreEqual("b", clonedB);
                Assert.IsNull(withoutA);
                Assert.AreEqual("b", withoutB);
            }
        }

        public class DisposeTests : DocumentFixture
        {
            [Test]
            public void StreamIsDisposedCorrectlyAfterClone()
            {
                // Given
                MetadataDictionary initialMetadata = new MetadataDictionary();
                DisposeCheckStream stream = new DisposeCheckStream();
                Document originalDoc = new Document(initialMetadata, new FilePath("Test", PathKind.Absolute), 
                    stream, Array.Empty<KeyValuePair<string, object>>(), true);
                Document clonedDoc = new Document(originalDoc, Array.Empty<KeyValuePair<string, object>>());

                // When
                originalDoc.Dispose();
                bool originalDocDisposedStream = stream.Disposed;
                clonedDoc.Dispose();
                bool clonedDocDisposedStream = stream.Disposed;

                // Then
                Assert.AreEqual(false, originalDocDisposedStream);
                Assert.AreEqual(true, clonedDocDisposedStream);
            }

            private class DisposeCheckStream : Stream
            {
                public bool Disposed { get; set; }

                protected override void Dispose(bool disposing)
                {
                    Disposed = true;
                    base.Dispose(disposing);
                }

                public override void Flush()
                {
                    throw new NotImplementedException();
                }

                public override long Seek(long offset, SeekOrigin origin)
                {
                    throw new NotImplementedException();
                }

                public override void SetLength(long value)
                {
                    throw new NotImplementedException();
                }

                public override int Read(byte[] buffer, int offset, int count)
                {
                    throw new NotImplementedException();
                }

                public override void Write(byte[] buffer, int offset, int count)
                {
                    throw new NotImplementedException();
                }

                public override bool CanRead { get; } = true;
                public override bool CanSeek { get; }
                public override bool CanWrite { get; }
                public override long Length { get; }
                public override long Position { get; set; }
            }
        }
    }
}
