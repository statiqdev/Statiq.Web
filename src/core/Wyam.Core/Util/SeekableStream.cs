using System;
using System.IO;
using System.Diagnostics;

namespace Wyam.Core.Util
{
    internal class SeekableStream : Stream
    {
        private readonly Stream _stream;
        private readonly bool _disposeStream;
        private readonly MemoryStream _memoryStream = new MemoryStream();
        private bool _endOfStream = false;
        private bool _disposed = false;

        public SeekableStream(Stream stream, bool disposeStream)
        {
            if (!stream.CanRead)
            {
                throw new ArgumentException("Wrapped stream must be readable.");
            }
            _stream = stream;
            _disposeStream = disposeStream;
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;

        public override void Flush()
        {
            _stream.Flush();
        }

        public override long Length
        {
            get
            {
                CheckDisposed();
                if (!_endOfStream)
                {
                    Fill();
                }
                return _memoryStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                CheckDisposed();
                return _memoryStream.Position;
            }
            set
            {
                CheckDisposed();
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            int memoryBytes = 0;
            int streamBytes = 0;
            memoryBytes = _memoryStream.Read(buffer, offset, count);
            if ((count > memoryBytes) && (!_endOfStream))
            {
                int read = _stream.Read(buffer, offset + memoryBytes + streamBytes, count - memoryBytes - streamBytes);
                streamBytes += read;
                if (read == 0)
                {
                    _endOfStream = true;
                }
                _memoryStream.Write(buffer, offset + memoryBytes, streamBytes);
            }
            return memoryBytes + streamBytes;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckDisposed();
            long newPosition = 0L;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    newPosition = offset;
                    break;
                case SeekOrigin.Current:
                    newPosition = _memoryStream.Position + offset;
                    break;
                case SeekOrigin.End:
                    if (!_endOfStream)
                    {
                        Fill();
                    }
                    newPosition = _memoryStream.Length + offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin));
            }

            // Read additional bytes from the underlying stream if seeking past the end of the buffer
            if ((newPosition > _memoryStream.Length) && (!_endOfStream))
            {
                _memoryStream.Position = _memoryStream.Length;
                int bytesToRead = (int)(newPosition - _memoryStream.Length);
                byte[] buffer = new byte[1024];
                do
                {
                    bytesToRead -= Read(buffer, 0, (bytesToRead >= buffer.Length) ? buffer.Length : bytesToRead);
                }
                while ((bytesToRead > 0) && (!_endOfStream));
            }
            _memoryStream.Position = (newPosition <= _memoryStream.Length) ? newPosition : _memoryStream.Length;
            return 0;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        private void Fill()
        {
            if (_endOfStream)
            {
                return;
            }

            _memoryStream.Position = _memoryStream.Length;
            int bytesRead = 0;
            byte[] buffer = new byte[1024];
            do
            {
                bytesRead = _stream.Read(buffer, 0, buffer.Length);
                _memoryStream.Write(buffer, 0, bytesRead);
            }
            while (bytesRead != 0);
            _endOfStream = true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_disposed || !disposing)
            {
                return;
            }

            _memoryStream?.Dispose();
            if (_disposeStream)
            {
                _stream.Dispose();
            }
            _disposed = true;
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SeekableStream));
            }
        }
    }
}