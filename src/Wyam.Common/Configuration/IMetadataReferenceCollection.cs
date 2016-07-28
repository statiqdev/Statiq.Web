using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.Configuration
{
    /// <summary>
    /// A collection of Roslyn <c>MetadataReference</c> objects for all dynamically
    /// compiled assemblies such as the configuration script.
    /// </summary>
    public interface IMetadataReferenceCollection : IReadOnlyCollection<object>
    {
        /// <summary>
        /// Adds the specified metadata reference.
        /// </summary>
        /// <param name="metadataReference">The metadata reference.</param>
        void Add(object metadataReference);
    }
}
