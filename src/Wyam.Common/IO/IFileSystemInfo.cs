using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.IO
{
    // Initially based on code from Cake (http://cakebuild.net/)
    /// <summary>
    /// Represents an entry in the file system
    /// </summary>
    public interface IFileSystemInfo
    {
        /// <summary>
        /// Gets the path to the entry.
        /// </summary>
        /// <value>The path.</value>
        Path Path { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IFileSystemInfo"/> exists.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the entry exists; otherwise, <c>false</c>.
        /// </value>
        bool Exists { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IFileSystemInfo"/> is hidden.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the entry is hidden; otherwise, <c>false</c>.
        /// </value>
        bool Hidden { get; }
    }
}
