using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Configuration
{
    /// <summary>
    /// A collection of raw assembly bytes for dynamically
    /// compiled assemblies such as the configuration script.
    /// </summary>
    public interface IRawAssemblyCollection : IReadOnlyCollection<byte[]>
    {
        /// <summary>
        /// Adds a raw assembly to the collection.
        /// </summary>
        /// <param name="rawAssembly">The bytes of the assembly to add.</param>
        void Add(byte[] rawAssembly);
    }
}
