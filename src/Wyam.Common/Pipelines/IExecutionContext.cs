using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Wyam.Common.Caching;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;

namespace Wyam.Common.Pipelines
{
    public interface IExecutionContext
    {
        byte[] RawConfigAssembly { get; }
        IEnumerable<Assembly> Assemblies { get; }
        IEnumerable<string> Namespaces { get; }
        IReadOnlyPipeline Pipeline { get; }
        IModule Module { get; }
        IExecutionCache ExecutionCache { get; }
        [Obsolete("This will be replaced by new IO functionality in the next release")]
        string RootFolder { get; }
        [Obsolete("This will be replaced by new IO functionality in the next release")]
        string InputFolder { get; }
        [Obsolete("This will be replaced by new IO functionality in the next release")]
        string OutputFolder { get; }
        IFileSystem FileSystem { get; }
        IDocumentCollection Documents { get; }

        string ApplicationInput { get; }

        /// <summary>
        /// Gets a new document with default initial metadata.
        /// </summary>
        /// <returns>The new document.</returns>
        IDocument GetDocument();

        /// <summary>
        /// Gets a new document with the specified source, content, and metadata (in addition to the default initial metadata).
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="content">The content.</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The new document.</returns>
        IDocument GetDocument(string source, string content, IEnumerable<KeyValuePair<string, object>> items = null);

        /// <summary>
        /// Gets a new document with the specified content and metadata (in addition to the default initial metadata).
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The new document.</returns>
        IDocument GetDocument(string content, IEnumerable<KeyValuePair<string, object>> items = null);

        /// <summary>
        /// Gets a new document with the specified source, content stream, and metadata (in addition to the default initial metadata).
        /// If <paramref name="disposeStream"/> is true (which it is by default), the provided 
        /// <see cref="Stream"/> will automatically be disposed when the document is disposed (I.e., the 
        /// document takes ownership of the <see cref="Stream"/>).
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="stream">The content stream.</param>
        /// <param name="items">The metadata items.</param>
        /// <param name="disposeStream">If set to <c>true</c> the provided <see cref="Stream"/> is disposed when the document is.</param>
        /// <returns>The new document.</returns>
        IDocument GetDocument(string source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true);

        /// <summary>
        /// Gets a new document with the specified content stream and metadata (in addition to the default initial metadata).
        /// If <paramref name="disposeStream"/> is true (which it is by default), the provided 
        /// <see cref="Stream"/> will automatically be disposed when the document is disposed (I.e., the 
        /// document takes ownership of the <see cref="Stream"/>).
        /// </summary>
        /// <param name="stream">The content stream.</param>
        /// <param name="items">The metadata items.</param>
        /// <param name="disposeStream">If set to <c>true</c> the provided <see cref="Stream"/> is disposed when the document is.</param>
        /// <returns>The new document.</returns>
        IDocument GetDocument(Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true);

        /// <summary>
        /// Gets a new document with the specified metadata (in addition to the default initial metadata).
        /// </summary>
        /// <param name="items">The metadata items.</param>
        /// <returns>The new document.</returns>
        IDocument GetDocument(IEnumerable<KeyValuePair<string, object>> items);
        
        /// <summary>
        /// Clones the specified source document with a new source, new content, and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <see cref="ModuleExtensions.AsNewDocuments{TModule}(TModule)"/> was called on the module.
        /// </summary>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="source">The source (if the source document contains a source, then this is ignored and the source document's source is used instead).</param>
        /// <param name="content">The content.</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The cloned or new document.</returns>
        IDocument GetDocument(IDocument sourceDocument, string source, string content, IEnumerable<KeyValuePair<string, object>> items = null);

        /// <summary>
        /// Clones the specified source document with new content and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <see cref="ModuleExtensions.AsNewDocuments{TModule}(TModule)"/> was called on the module.
        /// </summary>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="content">The content.</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The cloned or new document.</returns>
        IDocument GetDocument(IDocument sourceDocument, string content, IEnumerable<KeyValuePair<string, object>> items = null);

        /// <summary>
        /// Clones the specified source document with a new source, new content stream, and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <see cref="ModuleExtensions.AsNewDocuments{TModule}(TModule)"/> was called on the module.
        /// If <paramref name="disposeStream"/> is true (which it is by default), the provided 
        /// <see cref="Stream"/> will automatically be disposed when the document is disposed (I.e., the 
        /// document takes ownership of the <see cref="Stream"/>).
        /// </summary>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="source">The source (if the source document contains a source, then this is ignored and the source document's source is used instead).</param>
        /// <param name="stream">The content stream.</param>
        /// <param name="items">The metadata items.</param>
        /// <param name="disposeStream">If set to <c>true</c> the provided <see cref="Stream"/> is disposed when the document is.</param>
        /// <returns>The cloned or new document.</returns>
        IDocument GetDocument(IDocument sourceDocument, string source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true);

        /// <summary>
        /// Clones the specified source document with a new content stream, and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <see cref="ModuleExtensions.AsNewDocuments{TModule}(TModule)"/> was called on the module.
        /// If <paramref name="disposeStream"/> is true (which it is by default), the provided 
        /// <see cref="Stream"/> will automatically be disposed when the document is disposed (I.e., the 
        /// document takes ownership of the <see cref="Stream"/>).
        /// </summary>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="stream">The content stream.</param>
        /// <param name="items">The metadata items.</param>
        /// <param name="disposeStream">If set to <c>true</c> the provided <see cref="Stream"/> is disposed when the document is.</param>
        /// <returns>The cloned or new document.</returns>
        IDocument GetDocument(IDocument sourceDocument, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true);

        /// <summary>
        /// Clones the specified source document with identical content and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <see cref="ModuleExtensions.AsNewDocuments{TModule}(TModule)"/> was called on the module.
        /// </summary>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The cloned or new document.</returns>
        IDocument GetDocument(IDocument sourceDocument, IEnumerable<KeyValuePair<string, object>> items);

        // This provides access to the same enhanced type conversion used to convert metadata types
        bool TryConvert<T>(object value, out T result);

        // This executes the specified modules with the specified input documents and returns the result documents
        IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs);

        // This executes the specified modules with an empty initial input document with optional additional metadata and returns the result documents
        IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<KeyValuePair<string, object>> metadata = null);
        IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<MetadataItem> metadata);
    }
}
