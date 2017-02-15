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
    public abstract class ContainerModule : IModule, IModuleList
    {
        private readonly IModuleList _modules;

        protected ContainerModule()
        {
            _modules = new ModuleList();
        }

        protected ContainerModule(params IModule[] modules)
        {
            _modules = new ModuleList(modules);
        }

        protected ContainerModule(ModuleList modules)
        {
            if (modules == null)
            {
                throw new ArgumentNullException(nameof(modules));
            }
            _modules = modules;
        }

        public abstract IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<IModule> GetEnumerator() => _modules.GetEnumerator();

        public void Add(IModule item) => _modules.Add(item);

        public void Clear() => _modules.Clear();

        public bool Contains(IModule item) => _modules.Contains(item);

        public void CopyTo(IModule[] array, int arrayIndex) => _modules.CopyTo(array, arrayIndex);

        public bool Remove(IModule item) => _modules.Remove(item);

        public bool Remove(string name) => _modules.Remove(name);

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

        public bool Contains(string name) => _modules.Contains(name);

        public bool TryGetValue(string name, out IModule value) => _modules.TryGetValue(name, out value);

        public IModule this[string name] => _modules[name];

        public void Add(string name, IModule module) => _modules.Add(name, module);

        public void Insert(int index, string name, IModule module) => _modules.Insert(index, name, module);

        public IEnumerable<KeyValuePair<string, IModule>> AsKeyValuePairs() => _modules.AsKeyValuePairs();
    }
}
