using System.Collections.Generic;

namespace Wyam.Common.IO
{
    /// <summary>
    /// A read-only collection of file providers mapped to schemes.
    /// </summary>
    public interface IReadOnlyFileProviderCollection
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
        /// <param name="scheme">The scheme the provider supports.</param>
        /// <returns>The requested <see cref="IFileProvider"/>.</returns>
        IFileProvider Get(string scheme);

        /// <summary>
        /// Tries to get the requested file provider.
        /// </summary>
        /// <param name="scheme">The scheme the provider supports.</param>
        /// <param name="fileProvider">The file provider.</param>
        /// <returns><c>true</c> if the provider was found, otherwise <c>false</c>.</returns>
        bool TryGet(string scheme, out IFileProvider fileProvider);
    }
}