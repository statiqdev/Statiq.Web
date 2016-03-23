using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Testing.IO
{
    // Provides a writeable stream to a StringBuilder
    // Initially based on code from Simple.Web (https://github.com/markrendle/Simple.Web) 
    public class StringBuilderStream : Stream
    {
        private readonly MemoryStream _buffer;
        private readonly StreamReader _bufferReader;
        private readonly StringBuilder _resultBuilder;

        public StringBuilderStream(StringBuilder resultBuilder)
        {
            _buffer = new MemoryStream();
            _bufferReader = new StreamReader(_buffer, true);
            _resultBuilder = resultBuilder;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => 0;

        public override long Position { get; set; }

        public override void Flush()
        {
            _buffer.Position = 0;
            _resultBuilder.Append(_bufferReader.ReadToEnd());
            _buffer.SetLength(0);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _buffer.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            Flush();
            base.Dispose(disposing);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return _buffer.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            _buffer.EndWrite(asyncResult);
            Flush();
        }
    }
}
