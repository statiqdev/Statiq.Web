using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.IO
{
    /// <summary>
    /// Compares <see cref="Path"/> instances.
    /// </summary>
    public sealed class PathComparer : IEqualityComparer<Path>
    {
        private readonly bool _isCaseSensitive;

        /// <summary>
        /// Gets the file system (if one is set).
        /// </summary>
        /// <value>
        /// The file system or <c>null</c>.
        /// </value>
        public IFileSystem FileSystem { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this comparer is case sensitive.
        /// </summary>
        /// <value>
        /// <c>true</c> if this comparer is case sensitive; otherwise, <c>false</c>.
        /// </value>
        public bool IsCaseSensitive => FileSystem?.IsCaseSensitive ?? _isCaseSensitive;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathComparer"/> class.
        /// </summary>
        /// <param name="isCaseSensitive">if set to <c>true</c>, comparison is case sensitive.</param>
        public PathComparer(bool isCaseSensitive)
        {
            _isCaseSensitive = isCaseSensitive;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathComparer"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        public PathComparer(IFileSystem fileSystem)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }
            FileSystem = fileSystem;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Path"/> instances are equal.
        /// </summary>
        /// <param name="x">The first <see cref="Path"/> to compare.</param>
        /// <param name="y">The second <see cref="Path"/> to compare.</param>
        /// <returns>
        /// True if the specified <see cref="Path"/> instances are equal; otherwise, false.
        /// </returns>
        public bool Equals(Path x, Path y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }

            return IsCaseSensitive 
                ? x.FullPath.Equals(y.FullPath) 
                : x.FullPath.Equals(y.FullPath, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns a hash code for the specified <see cref="Path"/>.
        /// </summary>
        /// <param name="obj">The path.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public int GetHashCode(Path obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            return IsCaseSensitive 
                ? obj.FullPath.GetHashCode() 
                : obj.FullPath.ToUpperInvariant().GetHashCode();
        }
    }
}
