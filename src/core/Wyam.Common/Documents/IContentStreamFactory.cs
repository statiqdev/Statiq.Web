using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;

namespace Wyam.Common.Documents
{
    /// <summary>
    /// Provides streams to use for document content.
    /// </summary>
    public interface IContentStreamFactory
    {
        /// <summary>
        /// Gets a <see cref="Stream"/> that can be used for document content. If <paramref name="content"/>
        /// is not null, the stream is initialized with the specified content.
        /// <remarks>The position should be set to the beginning of the stream when returned.</remarks>
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="content">Content to initialize the stream with.</param>
        /// <returns>A stream for document content.</returns>
        Stream GetStream(IExecutionContext context, string content = null);
    }
}
