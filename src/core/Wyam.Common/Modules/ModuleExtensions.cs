using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Modules
{
    /// <summary>
    /// Extension methods for <see cref="IModule"/>.
    /// </summary>
    public static class ModuleExtensions
    {
        /// <summary>
        /// Converts the module to a named module.
        /// </summary>
        /// <param name="module">The module to provide a name for.</param>
        /// <param name="name">The name to provide to the module.</param>
        /// <returns>The named module.</returns>
        public static IModule WithName(this IModule module, string name) => new NamedModule(name, module);
    }
}
