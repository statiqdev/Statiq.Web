using System;
using System.Collections;
using System.Collections.Generic;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;

namespace Wyam.Common.Execution
{
    /// <summary>
    /// A utility class for specifying pipelines. Note that this is not required
    /// for pipeline creation and is typically only used when a pipeline needs to
    /// be specified outside the context of a configuration file (such as a recipe,
    /// though it's not even required for those). Instances of this class
    /// are typically assigned to read-only properties in a <see cref="Recipe"/> class and by
    /// convention the class name should match the property name (an exception will be thrown during
    /// recipe application if not). This class also implements string conversions and operators
    /// that return the pipeline name (so it can be used as a key for the pipeline it defines).
    /// Note that once this pipeline is added to an <see cref="IPipelineCollection"/>, the actual
    /// instance probably won't be what's stored by the collection and should not be used for comparisons.
    /// </summary>
    public class Pipeline : IPipeline
    {
        private readonly IModuleList _modules;

        /// <summary>
        /// Creates a pipeline with an empty modules collection
        /// and a default name equal to the class name.
        /// </summary>
        public Pipeline()
            : this(null, (IEnumerable<IModule>)null)
        {
        }

        /// <summary>
        /// Creates a pipeline with the specified modules
        /// and a default name equal to the class name.
        /// </summary>
        /// <param name="modules">The modules in the pipeline.</param>
        public Pipeline(params IModule[] modules)
            : this(null, (IEnumerable<IModule>)modules)
        {
        }

        /// <summary>
        /// Creates a pipeline with the specified modules
        /// and a default name equal to the class name.
        /// </summary>
        /// <param name="modules">The modules in the pipeline.</param>
        public Pipeline(IEnumerable<IModule> modules)
            : this(null, modules)
        {
        }

        /// <summary>
        /// Creates a pipeline with the specified modules
        /// and the specified name.
        /// </summary>
        /// <param name="name">The name of the pipeline.</param>
        /// <param name="modules">The modules in the pipeline.</param>
        public Pipeline(string name, params IModule[] modules)
            : this(name, (IEnumerable<IModule>)modules)
        {
        }

        /// <summary>
        /// Creates a pipeline with the specified modules
        /// and the specified name.
        /// </summary>
        /// <param name="name">The name of the pipeline.</param>
        /// <param name="modules">The modules in the pipeline.</param>
        public Pipeline(string name, IEnumerable<IModule> modules)
        {
            Name = name ?? GetType().Name;
            _modules = (modules as IModuleList) ?? new ModuleList(modules);
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public bool ProcessDocumentsOnce { get; set; }

        /// <summary>
        /// Provides the name for the pipeline when converted to a string.
        /// </summary>
        /// <param name="pipeline">The current instance.</param>
        public static implicit operator string(Pipeline pipeline) => pipeline?.Name;

        /// <inheritdoc />
        public override string ToString() => Name;

        /// <inheritdoc />
        public override int GetHashCode() => Name.GetHashCode();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public IEnumerator<IModule> GetEnumerator() => _modules.GetEnumerator();

        /// <inheritdoc />
        public void Add(IModule item) => _modules.Add(item);

        /// <inheritdoc />
        public void Clear() => _modules.Clear();

        /// <inheritdoc />
        public bool Contains(IModule item) => _modules.Contains(item);

        /// <inheritdoc />
        public void CopyTo(IModule[] array, int arrayIndex) => _modules.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public bool Remove(IModule item) => _modules.Remove(item);

        /// <inheritdoc />
        public bool Remove(string name) => _modules.Remove(name);

        /// <inheritdoc />
        public int Count => _modules.Count;

        /// <inheritdoc />
        public void Add(params IModule[] modules) => _modules.Add(modules);

        /// <inheritdoc />
        public void Insert(int index, params IModule[] modules) => _modules.Insert(index, modules);

        /// <inheritdoc />
        public int IndexOf(string name) => _modules.IndexOf(name);

        /// <inheritdoc />
        public bool IsReadOnly => _modules.IsReadOnly;

        /// <inheritdoc />
        public int IndexOf(IModule item) => _modules.IndexOf(item);

        /// <inheritdoc />
        public void Insert(int index, IModule item) => _modules.Insert(index, item);

        /// <inheritdoc />
        public void RemoveAt(int index) => _modules.RemoveAt(index);

        /// <inheritdoc />
        public IModule this[int index]
        {
            get { return _modules[index]; }
            set { _modules[index] = value; }
        }

        /// <inheritdoc />
        public bool Contains(string name) => _modules.Contains(name);

        /// <inheritdoc />
        public bool TryGetValue(string name, out IModule value) => _modules.TryGetValue(name, out value);

        /// <inheritdoc />
        public IModule this[string name] => _modules[name];

        /// <inheritdoc />
        public void Add(string name, IModule module) => _modules.Add(name, module);

        /// <inheritdoc />
        public void Insert(int index, string name, IModule module) => _modules.Insert(index, name, module);

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, IModule>> AsKeyValuePairs() => _modules.AsKeyValuePairs();
    }
}