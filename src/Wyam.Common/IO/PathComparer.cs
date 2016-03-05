using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.IO
{
    /// <summary>
    /// Compares <see cref="NormalizedPath"/> instances.
    /// </summary>
    public sealed class PathComparer : IEqualityComparer<NormalizedPath>
    {
        /// <summary>
        /// Determines whether the specified <see cref="NormalizedPath"/> instances are equal.
        /// </summary>
        /// <param name="x">The first <see cref="NormalizedPath"/> to compare.</param>
        /// <param name="y">The second <see cref="NormalizedPath"/> to compare.</param>
        /// <returns>
        /// True if the specified <see cref="NormalizedPath"/> instances are equal; otherwise, false.
        /// </returns>
        public bool Equals(NormalizedPath x, NormalizedPath y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }

            return x.Equals(y);
        }

        /// <summary>
        /// Returns a hash code for the specified <see cref="NormalizedPath"/>.
        /// </summary>
        /// <param name="obj">The path.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public int GetHashCode(NormalizedPath obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            return obj.GetHashCode();
        }
    }
}
