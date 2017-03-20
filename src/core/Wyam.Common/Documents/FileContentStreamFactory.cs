using System;
using System.IO;
using Wyam.Common.Execution;
using Wyam.Common.IO;

namespace Wyam.Common.Documents
{
    /// <summary>
    /// Provides content streams that are backed by a file in the file system. This
    /// trades performance (disk I/O is considerably slower than memory) for a
    /// reduced memory footprint.
    /// </summary>
    public class FileContentStreamFactory : IContentStreamFactory
    {
        /// <inheritdoc />
        public Stream GetStream(IExecutionContext context, string content = null)
        {
            IFile tempFile = context.FileSystem.GetTempFile();
            if (!string.IsNullOrEmpty(content))
            {
                tempFile.WriteAllText(content);
            }
            return new FileContentStream(tempFile);
        }
    }
}