using System.Collections.Generic;

namespace Wyam.Common.Modules
{
    /// <summary>
    /// A collection of optionally named modules.
    /// </summary>
    public interface IReadOnlyModuleList : IReadOnlyList<IModule>
    {
        /// <summary>
        /// Gets the index of a named module.
        /// </summary>
        /// <param name="name">The name of the module.</param>
        /// <returns>The index of the requested module or -1 if not found.</returns>
        int IndexOf(string name);

        /// <summary>
        /// Determines whether the list contains a module with a given name.
        /// </summary>
        /// <param name="name">The name of the module.</param>
        /// <returns><c>true</c> if a module with the given name exists in the list, <c>false</c> otherwise.</returns>
        bool Contains(string name);

        /// <summary>
        /// Attempts to get a module with a given name.
        /// </summary>
        /// <param name="name">The name of the module.</param>
        /// <param name="value">The module instance.</param>
        /// <returns><c>true</c> if a module with the given name exists in the list, <c>false</c> otherwise.</returns>
        bool TryGetValue(string name, out IModule value);

        /// <summary>
        /// Gets a module with a given name.
        /// </summary>
        /// <param name="name">The name of the module.</param>
        /// <returns>The module instance.</returns>
        IModule this[string name] { get; }

        /// <summary>
        /// Casts the list to a <c>IEnumerable&lt;KeyValuePair&lt;string, IModule&gt;&gt;</c>.
        /// </summary>
        /// <returns>An enumerable of <see cref="KeyValuePair{TKey, TValue}"/>.</returns>
        IEnumerable<KeyValuePair<string, IModule>> AsKeyValuePairs();
    }
}