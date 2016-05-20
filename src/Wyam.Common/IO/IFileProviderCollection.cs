using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.IO
{
    public interface IFileProviderCollection : IReadOnlyFileProviderCollection
    {
        /// <summary>
        /// Adds a file provider.
        /// </summary>
        /// <param name="scheme">The scheme the file provider supports.</param>
        /// <param name="provider">The file provider.</param>
        /// <returns><c>true</c> if the provider already existed and was overwritten, 
        /// <c>false</c> if no provider with the specified name existed.</returns>
        bool Add(string scheme, IFileProvider provider);

        /// <summary>
        /// Removes a file provider by scheme.
        /// </summary>
        /// <param name="scheme">The scheme to remove.</param>
        /// <returns><c>true</c> if the provider was found and removed, 
        /// <c>false</c> if the provider was not found.</returns>
        bool Remove(string scheme);
    }
}
