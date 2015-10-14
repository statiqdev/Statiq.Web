using System.Collections.Generic;
using System.IO;

namespace Wyam.Common.Documents
{
    /// <summary>
    /// Contains content and metadata for each item as it propagates through the pipeline.
    /// </summary>
    /// <remarks>
    /// Documents are immutable so you must call one of the <c>Clone</c> methods to create a new document. 
    /// Implements <see cref="IMetadata"/> and all metadata calls are passed through to the document's internal 
    /// <see cref="IMetadata"/> instance (exposed via the <see cref="Metadata"/> property). Note that both the 
    /// <see cref="Content"/> property and the result of the <see cref="GetStream"/> method are guaranteed not 
    /// to be null. When a document is created, either a string or a <see cref="Stream"/> is provided. Whenever 
    /// the other of the two is requested, the system will convert the current representation for you.
    /// </remarks>
    public interface IDocument : IMetadata
    {
        /// <summary>An identifier for the document meant to reflect the source of the data. These should be unique (such as a file name).</summary>
        /// <value>The source of the document.</value>
        string Source { get; }

        /// <summary>Gets the metadata associated with this document.</summary>
        /// <value>The metadata associated with this document.</value>
        IMetadata Metadata { get; }

        /// <summary>Gets the content associated with this document as a string.</summary>
        /// <value>The content associated with this document.</value>
        string Content { get; }

        /// <summary>Gets the content associated with this document as a <see cref="Stream"/>.</summary>
        /// <returns>A <see cref="Stream"/> of the content associated with this document.</returns>
        Stream GetStream();

        /// <summary>
        /// Clones the current document with a new source, new content, and additional metadata (all existing metadata is retained).
        /// </summary>
        /// <param name="source">The new source.</param>
        /// <param name="content">The new content.</param>
        /// <param name="items">Additional metadata items.</param>
        /// <returns>The newly cloned document.</returns>
        IDocument Clone(string source, string content, IEnumerable<KeyValuePair<string, object>> items = null);

        /// <summary>
        /// Clones the current document with new content and additional metadata (all existing metadata is retained).
        /// </summary>
        /// <param name="content">The new content.</param>
        /// <param name="items">Additional metadata items.</param>
        /// <returns>The newly cloned document.</returns>
        IDocument Clone(string content, IEnumerable<KeyValuePair<string, object>> items = null);

        /// <summary>
        /// Clones the current document with a new source, new content stream, and additional metadata (all existing 
        /// metadata is retained). If <paramref name="disposeStream"/> is true (which it is by default), the provided 
        /// <see cref="Stream"/> will automatically be disposed when the document is disposed (I.e., the cloned 
        /// document takes ownership of the <see cref="Stream"/>).
        /// </summary>
        /// <param name="source">The new source.</param>
        /// <param name="stream">The new content stream.</param>
        /// <param name="items">Additional metadata items.</param>
        /// <param name="disposeStream">If set to <c>true</c> the provided <see cref="Stream"/> is disposed when the document is.</param>
        /// <returns>The newly cloned document.</returns>
        IDocument Clone(string source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true);

        /// <summary>
        /// Clones the current document with a new content stream, and additional metadata (all existing 
        /// metadata is retained). If <paramref name="disposeStream"/> is true (which it is by default), the provided 
        /// <see cref="Stream"/> will automatically be disposed when the document is disposed (I.e., the cloned 
        /// document takes ownership of the <see cref="Stream"/>).
        /// </summary>
        /// <param name="stream">The new content stream.</param>
        /// <param name="items">Additional metadata items.</param>
        /// <param name="disposeStream">If set to <c>true</c> the provided <see cref="Stream"/> is disposed when the document is.</param>
        /// <returns>The newly cloned document.</returns>
        IDocument Clone(Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true);

        /// <summary>
        /// Clones the current document with identical content and additional metadata (all existing metadata is retained).
        /// </summary>
        /// <param name="items">Additional metadata items.</param>
        /// <returns>The newly cloned document.</returns>
        IDocument Clone(IEnumerable<KeyValuePair<string, object>> items);
    }
}