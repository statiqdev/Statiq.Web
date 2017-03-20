using System;
using System.IO;
using System.Text;
using Wyam.Common.Execution;

namespace Wyam.Common.Documents
{
    /// <summary>
    /// Provides content streams that are stored in memory without pooling. This trades better performance
    /// for increased memory usage.
    /// </summary>
    public class MemoryContentStreamFactory : IContentStreamFactory
    {
        /// <inheritdoc />
        public Stream GetStream(IExecutionContext context, string content = null) =>
            string.IsNullOrEmpty(content) ? new MemoryStream() : new MemoryStream(Encoding.UTF8.GetBytes(content));
    }
}