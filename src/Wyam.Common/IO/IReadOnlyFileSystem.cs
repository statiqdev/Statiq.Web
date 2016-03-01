using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization.Configuration;

namespace Wyam.Common.IO
{
    // Initially based on code from Cake (http://cakebuild.net/)
    /// <summary>
    /// Represents a file system.
    /// </summary>
    public interface IReadOnlyFileSystem
    {
        /// <summary>
        /// Gets a value indicating whether the file system is case sensitive.
        /// </summary>
        /// <value>
        /// <c>true</c> if the file system is case sensitive; otherwise, <c>false</c>.
        /// </value>
        bool IsCaseSensitive { get; }

        /// <summary>
        /// Gets the file providers.
        /// </summary>
        /// <value>
        /// The file providers.
        /// </value>
        IReadOnlyFileProviderCollection FileProviders { get; }

        /// <summary>
        /// Gets the default path comparer for this file system.
        /// </summary>
        /// <value>
        /// The default path comparer for this file system.
        /// </value>
        PathComparer PathComparer { get; }

        /// <summary>
        /// Gets the root path.
        /// </summary>
        /// <value>
        /// The root path.
        /// </value>
        DirectoryPath RootPath { get; }

        /// <summary>
        /// Gets the input paths. These are searched in reverse order for
        /// files and directories. For example, given input paths "A", "B",
        /// and "C" in that order, "C" will be checked for a requested file 
        /// or directory first, and then if it doesn't exist in "C", "B" 
        /// will be checked, and then "A". If none of the input paths contain
        /// the requested file or directory, the last input path (in this case, 
        /// "C") will be used as the location of the requested non-existent file
        /// or directory. If you attempt to create it at this point, it will be
        /// created under path "C".
        /// </summary>
        /// <value>
        /// The input paths.
        /// </value>
        IReadOnlyList<DirectoryPath> InputPaths { get; }

        /// <summary>
        /// Gets the output path.
        /// </summary>
        /// <value>
        /// The output path.
        /// </value>
        DirectoryPath OutputPath { get; }

        /// <summary>
        /// Gets a file representing an input.
        /// </summary>
        /// <param name="path">
        /// The path of the input file. If this is an absolute path,
        /// then a file representing the specified path is returned.
        /// If it's a relative path, then operations will search all
        /// current input paths.
        /// </param>
        /// <returns>An input file.</returns>
        IFile GetInputFile(FilePath path);

        /// <summary>
        /// Gets a directory representing an input.
        /// </summary>
        /// <param name="path">
        /// The path of the input directory. If this is an absolute path,
        /// then a directory representing the specified path is returned.
        /// If it's a relative path, then operations will search all
        /// current input paths.
        /// </param>
        /// <returns>An input directory.</returns>
        IDirectory GetInputDirectory(DirectoryPath path);

        /// <summary>
        /// Gets all absolute input directories.
        /// </summary>
        /// <returns>The absolute input directories.</returns>
        IReadOnlyList<IDirectory> GetInputDirectories();

        /// <summary>
        /// Gets a file representing an output.
        /// </summary>
        /// <param name="path">
        /// The path of the output file. If this is an absolute path,
        /// then a file representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current output path.
        /// </param>
        /// <returns>An output file.</returns>
        IFile GetOutputFile(FilePath path);

        /// <summary>
        /// Gets a directory representing an output.
        /// </summary>
        /// <param name="path">
        /// The path of the output directory. If this is an absolute path,
        /// then a directory representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current output path.
        /// </param>
        /// <returns>An output directory.</returns>
        IDirectory GetOutputDirectory(DirectoryPath path);

        /// <summary>
        /// Gets the absolute output directory.
        /// </summary>
        /// <returns>The absolute output directory.</returns>
        IDirectory GetOutputDirectory();

        /// <summary>
        /// Gets a file representing a root file.
        /// </summary>
        /// <param name="path">
        /// The path of the root file. If this is an absolute path,
        /// then a file representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current root path.
        /// </param>
        /// <returns>A root file.</returns>
        IFile GetRootFile(FilePath path);

        /// <summary>
        /// Gets a directory representing a root directory.
        /// </summary>
        /// <param name="path">
        /// The path of the root directory. If this is an absolute path,
        /// then a directory representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current root path.
        /// </param>
        /// <returns>A root directory.</returns>
        IDirectory GetRootDirectory(DirectoryPath path);

        /// <summary>
        /// Gets the absolute directory.
        /// </summary>
        /// <returns>The absolute root directory.</returns>
        IDirectory GetRootDirectory();

        /// <summary>
        /// Gets an absolute file.
        /// </summary>
        /// <param name="path">
        /// The absolute path of the file.
        /// </param>
        /// <returns>A file.</returns>
        IFile GetFile(FilePath path);

        /// <summary>
        /// Gets an absolute directory.
        /// </summary>
        /// <param name="path">
        /// The absolute path of the directory.
        /// </param>
        /// <returns>A directory.</returns>
        IDirectory GetDirectory(DirectoryPath path);
    }
}
