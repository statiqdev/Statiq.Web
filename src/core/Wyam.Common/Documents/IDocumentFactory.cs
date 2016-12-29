using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.Common.Documents
{
    public interface IDocumentFactory
    {
        /// <summary>
        /// Gets a new document with default initial metadata.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <returns>The new document.</returns>
        IDocument GetDocument(IExecutionContext context);

        /// <summary>
        /// Clones the specified source document with a new source, new content, and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <c>AsNewocuments()</c> was called on the module.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="source">The source (if the source document contains a source, then this is ignored and the source document's source is used instead).</param>
        /// <param name="content">The content.</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The cloned or new document.</returns>
        IDocument GetDocument(IExecutionContext context, IDocument sourceDocument, FilePath source, string content, IEnumerable<KeyValuePair<string, object>> items = null);

        /// <summary>
        /// Clones the specified source document with a new source and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <c>AsNewocuments()</c> was called on the module.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="source">The source (if the source document contains a source, then this is ignored and the source document's source is used instead).</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The cloned or new document.</returns>
        IDocument GetDocument(IExecutionContext context, IDocument sourceDocument, FilePath source, IEnumerable<KeyValuePair<string, object>> items = null);

        /// <summary>
        /// Clones the specified source document with new content and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <c>AsNewocuments()</c> was called on the module.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="content">The content.</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The cloned or new document.</returns>
        IDocument GetDocument(IExecutionContext context, IDocument sourceDocument, string content, IEnumerable<KeyValuePair<string, object>> items = null);

        /// <summary>
        /// Clones the specified source document with a new source, new content stream, and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <c>AsNewocuments()</c> was called on the module.
        /// If <paramref name="disposeStream"/> is true (which it is by default), the provided 
        /// <see cref="Stream"/> will automatically be disposed when the document is disposed (I.e., the 
        /// document takes ownership of the <see cref="Stream"/>).
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="source">The source (if the source document contains a source, then this is ignored and the source document's source is used instead).</param>
        /// <param name="stream">The content stream.</param>
        /// <param name="items">The metadata items.</param>
        /// <param name="disposeStream">If set to <c>true</c> the provided <see cref="Stream"/> is disposed when the document is.</param>
        /// <returns>The cloned or new document.</returns>
        IDocument GetDocument(IExecutionContext context, IDocument sourceDocument, FilePath source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true);

        /// <summary>
        /// Clones the specified source document with a new content stream, and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <c>AsNewocuments()</c> was called on the module.
        /// If <paramref name="disposeStream"/> is true (which it is by default), the provided 
        /// <see cref="Stream"/> will automatically be disposed when the document is disposed (I.e., the 
        /// document takes ownership of the <see cref="Stream"/>).
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="stream">The content stream.</param>
        /// <param name="items">The metadata items.</param>
        /// <param name="disposeStream">If set to <c>true</c> the provided <see cref="Stream"/> is disposed when the document is.</param>
        /// <returns>The cloned or new document.</returns>
        IDocument GetDocument(IExecutionContext context, IDocument sourceDocument, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true);

        /// <summary>
        /// Clones the specified source document with identical content and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <c>AsNewocuments()</c> was called on the module.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The cloned or new document.</returns>
        IDocument GetDocument(IExecutionContext context, IDocument sourceDocument, IEnumerable<KeyValuePair<string, object>> items);
    }
}
