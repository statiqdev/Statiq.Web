namespace Wyam.Common.IO
{
    public interface IReadOnlyFileProviderCollection
    {
        /// <summary>
        /// Adds a file provider.
        /// </summary>
        /// <param name="name">The name of the file provider.</param>
        /// <param name="provider">The file provider.</param>
        /// <returns><c>true</c> if the provider already existed and was overwritten, 
        /// <c>false</c> if no provider with the specified name existed.</returns>
        bool Add(string name, IFileProvider provider);

        /// <summary>
        /// Removes a file provider.
        /// </summary>
        /// <param name="name">The name of the file provider.</param>
        /// <returns><c>true</c> if the provider was found and removed, 
        /// <c>false</c> if the provider was not found.</returns>
        bool Remove(string name);
    }
}