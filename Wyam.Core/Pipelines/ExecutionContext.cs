using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Wyam.Common;
using Wyam.Core.Documents;

namespace Wyam.Core.Pipelines
{
    internal class ExecutionContext : IExecutionContext
    {
        private readonly Engine _engine;
        private readonly Pipeline _pipeline;

        public byte[] RawConfigAssembly => _engine.RawConfigAssembly;
        public IEnumerable<Assembly> Assemblies => _engine.Assemblies;
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

        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs)
        {
            return Execute(modules, inputs, null);
        }

        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<KeyValuePair<string, object>> metadata = null)
        {
            return Execute(modules, null, metadata);
        }

        private IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs, IEnumerable<KeyValuePair<string, object>> items)
        {
            // Store the document list before executing the child modules and restore it afterwards
            IReadOnlyList<IDocument> originalDocuments = _engine.DocumentCollection.Get(_pipeline.Name);
            Metadata metadata = new Metadata(_engine);
            if (items != null)
            {
                metadata = metadata.Clone(items);
            }
            List<IDocument> documents = inputs?.ToList() ?? new List<IDocument> { new Document(metadata, _pipeline) };
            IReadOnlyList<IDocument> results = _pipeline.Execute(this, modules, documents);
            _engine.DocumentCollection.Set(_pipeline.Name, originalDocuments);
            return results;
        }
    }
}
