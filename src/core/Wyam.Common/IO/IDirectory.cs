using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.IO
{
    // Initially based on code from Cake (http://cakebuild.net/)
    /// <summary>
    /// Represents a directory. Not all implementations will support all
    /// available methods and may throw <see cref="NotSupportedException"/>.
    /// </summary>
    public interface IDirectory : IFileSystemEntry
    {
        /// <summary>
        /// Gets the path to the directory.
        /// </summary>
        /// <value>The path.</value>
        new DirectoryPath Path { get; }

        /// <summary>
        /// Gets the parent directory or <c>null</c> if the directory is a root.
        /// </summary>
        IDirectory Parent { get; }
        
        /// <summary>
        /// Creates the directory, including any necessary parent directories.
        /// </summary>
        void Create();

        /// <summary>
        /// Deletes the directory.
        /// </summary>
        /// <param name="recursive">Will perform a recursive delete if set to <c>true</c>.</param>
        void Delete(bool recursive);

        /// <summary>
        /// Gets directories matching the specified filter and scope.
        /// </summary>
        /// <param name="searchOption">
        /// Specifies whether the operation should include only 
        /// the current directory or should include all subdirectories.
        /// </param>
        /// <returns>Directories matching the filter and scope.</returns>
        IEnumerable<IDirectory> GetDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly);

        /// <summary>
        /// Gets files matching the specified filter and scope.
        /// </summary>
        /// <param name="searchOption">
        /// Specifies whether the operation should include only 
        /// the current directory or should include all subdirectories.
        /// </param>
        /// <returns>Files matching the specified filter and scope.</returns>
        IEnumerable<IFile> GetFiles(SearchOption searchOption = SearchOption.TopDirectoryOnly);

        /// <summary>
        /// Gets a directory by combining it's path with the current directory's path.
        /// The specified directory path must be relative.
        /// </summary>
        /// <param name="directory">The path of the directory.</param>
        /// <returns>The directory.</returns>
        IDirectory GetDirectory(DirectoryPath directory);

        /// <summary>
        /// Gets a file by combining it's path with the current directory's path.
        /// The specified file path must be relative.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>The file.</returns>
        IFile GetFile(FilePath path);
    }
}
