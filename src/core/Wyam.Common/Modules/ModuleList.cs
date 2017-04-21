using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Modules
{
    /// <summary>
    /// A collection of optionally named modules.
    /// </summary>
    public class ModuleList : IModuleList
    {
        private readonly List<KeyValuePair<string, IModule>> _modules = new List<KeyValuePair<string, IModule>>();

        /// <summary>
        /// Creates a new empty module list.
        /// </summary>
        public ModuleList()
        {
        }

        /// <summary>
        /// Creates a new module list with an initial set of modules.
        /// Any <c>null</c> items in the sequence of modules will be discarded.
        /// </summary>
        /// <param name="modules">The initial modules in the list.</param>
        public ModuleList(params IModule[] modules)
            : this((IEnumerable<IModule>)modules)
        {
        }

        /// <summary>
        /// Creates a new module list with an initial set of modules.
        /// Any <c>null</c> items in the sequence of modules will be discarded.
        /// </summary>
        /// <param name="modules">The initial modules in the list.</param>
        public ModuleList(IEnumerable<IModule> modules)
        {
            if (modules != null)
            {
                foreach (IModule module in modules.Where(x => x != null))
                {
                    Add(module);
                }
            }
        }

        /// <inheritdoc />
        public void Add(params IModule[] modules)
        {
            foreach (IModule module in modules.Where(x => x != null))
            {
                Add(module);
            }
        }

        /// <inheritdoc />
        public void Add(IModule item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            Add(ExpandNamedModule(item));
        }

        /// <inheritdoc />
        public void Add(string name, IModule module) =>
            Add(new KeyValuePair<string, IModule>(name, module));

        private void Add(KeyValuePair<string, IModule> namedModule)
        {
            if (namedModule.Value == null)
            {
                throw new ArgumentException("Module cannot be null");
            }
            CheckName(namedModule.Key);
            _modules.Add(namedModule);
        }

        /// <inheritdoc />
        public void Insert(int index, params IModule[] modules)
        {
            modules = modules.Where(x => x != null).ToArray();
            for (int i = index; i < index + modules.Length; i++)
            {
                Insert(i, modules[i - index]);
            }
        }

        /// <inheritdoc />
        public void Insert(int index, IModule item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            Insert(index, ExpandNamedModule(item));
        }

        /// <inheritdoc />
        public void Insert(int index, string name, IModule module) =>
            Insert(index, new KeyValuePair<string, IModule>(name, module));

        private void Insert(int index, KeyValuePair<string, IModule> namedModule)
        {
            if (namedModule.Value == null)
            {
                throw new ArgumentException("Module cannot be null");
            }
            CheckName(namedModule.Key);
            _modules.Insert(index, namedModule);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool Remove(string name)
        {
            int index = IndexOf(name);
            if (index >= 0)
            {
                _modules.RemoveAt(index);
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public void RemoveAt(int index) => _modules.RemoveAt(index);

        /// <inheritdoc />
        public IModule this[int index]
        {
            get
            {
                return _modules[index].Value;
            }
            set
            {
                // Zero it out before setting so we don't throw for duplicate keys
                _modules[index] = default(KeyValuePair<string, IModule>);
                KeyValuePair<string, IModule> namedModule = ExpandNamedModule(value);
                CheckName(namedModule.Key);
                _modules[index] = namedModule;
            }
        }

        /// <inheritdoc />
        public IModule this[string name]
        {
            get
            {
                IModule module;
                if (TryGetValue(name, out module))
                {
                    return module;
                }
                throw new KeyNotFoundException($"The key \"{name}\" was not present in the dictionary.");
            }
        }

        /// <inheritdoc />
        public int Count => _modules.Count;

        /// <inheritdoc />
        public void Clear() => _modules.Clear();

        /// <inheritdoc />
        public bool Contains(IModule item) => _modules.Any(x => x.Value.Equals(item));

        /// <inheritdoc />
        public bool Contains(string name) => _modules.Any(x => string.Equals(x.Key, name, StringComparison.OrdinalIgnoreCase));

        /// <inheritdoc />
        public void CopyTo(IModule[] array, int arrayIndex) => _modules.Select(x => x.Value).ToList().CopyTo(array);

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public int IndexOf(IModule item) => _modules.FindIndex(x => x.Value.Equals(item));

        /// <inheritdoc />
        public bool TryGetValue(string name, out IModule value)
        {
            foreach (KeyValuePair<string, IModule> item in _modules)
            {
                if (string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = item.Value;
                    return true;
                }
            }
            value = null;
            return false;
        }

        /// <inheritdoc />
        public int IndexOf(string name) => _modules.FindIndex(x => string.Equals(x.Key, name, StringComparison.OrdinalIgnoreCase));

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, IModule>> AsKeyValuePairs() => _modules;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public IEnumerator<IModule> GetEnumerator() => _modules.Select(x => x.Value).GetEnumerator();

        private KeyValuePair<string, IModule> ExpandNamedModule(IModule module)
        {
            NamedModule namedModule = module as NamedModule;
            return namedModule != null
                ? new KeyValuePair<string, IModule>(namedModule.Name, namedModule.Module)
                : new KeyValuePair<string, IModule>(null, module);
        }

        private void CheckName(string name)
        {
            if (name != null && _modules.Any(x => x.Key != null && string.Equals(x.Key, name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"A module with the name {name} already exists");
            }
        }
    }
}
