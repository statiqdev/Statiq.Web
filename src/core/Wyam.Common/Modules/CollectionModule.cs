using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;

namespace Wyam.Common.Modules
{
    /// <summary>
    /// A base class for modules that contain a collection of child modules.
    /// </summary>
    public abstract class CollectionModule : IModule, IModuleCollection
    {
        private readonly IModuleCollection _modules;

        protected CollectionModule()
        {
            _modules = new ModuleCollection();
        }

        protected CollectionModule(params IModule[] modules)
        {
            _modules = new ModuleCollection(modules);
        }

        public abstract IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context);

        IEnumerator<KeyValuePair<string, IModule>> IEnumerable<KeyValuePair<string, IModule>>.GetEnumerator() => 
            ((IEnumerable<KeyValuePair<string, IModule>>)_modules).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _modules).GetEnumerator();

        public IEnumerator<IModule> GetEnumerator() => _modules.GetEnumerator();

        public void Add(IModule item) => _modules.Add(item);

        public void Clear() => _modules.Clear();

        public bool Contains(IModule item) => _modules.Contains(item);

        public void CopyTo(IModule[] array, int arrayIndex) => _modules.CopyTo(array, arrayIndex);

        public bool Remove(IModule item) => _modules.Remove(item);

        public int Count => _modules.Count;

        public void Add(params IModule[] modules) => _modules.Add(modules);

        public void Insert(int index, params IModule[] modules) => _modules.Insert(index, modules);

        public int IndexOf(string name) => _modules.IndexOf(name);

        public bool IsReadOnly => _modules.IsReadOnly;

        public int IndexOf(IModule item) => _modules.IndexOf(item);

        public void Insert(int index, IModule item) => _modules.Insert(index, item);

        public void RemoveAt(int index) => _modules.RemoveAt(index);

        public IModule this[int index]
        {
            get { return _modules[index]; }
            set { _modules[index] = value; }
        }

        public bool ContainsKey(string key) => _modules.ContainsKey(key);

        public bool TryGetValue(string key, out IModule value) => _modules.TryGetValue(key, out value);

        public IModule this[string key] => _modules[key];

        public IEnumerable<string> Keys => _modules.Keys;

        public IEnumerable<IModule> Values => _modules.Values;

        public void Add(string name, IModule module) => _modules.Add(name, module);

        public void Insert(int index, string name, IModule module) => _modules.Insert(index, name, module);
    }
}
