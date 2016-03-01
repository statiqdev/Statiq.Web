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
        /// Gets the current file providers.
        /// </summary>
        /// <value>
        /// The current file providers.
        /// </value>
        IReadOnlyDictionary<string, IFileProvider> Providers { get; }

        /// <summary>
        /// Gets the requested file provider. Throws <see cref="KeyNotFoundException"/>
        /// if the provider couldn't be found.
        /// </summary>
        /// <param name="name">The name of the provider.</param>
        /// <returns>The requested <see cref="IFileProvider"/>.</returns>
        IFileProvider Get(string name);

        /// <summary>
        /// Tries to get the requested file provider.
        /// </summary>
        /// <param name="name">The name of the provider.</param>
        /// <param name="fileProvider">The file provider.</param>
        /// <returns><c>true</c> if the provider was found and removed, otherwise <c>false</c>.</returns>
        bool TryGet(string name, out IFileProvider fileProvider);
    }
}
