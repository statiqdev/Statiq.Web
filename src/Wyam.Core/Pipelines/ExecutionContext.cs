using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Wyam.Common;
using Wyam.Common.Caching;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Common.Tracing;
using Wyam.Core.Documents;

namespace Wyam.Core.Pipelines
{
    internal class ExecutionContext : IExecutionContext
    {
        private readonly Pipeline _pipeline;

        public Engine Engine { get; }

        public byte[] RawConfigAssembly => Engine.RawConfigAssembly;

        public IEnumerable<Assembly> Assemblies => Engine.Assemblies;

        public IEnumerable<string> Namespaces => Engine.Namespaces;

        public IReadOnlyPipeline Pipeline => new ReadOnlyPipeline(_pipeline);

        public IModule Module { get; }

        public IDocumentCollection Documents => Engine.Documents;

        [Obsolete]
        public string RootFolder => Engine.RootFolder;

        [Obsolete]
        public string InputFolder => Engine.InputFolder;

        [Obsolete]
        public string OutputFolder => Engine.OutputFolder;

        public IFileSystem FileSystem => Engine.FileSystem;

        public IExecutionCache ExecutionCache => Engine.ExecutionCacheManager.Get(Module);

        public ExecutionContext(Engine engine, Pipeline pipeline)
        {
            Engine = engine;
            _pipeline = pipeline;
        }

        private ExecutionContext(ExecutionContext original, IModule module)
        {
            Engine = original.Engine;
            _pipeline = original._pipeline;
            Module = module;
        }

        internal ExecutionContext Clone(IModule module)
        {
            return new ExecutionContext(this, module);
        }

        public bool TryConvert<T>(object value, out T result)
        {
            return TypeHelper.TryConvert(value, out result);
        }

        public IDocument GetNewDocument(string source, string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return new Document(Engine.InitialMetadata, _pipeline, source, null, content, items, true);
        }

        public IDocument GetNewDocument(string source, string content, IEnumerable<MetadataItem> items)
        {
            return GetNewDocument(source, content, items?.Select(x => x.Pair));
        }

        public IDocument GetNewDocument(string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return new Document(Engine.InitialMetadata, _pipeline, string.Empty, null, content, items, true);
        }

        public IDocument GetNewDocument(string content, IEnumerable<MetadataItem> items)
        {
            return GetNewDocument(content, items?.Select(x => x.Pair));
        }

        public IDocument GetNewDocument(string source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
        {
            return new Document(Engine.InitialMetadata, _pipeline, source, stream, null, items, disposeStream);
        }

        public IDocument GetNewDocument(string source, Stream stream, IEnumerable<MetadataItem> items, bool disposeStream = true)
        {
            return GetNewDocument(source, stream, items?.Select(x => x.Pair), disposeStream);
        }

        public IDocument GetNewDocument(Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
        {
            return new Document(Engine.InitialMetadata, _pipeline, string.Empty, stream, null, items, disposeStream);
        }

        public IDocument GetNewDocument(Stream stream, IEnumerable<MetadataItem> items, bool disposeStream = true)
        {
            return GetNewDocument(stream, items?.Select(x => x.Pair), disposeStream);
        }

        public IDocument GetNewDocument(IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return new Document(Engine.InitialMetadata, _pipeline, string.Empty, null, null, items, true);
        }

        public IDocument GetNewDocument(IEnumerable<MetadataItem> items)
        {
            return GetNewDocument(items?.Select(x => x.Pair));
        }

        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs)
        {
            return Execute(modules, inputs, null);
        }

        // Executes the module with an empty document containing the specified metadata items
        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return Execute(modules, null, items);
        }

        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<MetadataItem> items)
        {
            return Execute(modules, items?.Select(x => x.Pair));
        }

        private IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs, IEnumerable<KeyValuePair<string, object>> items)
        {
            if (modules == null)
            {
                return ImmutableArray<IDocument>.Empty;
            }

            // Store the document list before executing the child modules and restore it afterwards
            IReadOnlyList<IDocument> originalDocuments = Engine.DocumentCollection.Get(_pipeline.Name);
            ImmutableArray<IDocument> documents = inputs?.ToImmutableArray() 
                ?? new [] { GetNewDocument(items) }.ToImmutableArray();
            IReadOnlyList<IDocument> results = _pipeline.Execute(this, modules, documents);
            Engine.DocumentCollection.Set(_pipeline.Name, originalDocuments);
            return results;
        }
    }
}
