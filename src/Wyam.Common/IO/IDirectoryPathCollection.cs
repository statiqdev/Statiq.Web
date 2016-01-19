using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.IO
{
    /// <summary>
    /// A ordered collection of <see cref="DirectoryPath"/>. This also works like a <see cref="HashSet{T}"/>
    /// by ensuring each path is unique.
    /// </summary>
    public interface IDirectoryPathCollection : IReadOnlyList<DirectoryPath>
    {
        /// <summary>
        /// Adds the specified path to the collection.
        /// </summary>
        /// <param name="path">The path to add.</param>
        /// <returns>
        /// <c>true</c> if the path was added to the collection, 
        /// <c>false</c> if it was already in the collection.
        /// </returns>
        bool Add(DirectoryPath path);

        /// <summary>
        /// Clears all paths from the collection.
        /// </summary>
        void Clear();

        /// <summary>
        /// Determines whether the collection contains the specified path.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns><c>true</c> if the collection contains the path, otherwise <c>false</c>.</returns>
        bool Contains(DirectoryPath path);

        /// <summary>
        /// Removes the specified path from the collection.
        /// </summary>
        /// <param name="path">The path to remove.</param>
        /// <returns>
        /// <c>true</c> if the path was removed from the collection, 
        /// otherwise <c>false</c> if the path wasn't found in the collection.
        /// </returns>
        bool Remove(DirectoryPath path);

        /// <summary>
        /// Gets the zero-based index of the specified path within the collection.
        /// </summary>
        /// <param name="path">The path to find.</param>
        /// <returns>The zero-based index of the specified path within the collection or -1 if not found.</returns>
        int IndexOf(DirectoryPath path);

        /// <summary>
        /// Inserts the path at the specified index.
        /// </summary>
        /// <param name="index">The index at which to insert the path.</param>
        /// <param name="path">The path to insert.</param>
        /// <returns>
        /// <c>true</c> if the path was inserted into the collection, 
        /// <c>false</c> if it was already in the collection.
        /// </returns>
        bool Insert(int index, DirectoryPath path);

        /// <summary>
        /// Removes the path at the specified index.
        /// </summary>
        /// <param name="index">The index of the path to remove.</param>
        void RemoveAt(int index);
    }
}
