using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Wyam.Core.Documents
{
    internal class BlockingStream : Stream
    {
        private readonly Stream _stream;
        private readonly Document _document;
        private bool _disposed;

        // Monitor.Enter should have already been called by this point
        public BlockingStream(Stream stream, Document document)
        {
            _stream = stream;
            _document = document;
        }

        protected override void Dispose(bool disposing)
        {
            CheckDisposed();
            _disposed = true;
            _document.ReleaseStream();
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(BlockingStream));
            }
        }

        // Delegating members

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            CheckDisposed();
            return _stream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override void Flush()
        {
            CheckDisposed();
            _stream.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            CheckDisposed();
            return _stream.FlushAsync(cancellationToken);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            CheckDisposed();
            return _stream.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            CheckDisposed();
            return _stream.EndRead(asyncResult);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            CheckDisposed();
            return _stream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            CheckDisposed();
            return _stream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            CheckDisposed();
            _stream.EndWrite(asyncResult);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            CheckDisposed();
            return _stream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckDisposed();
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            CheckDisposed();
            _stream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            return _stream.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            CheckDisposed();
            return _stream.ReadByte();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            _stream.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            CheckDisposed();
            _stream.WriteByte(value);
        }

        public override bool CanRead
        {
            get
            {
                CheckDisposed();
                return _stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                CheckDisposed();
                return _stream.CanSeek;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                CheckDisposed();
                return _stream.CanTimeout;
            }
        }

        public override bool CanWrite
        {
            get
            {
                CheckDisposed();
                return _stream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                CheckDisposed();
                return _stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                CheckDisposed();
                return _stream.Position;
            }
            set
            {
                CheckDisposed();
                _stream.Position = value;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                CheckDisposed();
                return _stream.ReadTimeout;
            }
            set
            {
                CheckDisposed();
                _stream.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                CheckDisposed();
                return _stream.WriteTimeout;
            }
            set
            {
                CheckDisposed();
                _stream.WriteTimeout = value;
            }
        }
    }
}
