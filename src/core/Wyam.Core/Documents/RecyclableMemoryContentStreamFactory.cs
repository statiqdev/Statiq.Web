using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IO;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Core.Documents
{
    internal class RecyclableMemoryContentStreamFactory : IContentStreamFactory
    {
        private const int BlockSize = 16384;
        private readonly object _managerLock = new object();
        private RecyclableMemoryStreamManager _manager;

        public Stream GetStream(IExecutionContext context, string content = null)
        {
            lock (_managerLock)
            {
                if (_manager == null)
                {
                    _manager = new RecyclableMemoryStreamManager(
                        BlockSize,
                        RecyclableMemoryStreamManager.DefaultLargeBufferMultiple,
                        RecyclableMemoryStreamManager.DefaultMaximumBufferSize)
                    {
                        MaximumFreeSmallPoolBytes = BlockSize * 32768L * 2, // 1 GB
                    };
                }
            }

            if (string.IsNullOrEmpty(content))
            {
                return _manager.GetStream();
            }
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            return _manager.GetStream(null, contentBytes, 0, contentBytes.Length);
        }
    }
}
