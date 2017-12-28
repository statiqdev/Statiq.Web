using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wyam.Testing.IO
{
    // Provides a writeable stream to a StringBuilder
    // Initially based on code from Simple.Web (https://github.com/markrendle/Simple.Web)
    public class StringBuilderStream : Stream
    {
        private readonly Random _random = new Random();
        private readonly MemoryStream _buffer;
        private readonly StreamReader _bufferReader;
        private readonly StringBuilder _resultBuilder;

        public StringBuilderStream(StringBuilder resultBuilder)
        {
            _buffer = new MemoryStream();

            // Copy the old result into the current buffer.
            StreamWriter writer = new StreamWriter(_buffer);
            writer.Write(resultBuilder.ToString());
            writer.Flush();
            _buffer.Position = 0;

            _bufferReader = new StreamReader(_buffer, true);
            _resultBuilder = resultBuilder;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => _buffer.Length;

        public override long Position
        {
            get => _buffer.Position;
            set => _buffer.Position = value;
        }

        public override void Flush()
        {
            _buffer.Position = 0;
            _resultBuilder.Clear();
            _resultBuilder.Append(_bufferReader.ReadToEnd());
            _buffer.SetLength(0);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            _buffer.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Thread.Sleep(_random.Next(200)); // Sleep for a random amount to simulate realistic file operations
            _buffer.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            Flush();
            base.Dispose(disposing);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            Thread.Sleep(_random.Next(200)); // Sleep for a random amount to simulate realistic file operations
            return _buffer.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            _buffer.EndWrite(asyncResult);
            Flush();
        }
    }
}
