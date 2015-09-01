using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Wyam.Common;
using Wyam.Core.Documents;

namespace Wyam.Core.Pipelines
{
    internal class ExecutionContext : IExecutionContext
    {
        private readonly Engine _engine;
        private readonly Pipeline _pipeline;
        private readonly Metadata _metadata;
        private readonly IModule _module;

        public byte[] RawConfigAssembly => _engine.RawConfigAssembly;
        public IEnumerable<Assembly> Assemblies => _engine.Assemblies;
        public IReadOnlyPipeline Pipeline => new ReadOnlyPipeline(_pipeline);
        public IModule Module => _module;
        public ITrace Trace => _engine.Trace;
        public IDocumentCollection Documents => _engine.Documents;
        public string RootFolder => _engine.RootFolder;
        public string InputFolder => _engine.InputFolder;
        public string OutputFolder => _engine.OutputFolder;
        public IExecutionCache ExecutionCache => _engine.ExecutionCacheManager.Get(Module, _engine);
        public IMetadata Metadata => _metadata;

        public ExecutionContext(Engine engine, Pipeline pipeline)
        {
            _engine = engine;
            _pipeline = pipeline;
            _metadata = new Metadata(engine);
        }

        private ExecutionContext(ExecutionContext original, IModule module)
        {
            _engine = original._engine;
            _pipeline = original._pipeline;
            _metadata = original._metadata;
            _module = module;
        }

        private ExecutionContext(ExecutionContext original, IEnumerable<KeyValuePair<string, object>> metadata)
        {
            _engine = original._engine;
            _pipeline = original._pipeline;
            _metadata = original._metadata.Clone(metadata);
            _module = original.Module;
        }

        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs, IEnumerable<KeyValuePair<string, object>> metadata = null)
        {
            // Store the document list before executing the child modules and restore it afterwards
            IReadOnlyList<IDocument> documents = _engine.DocumentCollection.Get(_pipeline.Name);
            ExecutionContext context = metadata == null ? this : new ExecutionContext(this, metadata);
            IReadOnlyList<IDocument> results = _pipeline.Execute(context, modules, inputs);
            _engine.DocumentCollection.Set(_pipeline.Name, documents);
            return results;
        }

        public IExecutionContext Clone(IEnumerable<KeyValuePair<string, object>> metadata)
        {
            return new ExecutionContext(this, metadata);
        }

        internal ExecutionContext Clone(IModule module)
        {
            return new ExecutionContext(this, module);
        }

        // IMetadata

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _metadata.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ContainsKey(string key)
        {
            return _metadata.ContainsKey(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _metadata.TryGetValue(key, out value);
        }

        public object this[string key] => _metadata[key];

        public IEnumerable<string> Keys => _metadata.Keys;

        public IEnumerable<object> Values => _metadata.Values;

        public IMetadata<T> MetadataAs<T>()
        {
            return _metadata.MetadataAs<T>();
        }

        public object Get(string key, object defaultValue)
        {
            return _metadata.Get(key, defaultValue);
        }

        public T Get<T>(string key)
        {
            return _metadata.Get<T>(key);
        }

        public T Get<T>(string key, T defaultValue)
        {
            return _metadata.Get<T>(key, defaultValue);
        }

        public string String(string key, string defaultValue = null)
        {
            return _metadata.String(key, defaultValue);
        }

        public string Link(string key, string defaultValue = null)
        {
            return _metadata.Link(key, defaultValue);
        }

        public int Count => _metadata.Count;
    }
}
