using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Microsoft.Framework.Expiration.Interfaces;
using Wyam.Common.IO;
using IFileProvider = Microsoft.Extensions.FileProviders.IFileProvider;

namespace Wyam.Razor
{

    /// <summary>
    /// A IFileProvider that provides files based on their stream for use with arbitrary documents.
    /// </summary>
    public class WyamStreamFileProvider : IFileProvider
    {
        private readonly WyamFileProvider _wyamFileProvider;
        private readonly Stream _stream;

        public WyamStreamFileProvider(WyamFileProvider wyamFileProvider, Stream stream)
        {
            _wyamFileProvider = wyamFileProvider;
            _stream = stream;
        }

        public IFileInfo GetFileInfo(string subpath) => 
            new DocumentFileInfo(_wyamFileProvider.GetFileInfo(subpath), _stream);

        public IDirectoryContents GetDirectoryContents(string subpath) => 
            _wyamFileProvider.GetDirectoryContents(subpath);

        IChangeToken IFileProvider.Watch(string filter) => new EmptyChangeToken();

        private class DocumentFileInfo : IFileInfo
        {
            private readonly IFileInfo _info;
            private readonly Stream _stream;

            public DocumentFileInfo(IFileInfo info, Stream stream)
            {
                _info = info;
                _stream = stream;
            }

            public bool Exists => true;

            public long Length => _stream.Length;

            public string PhysicalPath => _info.PhysicalPath;

            public string Name => _info.Name;

            public DateTimeOffset LastModified => DateTimeOffset.Now;

            public bool IsDirectory => false;

            public Stream CreateReadStream() => new NonDisposableStreamWrapper(_stream);
        }

        // This prevents the underlying stream from being disposed when the wrapper is
        private class NonDisposableStreamWrapper : Stream
        {
            private readonly Stream _stream;

            public NonDisposableStreamWrapper(Stream stream)
            {
                _stream = stream;
            }

            public override void Flush()
            {
                _stream.Flush();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _stream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                _stream.SetLength(value);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _stream.Read(buffer, offset, count);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _stream.Write(buffer, offset, count);
            }

            public override bool CanRead => _stream.CanRead;
            public override bool CanSeek => _stream.CanSeek;
            public override bool CanWrite => _stream.CanWrite;
            public override long Length => _stream.Length;

            public override long Position
            {
                get { return _stream.Position; }
                set { _stream.Position = value; }
            }
        }
    }
}
