using System;
using System.Buffers;
using System.IO;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;

namespace Wyam.Razor
{
    // Coordinates the memory pool between a Razor view and partials, etc.
    // Underlying implementation is not thread safe, so this wraps it
    // See https://github.com/aspnet/Mvc/issues/5106
    internal class SafeMemoryPoolViewBufferScope : IViewBufferScope, IDisposable
    {
        private readonly object _lock = new object();
        private readonly MemoryPoolViewBufferScope _pool;

        public SafeMemoryPoolViewBufferScope(ArrayPool<ViewBufferValue> viewBufferPool, ArrayPool<char> charPool)
        {
            _pool = new MemoryPoolViewBufferScope(viewBufferPool, charPool);
        }

        public ViewBufferValue[] GetPage(int pageSize)
        {
            lock (_lock)
            {
                return _pool.GetPage(pageSize);
            }
        }

        public void ReturnSegment(ViewBufferValue[] segment)
        {
            lock (_lock)
            {
                _pool.ReturnSegment(segment);
            }
        }

        public PagedBufferedTextWriter CreateWriter(TextWriter writer) => _pool.CreateWriter(writer);

        public void Dispose() => _pool.Dispose();
    }
}