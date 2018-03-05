using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam.Common.Documents
{
    /// <summary>
    /// This creates a file stream that deletes the underlying file on dispose.
    /// </summary>
    internal class FileContentStream : Stream
    {
        private readonly IFile _file;
        private Stream _stream;
        private bool _disposed;

        public FileContentStream(IFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }
            _file = file;
        }

        protected override void Dispose(bool disposing)
        {
            CheckDisposed();
            _disposed = true;
            _stream?.Dispose();
            if (_file.Exists)
            {
                _file.Delete();
            }
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(FileContentStream));
            }
        }

        private Stream GetStream() => _stream ?? (_stream = _file.Open());

        public override object InitializeLifetimeService()
        {
            CheckDisposed();
            return GetStream().InitializeLifetimeService();
        }
        
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            CheckDisposed();
            return GetStream().CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override void Close()
        {
            CheckDisposed();
            GetStream().Close();
        }

        public override void Flush()
        {
            CheckDisposed();
            GetStream().Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            CheckDisposed();
            return GetStream().FlushAsync(cancellationToken);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            CheckDisposed();
            return GetStream().BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            CheckDisposed();
            return GetStream().EndRead(asyncResult);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            CheckDisposed();
            return GetStream().ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            CheckDisposed();
            return GetStream().BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            CheckDisposed();
            GetStream().EndWrite(asyncResult);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            CheckDisposed();
            return GetStream().WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckDisposed();
            return GetStream().Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            CheckDisposed();
            GetStream().SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            return GetStream().Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            CheckDisposed();
            return GetStream().ReadByte();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            GetStream().Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            CheckDisposed();
            GetStream().WriteByte(value);
        }

        public override bool CanRead
        {
            get
            {
                CheckDisposed();
                return GetStream().CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                CheckDisposed();
                return GetStream().CanSeek;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                CheckDisposed();
                return GetStream().CanTimeout;
            }
        }

        public override bool CanWrite
        {
            get
            {
                CheckDisposed();
                return GetStream().CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                CheckDisposed();
                return GetStream().Length;
            }
        }

        public override long Position
        {
            get
            {
                CheckDisposed();
                return GetStream().Position;
            }
            set
            {
                CheckDisposed();
                GetStream().Position = value;
            }
        }

        public override int ReadTimeout
        {
            get
            {
                CheckDisposed();
                return GetStream().ReadTimeout;
            }
            set
            {
                CheckDisposed();
                GetStream().ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                CheckDisposed();
                return GetStream().WriteTimeout;
            }
            set
            {
                CheckDisposed();
                GetStream().WriteTimeout = value;
            }
        }
    }
}
