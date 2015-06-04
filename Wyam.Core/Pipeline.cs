using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Abstractions;

namespace Wyam.Core
{
    internal class Pipeline : IPipeline
    {
        private readonly string _name;
        private readonly Engine _engine;
        private readonly IModule[] _modules;

        public Pipeline(string name, Engine engine, IModule[] modules)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("name");
            }
            if (engine == null)
            {
                throw new ArgumentNullException("engine");
            }
            _name = name;
            _engine = engine;
            _modules = modules;
        }

        public string Name
        {
            get { return _name; }
        }

        public int Count
        {
            get { return _modules.Length; }
        }

        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputDocuments, Action<IReadOnlyList<IDocument>> setDocuments)
        {
            List<IDocument> documents = inputDocuments == null 
                ? new List<IDocument> { new Document(new Metadata(_engine)) } : inputDocuments.ToList();
            ExecutionContext context = new ExecutionContext(_engine, this);
            if (modules != null)
            {
                foreach (IModule module in modules.Where(x => x != null))
                {
                    string moduleName = module.GetType().Name;
                    _engine.Trace.Verbose("Executing module {0} with {1} input document(s)...", moduleName, documents.Count);
                    int indent = _engine.Trace.Indent();
                    try
                    {
                        // Make sure we clone the output context if it's the same as the input
                        IEnumerable<IDocument> outputs = module.Execute(documents, context);
                        documents = outputs == null ? new List<IDocument>() : outputs.Where(x => x != null).ToList();
                        if (setDocuments != null)
                        {
                            setDocuments(documents.AsReadOnly());
                        }
                        _engine.Trace.IndentLevel = indent;
                        _engine.Trace.Verbose("Executed module {0} resulting in {1} output document(s).", moduleName, documents.Count);
                    }
                    catch (Exception ex)
                    {
                        _engine.Trace.Error("Error while executing module {0}: {1}", moduleName, ex.Message);
                        _engine.Trace.Verbose(ex.ToString());
                        _engine.Trace.IndentLevel = indent;
                        documents = new List<IDocument>();
                        if (setDocuments != null)
                        {
                            setDocuments(documents.AsReadOnly());
                        }
                        break;
                    }
                }
            }
            return documents.AsReadOnly();
        }

        public void Execute(Action<IReadOnlyList<IDocument>> setDocuments)
        {
            Execute(_modules, null, setDocuments);
        }
    }
}
