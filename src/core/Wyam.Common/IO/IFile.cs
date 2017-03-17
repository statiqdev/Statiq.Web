using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.IO
{
    /// <summary>
    /// Represents a file. Not all implementations will support all
    /// available methods and may throw <see cref="NotSupportedException"/>.
    /// </summary>
    // Initially based on code from Cake (http://cakebuild.net/)
    public interface IFile : IFileSystemEntry
    {
        /// <summary>
        /// Gets the path to the file.
        /// </summary>
        /// <value>The path.</value>
        new FilePath Path { get; }

        /// <summary>
        /// Gets the directory of the file.
        /// </summary>
        /// <value>
        /// The directory of the file.
        /// </value>
        IDirectory Directory { get; }

        /// <summary>
        /// Gets the length of the file.
        /// </summary>
        /// <value>The length of the file.</value>
        long Length { get; }

        /// <summary>
        /// Copies the file to the specified destination file.
        /// </summary>
        /// <param name="destination">The destination file.</param>
        /// <param name="overwrite">Will overwrite existing destination file if set to <c>true</c>.</param>
        /// <param name="createDirectory">Will create any needed directories that don't already exist if set to <c>true</c>.</param>
        void CopyTo(IFile destination, bool overwrite = true, bool createDirectory = true);

        /// <summary>
        /// Moves the file to the specified destination file.
        /// </summary>
        /// <param name="destination">The destination file.</param>
        void MoveTo(IFile destination);

        /// <summary>
        /// Deletes the file.
        /// </summary>
        void Delete();

        /// <summary>
        /// Reads all text from the file.
        /// </summary>
        /// <returns>All text in the file.</returns>
        string ReadAllText();

        /// <summary>
        /// Writes the specified text to a file.
        /// </summary>
        /// <param name="contents">The text to write.</param>
        /// <param name="createDirectory">Will create any needed directories that don't already exist if set to <c>true</c>.</param>
        void WriteAllText(string contents, bool createDirectory = true);

        /// <summary>
        /// Opens the file for reading. If it does not exist, an exception
        /// will be thrown.
        /// </summary>
        /// <returns>The stream.</returns>
        Stream OpenRead();

        /// <summary>
        /// Opens the file for writing. This will either create the file
        /// if it doesn't exist or overwrite it if it does.
        /// </summary>
        /// <param name="createDirectory">Will create any needed directories that don't already exist if set to <c>true</c>.</param>
        /// <returns>The stream.</returns>
        Stream OpenWrite(bool createDirectory = true);

        /// <summary>
        /// Opens the file for writing. This will either create the file
        /// if it doesn't exist or append to it if it does.
        /// </summary>
        /// <param name="createDirectory">Will create any needed directories that don't already exist if set to <c>true</c>.</param>
        /// <returns>The stream.</returns>
        Stream OpenAppend(bool createDirectory = true);

        /// <summary>
        /// Opens the file for reading and writing. This will either create the file
        /// if it doesn't exist or overwrite it if it does.
        /// </summary>
        /// <param name="createDirectory">Will create any needed directories that don't already exist if set to <c>true</c>.</param>
        /// <returns>The stream.</returns>
        Stream Open(bool createDirectory = true);
    }
}
