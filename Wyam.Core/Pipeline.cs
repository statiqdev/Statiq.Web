using System;
using System.Collections;
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
        private readonly List<IModule> _modules = new List<IModule>();

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
            if (modules != null)
            {
                _modules.AddRange(modules);
            }
        }

        public string Name
        {
            get { return _name; }
        }

        public void Add(IModule item)
        {
            _modules.Add(item);
        }

        public void Add(params IModule[] items)
        {
            _modules.AddRange(items);
        }

        public void Clear()
        {
            _modules.Clear();
        }

        public bool Contains(IModule item)
        {
            return _modules.Contains(item);
        }

        public void CopyTo(IModule[] array, int arrayIndex)
        {
            _modules.CopyTo(array, arrayIndex);
        }

        public bool Remove(IModule item)
        {
            return _modules.Remove(item);
        }

        public int Count
        {
            get { return _modules.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public IEnumerator<IModule> GetEnumerator()
        {
            return _modules.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(IModule item)
        {
            return _modules.IndexOf(item);
        }

        public void Insert(int index, IModule item)
        {
            _modules.Insert(index, item);
        }

        public void Insert(int index, params IModule[] items)
        {
            _modules.InsertRange(index, items);
        }

        public void RemoveAt(int index)
        {
            _modules.RemoveAt(index);
        }

        public IModule this[int index]
        {
            get { return _modules[index]; }
            set { _modules[index] = value; }
        }

        public IReadOnlyList<IDocument> Execute(IEnumerable<IModule> modules, IEnumerable<IDocument> inputDocuments)
        {
            List<IDocument> documents = inputDocuments == null
                ? new List<IDocument> { new Document(new Metadata(_engine)) } : inputDocuments.ToList();
            ExecutionContext context = new ExecutionContext(_engine, this);
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
                    _engine.DocumentCollection.Set(Name, documents.AsReadOnly());
                    _engine.Trace.IndentLevel = indent;
                    _engine.Trace.Verbose("Executed module {0} resulting in {1} output document(s).", moduleName, documents.Count);
                }
                catch (Exception ex)
                {
                    _engine.Trace.Error("Error while executing module {0}: {1}", moduleName, ex.Message);
                    _engine.Trace.Verbose(ex.ToString());
                    _engine.Trace.IndentLevel = indent;
                    documents = new List<IDocument>();
                    _engine.DocumentCollection.Set(Name, documents.AsReadOnly());
                    break;
                }
            }
            return documents.AsReadOnly();
        }

        public void Execute()
        {
            Execute(_modules, null);
        }
    }
}
