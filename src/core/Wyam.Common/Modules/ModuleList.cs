using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Modules
{
    public class ModuleList : IModuleList
    {
        private readonly List<KeyValuePair<string, IModule>> _modules = new List<KeyValuePair<string, IModule>>();

        public ModuleList()
        {
        }

        public ModuleList(IEnumerable<IModule> modules)
        {
            if (modules == null)
            {
                throw new ArgumentNullException(nameof(modules));
            }
            _modules.AddRange(modules.Select(x => GetNamedPair(x, false)));
        }
        
        IEnumerator IEnumerable.GetEnumerator() => _modules.Select(x => x.Value).GetEnumerator();

        IEnumerator<KeyValuePair<string, IModule>> IEnumerable<KeyValuePair<string, IModule>>.GetEnumerator() => _modules.GetEnumerator();

        public IEnumerator<IModule> GetEnumerator() => _modules.Select(x => x.Value).GetEnumerator();

        public int Count => _modules.Count;

        public void Add(IModule item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            _modules.Add(GetNamedPair(item));
        }

        public void Clear() => _modules.Clear();

        public bool Contains(IModule item) => _modules.Any(x => x.Value.Equals(item));

        public void CopyTo(IModule[] array, int arrayIndex) => _modules.Select(x => x.Value).ToList().CopyTo(array);

        public bool Remove(IModule item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                _modules.RemoveAt(index);
                return true;
            }
            return false;
        }

        public bool IsReadOnly => false;

        public int IndexOf(IModule item) => _modules.FindIndex(x => x.Value.Equals(item));

        public void Insert(int index, IModule item) => _modules.Insert(index, GetNamedPair(item));

        public void RemoveAt(int index) => _modules.RemoveAt(index);

        public IModule this[int index]
        {
            get { return _modules[index].Value; }
            set
            {
                _modules[index] = default(KeyValuePair<string, IModule>); // Zero it out before setting so we don't throw for duplicate keys
                _modules[index] = GetNamedPair(value);
            }
        }
        
        public bool ContainsKey(string key) => _modules.Any(x => x.Key.Equals(key));

        public bool TryGetValue(string key, out IModule value)
        {
            foreach (KeyValuePair<string, IModule> item in _modules)
            {
                if (item.Key.Equals(key))
                {
                    value = item.Value;
                    return true;
                }
            }
            value = null;
            return false;
        }

        public IModule this[string key]
        {
            get
            {
                IModule module;
                if (TryGetValue(key, out module))
                {
                    return module;
                }
                throw new KeyNotFoundException($"The key \"{key}\" was not present in the dictionary.");
            }
        }

        public IEnumerable<string> Keys => _modules.Select(x => x.Key);

        public IEnumerable<IModule> Values => _modules.Select(x => x.Value);

        public void Add(params IModule[] modules)
        {
            foreach (IModule module in modules)
            {
                Add(module);
            }
        }

        public void Insert(int index, params IModule[] modules)
        {
            for (int i = index; i < index + modules.Length; i++)
            {
                Insert(i, modules[i - index]);
            }
        }

        public void Add(string name, IModule module) => Add(new NamedModule(name, module));

        public void Insert(int index, string name, IModule module) => Insert(index, new NamedModule(name, module));

        public int IndexOf(string name) => _modules.FindIndex(x => x.Key.Equals(name));

        private KeyValuePair<string, IModule> GetNamedPair(IModule module, bool checkNames = true)
        {
            NamedModule namedModule = module as NamedModule;
            if (namedModule != null)
            {
                if (checkNames && _modules.Any(x => x.Key != null && x.Key.Equals(namedModule.Name)))
                {
                    throw new ArgumentException($"A module with the name {namedModule.Name} already exists in the collection");
                }
                return new KeyValuePair<string, IModule>(namedModule.Name, namedModule.Module);
            }
            return new KeyValuePair<string, IModule>(null, module);
        }
    }
}
