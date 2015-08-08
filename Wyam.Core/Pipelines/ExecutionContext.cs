using System.Collections.Generic;
using System.Reflection;
using Wyam.Common;

namespace Wyam.Core.Pipelines
{
    internal class ExecutionContext : IExecutionContext
    {
        private readonly Engine _engine;
        private readonly Pipeline _pipeline;

        public byte[] RawConfigAssembly => _engine.RawConfigAssembly;
        public IEnumerable<Assembly> Assemblies => _engine.Assemblies;
        public IReadOnlyPipeline Pipeline => new ReadOnlyPipeline(_pipeline);
        public IModule Module { get; internal set; }
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

        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs)
        {
            // Store the document list before executing the child modules and restore it afterwards
            IReadOnlyList<IDocument> documents = _engine.DocumentCollection.Get(_pipeline.Name);
            IReadOnlyList<IDocument> results = _pipeline.Execute(modules, inputs);
            _engine.DocumentCollection.Set(_pipeline.Name, documents);
            return results;
        }
    }
}
