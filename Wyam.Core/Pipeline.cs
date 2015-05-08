using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Extensibility;

namespace Wyam.Core
{
    internal class Pipeline : IPipeline, IPipelineContext
    {
        private readonly Engine _engine;
        private readonly IModule[] _modules;
        private readonly IReadOnlyList<IDocument> _completedDocuments;

        public Pipeline(Engine engine, IModule[] modules, IReadOnlyList<IDocument> completedDocuments)
        {
            _engine = engine;
            _modules = modules;
            _completedDocuments = completedDocuments;
        }

        public int Count
        {
            get { return _modules.Length; }
        }

        public ITrace Trace
        {
            get { return _engine.Trace; }
        }

        public IReadOnlyList<IDocument> CompletedDocuments
        {
            get { return _completedDocuments; }
        }

        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputDocuments)
        {
            List<IDocument> documents = inputDocuments == null 
                ? new List<IDocument> { new Document(new Metadata(_engine)) } : inputDocuments.ToList();
            if (modules != null)
            {
                foreach (IModule module in modules.Where(x => x != null))
                {
                    string moduleName = module.GetType().Name;
                    Trace.Information("Executing module {0} with {1} input(s)...", moduleName, documents.Count);
                    int indent = Trace.Indent();
                    try
                    {
                        // Make sure we clone the output context if it's the same as the input
                        IEnumerable<IDocument> outputs = module.Execute(documents, this);
                        documents = outputs == null ? new List<IDocument>() : outputs.Where(x => x != null).ToList();
                        Trace.IndentLevel = indent;
                        Trace.Information("Executed module {0} resulting in {1} output(s).", moduleName, documents.Count);
                    }
                    catch (Exception ex)
                    {
                        Trace.Error("Error while executing module {0}: {1}", moduleName, ex.Message);
                        Trace.Verbose(ex.ToString());
                        Trace.IndentLevel = indent;
                        documents = new List<IDocument>();
                        break;
                    }
                }
            }
            return documents.AsReadOnly();
        }

        public IReadOnlyList<IDocument> Execute()
        {
            return Execute(_modules, null);
        }
    }
}
