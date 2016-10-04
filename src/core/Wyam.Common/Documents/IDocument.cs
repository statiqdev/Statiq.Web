using System;
using System.Collections.Generic;
using System.IO;
using Wyam.Common.IO;
using Wyam.Common.Meta;

namespace Wyam.Common.Documents
{
    /// <summary>
    /// Contains content and metadata for each item as it propagates through the pipeline.
    /// </summary>
    /// <remarks>
    /// Documents are immutable so you must call one of the <c>GetDocument</c> methods of <see cref="IDocumentFactory"/> 
    /// to create a new document. Implements <see cref="IMetadata"/> and all metadata calls are passed through
    /// to the document's internal <see cref="IMetadata"/> instance (exposed via the <see cref="Metadata"/>
    /// property). Note that both the <see cref="Content"/> property and the result of the <see cref="GetStream"/>
    /// method are guaranteed not to be null. When a document is created, either a string or a <see cref="Stream"/>
    /// is provided. Whenever the other of the two is requested, the system will convert the current representation
    /// for you.
    /// </remarks>
    public interface IDocument : IMetadata, IDisposable
    {
        /// <summary>An identifier for the document meant to reflect the source of the data. These should be unique (such as a file name).</summary>
        /// <value>The source of the document, or <c>null</c> if the document doesn't have a source.</value>
        FilePath Source { get; }

        /// <summary>
        /// Gets a string representation of the source that's guaranteed non-null, used primarily for trace messages.
        /// </summary>
        string SourceString();

        /// <summary>An identifier that is generated when the document is created and stays the same after cloning.</summary>
        /// <value>The identifier.</value>
        string Id { get; }

        /// <summary>Gets the metadata associated with this document.</summary>
        /// <value>The metadata associated with this document.</value>
        IMetadata Metadata { get; }

        /// <summary>Gets the content associated with this document as a string.</summary>
        /// <value>The content associated with this document.</value>
        string Content { get; }

        /// <summary>Gets the content associated with this document as a <see cref="Stream"/>.</summary>
        /// <returns>A <see cref="Stream"/> of the content associated with this document.</returns>
        Stream GetStream();
    }
}