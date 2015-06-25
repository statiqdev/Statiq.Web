using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wyam.Abstractions;

namespace Wyam.Core
{
    internal class ExecutionContext : IExecutionContext
    {
        private readonly Engine _engine;
        private readonly Pipeline _pipeline;

        public ExecutionContext(Engine engine, Pipeline pipeline)
        {
            _engine = engine;
            _pipeline = pipeline;
        }

        public byte[] RawConfigAssembly
        {
            get {  return _engine.RawConfigAssembly; }
        }

        public IEnumerable<Assembly> Assemblies
        {
            get { return _engine.Assemblies; }
        }

        public IReadOnlyPipeline Pipeline
        {
            get { return new ReadOnlyPipeline(_pipeline); }
        }

        public ITrace Trace
        {
            get { return _engine.Trace; }
        }

        public IDocumentCollection Documents
        {
            get { return _engine.Documents; }
        }

        public string RootFolder
        {
            get { return _engine.RootFolder; }
        }

        public string InputFolder
        {
            get { return _engine.InputFolder; }
        }

        public string OutputFolder
        {
            get { return _engine.OutputFolder; }
        }

        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputDocuments)
        {
            // Store the document list before executing the child modules and restore it afterwards
            IReadOnlyList<IDocument> documents = _engine.DocumentCollection.Get(_pipeline.Name);
            IReadOnlyList<IDocument> results = _pipeline.Execute(modules, inputDocuments);
            _engine.DocumentCollection.Set(_pipeline.Name, documents);
            return results;
        }
    }
}
