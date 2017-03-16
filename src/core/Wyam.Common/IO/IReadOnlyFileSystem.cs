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
        /// Gets the file providers.
        /// </summary>
        /// <value>
        /// The file providers.
        /// </value>
        IReadOnlyFileProviderCollection FileProviders { get; }

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
        /// Gets the temporary file path.
        /// </summary>
        /// <value>
        /// The temporary file path.
        /// </value>
        DirectoryPath TempPath { get; }

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
        /// Gets matching input files based on globbing patterns and/or absolute paths. If any absolute paths
        /// are provided, only those that actually exist are returned.
        /// </summary>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>All input files that match the globbing patterns and/or absolute paths.</returns>
        IEnumerable<IFile> GetInputFiles(params string[] patterns);

        /// <summary>
        /// Gets matching input files based on globbing patterns and/or absolute paths. If any absolute paths
        /// are provided, only those that actually exist are returned.
        /// </summary>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>All input files that match the globbing patterns and/or absolute paths.</returns>
        IEnumerable<IFile> GetInputFiles(IEnumerable<string> patterns);

        /// <summary>
        /// Gets a directory representing an input.
        /// </summary>
        /// <param name="path">
        /// The path of the input directory. If this is an absolute path,
        /// then a directory representing the specified path is returned.
        /// If it's a relative path, then the returned directory will
        /// be a virtual directory that aggregates all input
        /// paths. If this is <c>null</c> then a virtual
        /// directory aggregating all input paths is returned.
        /// </param>
        /// <returns>An input directory.</returns>
        IDirectory GetInputDirectory(DirectoryPath path = null);

        /// <summary>
        /// Gets all absolute input directories.
        /// </summary>
        /// <returns>The absolute input directories.</returns>
        IReadOnlyList<IDirectory> GetInputDirectories();

        /// <summary>
        /// Gets the absolute input path that contains the specified file or directory. If the provided
        /// file or directory path is absolute, this returns the input path that contains the specified
        /// path (note that the specified file or directory does not need to exist and this just returns
        /// the input path that would contain the file or directory based only on path information). If
        /// the provided path is relative, this checks all input paths for the existence of the file
        /// or directory and returns the first one where it exists.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>The input path that contains the specified file,
        /// or <c>null</c> if no input path does.</returns>
        DirectoryPath GetContainingInputPath(NormalizedPath path);

        /// <summary>
        /// Gets an output file path by combining it with the root path and output path.
        /// </summary>
        /// <param name="path">The path to combine with the root path and output path.</param>
        /// <returns>The output file path.</returns>
        FilePath GetOutputPath(FilePath path);

        /// <summary>
        /// Gets an output directory path by combining it with the root path and output path.
        /// </summary>
        /// <param name="path">The path to combine with the root path and output path.
        /// If this is <c>null</c>, returns the root path combined with the output path.</param>
        /// <returns>The output directory path.</returns>
        DirectoryPath GetOutputPath(DirectoryPath path = null);

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
        /// current output path. If this is <c>null</c> then the base
        /// output directory is returned.
        /// </param>
        /// <returns>An output directory.</returns>
        IDirectory GetOutputDirectory(DirectoryPath path = null);

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
        /// current root path. If this is <c>null</c> then the base
        /// root directory is returned.
        /// </param>
        /// <returns>A root directory.</returns>
        IDirectory GetRootDirectory(DirectoryPath path = null);

        /// <summary>
        /// Gets a temp file path by combining it with the root path and temp path.
        /// </summary>
        /// <param name="path">The path to combine with the root path and temp path.</param>
        /// <returns>The temp file path.</returns>
        FilePath GetTempPath(FilePath path);

        /// <summary>
        /// Gets a temp directory path by combining it with the root path and temp path.
        /// </summary>
        /// <param name="path">The path to combine with the root path and temp path.
        /// If this is <c>null</c>, returns the root path combined with the temp path.</param>
        /// <returns>The temp directory path.</returns>
        DirectoryPath GetTempPath(DirectoryPath path = null);

        /// <summary>
        /// Gets a file representing a temp file.
        /// </summary>
        /// <param name="path">
        /// If this is an absolute path,
        /// then a file representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current temp path.
        /// </param>
        /// <returns>A temp file.</returns>
        IFile GetTempFile(FilePath path);

        /// <summary>
        /// Gets a file representing a temp file with a random file name.
        /// </summary>
        /// <returns>A temp file.</returns>
        IFile GetTempFile();

        /// <summary>
        /// Gets a directory representing temp files.
        /// </summary>
        /// <param name="path">
        /// The path of the temp directory. If this is an absolute path,
        /// then a directory representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current temp path. If this is <c>null</c> then the base
        /// temp directory is returned.
        /// </param>
        /// <returns>A temp directory.</returns>
        IDirectory GetTempDirectory(DirectoryPath path = null);

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

        /// <summary>
        /// Gets matching files based on globbing patterns from the root path or absolute paths.
        /// </summary>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>
        /// All files in the specified directory that match the globbing patterns and/or absolute paths.
        /// </returns>
        IEnumerable<IFile> GetFiles(string[] patterns);

        /// <summary>
        /// Gets matching files based on globbing patterns from the root path or absolute paths.
        /// </summary>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>
        /// All files in the specified directory that match the globbing patterns and/or absolute paths.
        /// </returns>
        IEnumerable<IFile> GetFiles(IEnumerable<string> patterns);

        /// <summary>
        /// Gets matching files based on globbing patterns and/or absolute paths. If any absolute paths
        /// are provided, only those that actually exist are returned.
        /// </summary>
        /// <param name="directory">The directory to search.</param>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>
        /// All files in the specified directory that match the globbing patterns and/or absolute paths.
        /// </returns>
        IEnumerable<IFile> GetFiles(IDirectory directory, params string[] patterns);

        /// <summary>
        /// Gets matching files based on globbing patterns and/or absolute paths. If any absolute paths
        /// are provided, only those that actually exist are returned.
        /// </summary>
        /// <param name="directory">The directory to search.</param>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>
        /// All files in the specified directory that match the globbing patterns and/or absolute paths.
        /// </returns>
        IEnumerable<IFile> GetFiles(IDirectory directory, IEnumerable<string> patterns);
    }
}
