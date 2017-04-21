using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;

namespace Wyam.Common.Modules
{
    /// <summary>
    /// A collection of optionally named modules. Implementations should "unwrap" <see cref="NamedModule"/>
    /// objects to obtain the module name.
    /// </summary>
    public interface IModuleList : IList<IModule>
    {
        /// <summary>
        /// Adds a module to the list with a specified name.
        /// </summary>
        /// <param name="name">The name of the module to add.</param>
        /// <param name="module">The module to add.</param>
        void Add(string name, IModule module);

        /// <summary>
        /// Adds modules to the list.
        /// Any <c>null</c> items in the sequence of modules will be discarded.
        /// </summary>
        /// <param name="modules">The modules to add.</param>
        void Add(params IModule[] modules);

        /// <summary>
        /// Inserts a module into the list with a specified name.
        /// </summary>
        /// <param name="index">The index at which to insert the module.</param>
        /// <param name="name">The name of the inserted module.</param>
        /// <param name="module">The module to insert/</param>
        void Insert(int index, string name, IModule module);

        /// <summary>
        /// Inserts modules into the list.
        /// Any <c>null</c> items in the sequence of modules will be discarded.
        /// </summary>
        /// <param name="index">The index at which to insert the modules.</param>
        /// <param name="modules">The modules to insert.</param>
        void Insert(int index, params IModule[] modules);

        /// <summary>
        /// Removes a module by name.
        /// </summary>
        /// <param name="name">The name of the module to remove.</param>
        /// <returns><c>true</c> if a module with the specified name was found and removed, otherwise <c>false</c>.</returns>
        bool Remove(string name);

        /// <summary>
        /// Gets the index of the module with the specified name.
        /// </summary>
        /// <param name="name">The name of the module.</param>
        /// <returns>The index of the module with the specified name.</returns>
        int IndexOf(string name);

        /// <summary>
        /// Determines if the list contains a module with the specified name.
        /// </summary>
        /// <param name="name">The name of the module.</param>
        /// <returns><c>true</c> if a module exists with the specified name, otherwise <c>false</c>.</returns>
        bool Contains(string name);

        /// <summary>
        /// Attempts to get a module with the specified name.
        /// </summary>
        /// <param name="name">The name of the module.</param>
        /// <param name="value">The module with the specified name.</param>
        /// <returns><c>true</c> if a module was found with the specified name, otherwise <c>false</c>.</returns>
        bool TryGetValue(string name, out IModule value);

        /// <summary>
        /// Gets the module with the specified name.
        /// </summary>
        /// <param name="name">The name of the module.</param>
        /// <returns>The module with the specified name.</returns>
        IModule this[string name] { get; }

        /// <summary>
        /// Returns the list as a sequence of key-value pairs with the keys being
        /// the module names and the values being the module instances.
        /// </summary>
        /// <returns>The list as a sequence of key-value pairs.</returns>
        IEnumerable<KeyValuePair<string, IModule>> AsKeyValuePairs();
    }
}
