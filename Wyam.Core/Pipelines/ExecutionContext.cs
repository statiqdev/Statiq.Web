using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Wyam.Common;
using Wyam.Common.Caching;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Common.Tracing;
using Wyam.Core.Documents;

namespace Wyam.Core.Pipelines
{
    internal class ExecutionContext : IExecutionContext
    {
        private readonly Engine _engine;
        private readonly Pipeline _pipeline;

        public byte[] RawConfigAssembly => _engine.RawConfigAssembly;
        public IEnumerable<Assembly> Assemblies => _engine.Assemblies;
        public IEnumerable<string> Namespaces => _engine.Namespaces;
        public IReadOnlyPipeline Pipeline => new ReadOnlyPipeline(_pipeline);
        public IModule Module { get; }
        public ITrace Trace => _engine.Trace;
        public IDocumentCollection Documents => _engine.Documents;
        public string RootFolder => _engine.RootFolder;
        public string InputFolder => _engine.InputFolder;
        public string OutputFolder => _engine.OutputFolder;
        public IExecutionCache ExecutionCache => _engine.ExecutionCacheManager.Get(Module, _engine);

        public ExecutionContext(Engine engine, Pipeline pipeline)
        {
            _engine = engine;
            _pipeline = pipeline;
        }

        private ExecutionContext(ExecutionContext original, IModule module)
        {
            _engine = original._engine;
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
            return new Document(_engine, _pipeline, source, null, content, items, true);
        }

        public IDocument GetNewDocument(string content, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return new Document(_engine, _pipeline, string.Empty, null, content, items, true);
        }

        public IDocument GetNewDocument(string source, Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
        {
            return new Document(_engine, _pipeline, source, stream, null, items, disposeStream);
        }

        public IDocument GetNewDocument(Stream stream, IEnumerable<KeyValuePair<string, object>> items = null, bool disposeStream = true)
        {
            return new Document(_engine, _pipeline, string.Empty, stream, null, items, disposeStream);
        }

        public IDocument GetNewDocument(IEnumerable<KeyValuePair<string, object>> items = null)
        {
            return new Document(_engine, _pipeline, string.Empty, null, null, items, true);
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

        private IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs, IEnumerable<KeyValuePair<string, object>> items)
        {
            if (modules == null)
            {
                return ImmutableArray<IDocument>.Empty;
            }

            // Store the document list before executing the child modules and restore it afterwards
            IReadOnlyList<IDocument> originalDocuments = _engine.DocumentCollection.Get(_pipeline.Name);
            ImmutableArray<IDocument> documents = inputs?.ToImmutableArray() 
                ?? new [] { GetNewDocument(items) }.ToImmutableArray();
            IReadOnlyList<IDocument> results = _pipeline.Execute(this, modules, documents);
            _engine.DocumentCollection.Set(_pipeline.Name, originalDocuments);
            return results;
        }
    }
}
