using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Wyam.Common.Caching;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.JavaScript;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;

namespace Wyam.Common.Execution
{
    /// <summary>
    /// All of the information that represents a given build. Also implements
    /// <see cref="IMetadata"/> to expose the global metadata.
    /// </summary>
    public interface IExecutionContext : IMetadata
    {
        /// <summary>
        /// Gets the raw bytes for dynamically compiled assemblies (such as the configuration script).
        /// </summary>
        IReadOnlyCollection<byte[]> DynamicAssemblies { get; }

        /// <summary>
        /// Gets a set of namespaces that should be brought into scope for modules that perform dynamic compilation.
        /// </summary>
        IReadOnlyCollection<string> Namespaces { get; }

        /// <summary>
        /// Gets the currently executing pipeline.
        /// </summary>
        IReadOnlyPipeline Pipeline { get; }

        /// <summary>
        /// Gets the currently executing module.
        /// </summary>
        IModule Module { get; }

        /// <summary>
        /// Gets the current execution cache. Modules can use the cache to store data between executions.
        /// </summary>
        IExecutionCache ExecutionCache { get; }

        /// <summary>
        /// Gets the current file system.
        /// </summary>
        IReadOnlyFileSystem FileSystem { get; }

        /// <summary>
        /// Gets the current settings metadata.
        /// </summary>
        IReadOnlySettings Settings { get; }

        /// <summary>
        /// Gets the collection of all previously processed documents.
        /// </summary>
        IDocumentCollection Documents { get; }

        [Obsolete]
        IMetadata GlobalMetadata { get; }

        /// <summary>
        /// Gets any input that was passed to the application (for example, on stdin via piping).
        /// </summary>
        /// <value>
        /// The application input.
        /// </value>
        string ApplicationInput { get; }

        /// <summary>
        /// Gets a <see cref="Stream"/> that can be used for document content. If <paramref name="content"/>
        /// is not null, the stream is initialized with the specified content. It is prefered to use
        /// this method to obtain a stream over creating your own if the source of the content does
        /// not already provide one. The returned streams are optimized for memory usage and performance.
        /// <remarks>The position is set to the beginning of the stream when returned.</remarks>
        /// </summary>
        /// <param name="content">Content to initialize the stream with.</param>
        /// <returns>A stream for document content.</returns>
        Stream GetContentStream(string content = null);

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
        [Obsolete("Document content strings are deprecated, please use " + nameof(IExecutionContext) + "." + nameof(IExecutionContext.GetContentStream) + " to get a stream and use the stream instead")]
        IDocument GetDocument(FilePath source, string content, IEnumerable<KeyValuePair<string, object>> items = null);

        /// <summary>
        /// Gets a new document with the specified content and metadata (in addition to the default initial metadata).
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The new document.</returns>
        [Obsolete("Document content strings are deprecated, please use " + nameof(IExecutionContext) + "." + nameof(IExecutionContext.GetContentStream) + " to get a stream and use the stream instead")]
        IDocument GetDocument(string content, IEnumerable<KeyValuePair<string, object>> items = null);

        /// <summary>
        /// Clones the specified source document with a new source, new content, and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <c>AsNewDocuments()</c> was called on the module.
        /// </summary>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="source">The source (if the source document contains a source, then this is ignored and the source document's source is used instead).</param>
        /// <param name="content">The content.</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The cloned or new document.</returns>
        [Obsolete("Document content strings are deprecated, please use " + nameof(IExecutionContext) + "." + nameof(IExecutionContext.GetContentStream) + " to get a stream and use the stream instead")]
        IDocument GetDocument(IDocument sourceDocument, FilePath source, string content, IEnumerable<KeyValuePair<string, object>> items = null);

        /// <summary>
        /// Clones the specified source document with new content and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <c>AsNewDocuments()</c> was called on the module.
        /// </summary>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="content">The content.</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The cloned or new document.</returns>
        [Obsolete("Document content strings are deprecated, please use " + nameof(IExecutionContext) + "." + nameof(IExecutionContext.GetContentStream) + " to get a stream and use the stream instead")]
        IDocument GetDocument(IDocument sourceDocument, string content, IEnumerable<KeyValuePair<string, object>> items = null);

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
        IDocument GetDocument(FilePath source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true);

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
        /// Clones the specified source document with a new source, new content stream, and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <c>AsNewDocuments()</c> was called on the module.
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
        IDocument GetDocument(IDocument sourceDocument, FilePath source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true);

        /// <summary>
        /// Clones the specified source document with a new content stream, and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <c>AsNewDocuments()</c> was called on the module.
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
        /// Clones the specified source document with a new source and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <c>AsNewDocuments()</c> was called on the module.
        /// </summary>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="source">The source (if the source document contains a source, then this is ignored and the source document's source is used instead).</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The cloned or new document.</returns>
        IDocument GetDocument(IDocument sourceDocument, FilePath source, IEnumerable<KeyValuePair<string, object>> items = null);

        /// <summary>
        /// Clones the specified source document with identical content and additional metadata (all existing metadata is retained)
        /// or gets a new document if the source document is null or <c>AsNewDocuments()</c> was called on the module.
        /// </summary>
        /// <param name="sourceDocument">The source document.</param>
        /// <param name="items">The metadata items.</param>
        /// <returns>The cloned or new document.</returns>
        IDocument GetDocument(IDocument sourceDocument, IEnumerable<KeyValuePair<string, object>> items);

        /// <summary>
        /// Provides access to the same enhanced type conversion used to convert metadata types.
        /// </summary>
        /// <typeparam name="T">The destination type.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <param name="result">The result of the conversion.</param>
        /// <returns><c>true</c> if the conversion could be completed, <c>false</c> otherwise.</returns>
        bool TryConvert<T>(object value, out T result);

        /// <summary>
        /// Executes the specified modules with the specified input documents and returns the result documents.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        /// <param name="inputs">The input documents.</param>
        /// <returns>The result documents from the executed modules.</returns>
        IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs);

        /// <summary>
        /// Executes the specified modules with an empty initial input document with optional additional metadata and returns the result documents.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        /// <param name="metadata">The metadata to use.</param>
        /// <returns>The result documents from the executed modules.</returns>
        IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<KeyValuePair<string, object>> metadata = null);

        /// <summary>
        /// Executes the specified modules with an empty initial input document with optional additional metadata and returns the result documents.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        /// <param name="metadata">The metadata to use.</param>
        /// <returns>The result documents from the executed modules.</returns>
        IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<MetadataItem> metadata);

        /// <summary>
        /// Gets a new <see cref="IJsEnginePool"/>. The returned engine pool should be disposed
        /// when no longer needed.
        /// </summary>
        /// <param name="initializer">
        /// The code to run when a new engine is created. This should configure
        /// the environment and set up any required JavaScript libraries.
        /// </param>
        /// <param name="startEngines">The number of engines to initially start when a pool is created.</param>
        /// <param name="maxEngines">The maximum number of engines that will be created in the pool.</param>
        /// <param name="maxUsagesPerEngine">The maximum number of times an engine can be reused before it is disposed.</param>
        /// <param name="engineTimeout">
        /// The default timeout to use when acquiring an engine from the pool (defaults to 5 seconds).
        /// If an engine can not be acquired in this timeframe, an exception will be thrown.
        /// </param>
        /// <returns>A new JavaScript engine pool.</returns>
        IJsEnginePool GetJsEnginePool(
            Action<IJsEngine> initializer = null,
            int startEngines = 10,
            int maxEngines = 25,
            int maxUsagesPerEngine = 100,
            TimeSpan? engineTimeout = null);
    }
}
