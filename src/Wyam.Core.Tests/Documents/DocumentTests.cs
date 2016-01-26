using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;
using Wyam.Core.Documents;
using Wyam.Core.Meta;
using Wyam.Core.Pipelines;
using Wyam.Testing;

namespace Wyam.Core.Tests.Documents
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class DocumentTests : BaseFixture
    {
        public class DisposeMethodTests : DocumentTests
        {
            [Test]
            public void StreamIsDisposedCorrectlyAfterClone()
            {
                // Given
                Pipeline pipeline = new Pipeline("Test", Array.Empty<IModule>());
                InitialMetadata initialMetadata = new InitialMetadata();
                DisposeCheckStream stream = new DisposeCheckStream();
                Document originalDoc = new Document(initialMetadata, pipeline, "Test", stream, null,
                    Array.Empty<KeyValuePair<string, object>>(), true);
                Document clonedDoc = (Document) originalDoc.Clone(Array.Empty<KeyValuePair<string, object>>());

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
