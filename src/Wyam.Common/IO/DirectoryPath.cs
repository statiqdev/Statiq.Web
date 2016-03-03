using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Pipelines;

namespace Wyam.Common.IO
{
    // Initially based on code from Cake (http://cakebuild.net/)
    /// <summary>
    /// Represents a directory path.
    /// </summary>
    public sealed class DirectoryPath : NormalizedPath
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryPath"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public DirectoryPath(string path)
            : base(path)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryPath" /> class
        /// with the specified provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="path">The path.</param>
        public DirectoryPath(string provider, string path)
                    : base(provider, path)
        {
        }

        /// <summary>
        /// Gets the name of the directory.
        /// </summary>
        /// <returns>The directory name.</returns>
        /// <remarks>
        /// If this is passed a file path, it will return the file name.
        /// This is by-and-large equivalent to how DirectoryInfo handles this scenario.
        /// If we wanted to return the *actual* directory name, we'd need to pull in IFileSystem,
        /// and do various checks to make sure things exists.
        /// </remarks>
        public string GetDirectoryName() => Segments.Last();

        /// <summary>
        /// Combines the current path with the file name of a <see cref="FilePath"/>. The current file provider
        /// is maintained.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A combination of the current path and the file name of the provided <see cref="FilePath"/>.</returns>
        public FilePath GetFilePath(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            return new FilePath(Provider, System.IO.Path.Combine(FullPath, path.GetFilename().FullPath));
        }

        /// <summary>
        /// Get the relative path to another directory.
        /// </summary>
        /// <param name="target">The target directory path.</param>
        /// <returns>A <see cref="DirectoryPath"/>.</returns>
        public DirectoryPath GetRelativePath(DirectoryPath target)
        {
            return RelativePathResolver.Resolve(this, target);
        }

        /// <summary>
        /// Get the relative path to another file.
        /// </summary>
        /// <param name="target">The target file path.</param>
        /// <returns>A <see cref="FilePath"/>.</returns>
        public FilePath GetRelativePath(FilePath target)
        {
            return RelativePathResolver.Resolve(this, target);
        }

        /// <summary>
        /// Combines the current path with a <see cref="FilePath"/>.
        /// If the provided <see cref="FilePath"/> is not relative, then it is returned.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A combination of the current path and the provided <see cref="FilePath"/>, unless
        /// the provided <see cref="FilePath"/> is absolute in which case it is returned.</returns>
        public FilePath CombineFile(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            return !path.IsRelative ? path : new FilePath(Provider, System.IO.Path.Combine(FullPath, path.FullPath));
        }

        /// <summary>
        /// Combines the current path with another <see cref="DirectoryPath"/>.
        /// If the provided <see cref="DirectoryPath"/> is not relative, then it is returned.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A combination of the current path and the provided <see cref="DirectoryPath"/>, unless
        /// the provided <see cref="DirectoryPath"/> is absolute in which case it is returned.</returns>
        public DirectoryPath Combine(DirectoryPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            return !path.IsRelative ? path : new DirectoryPath(Provider, System.IO.Path.Combine(FullPath, path.FullPath));
        }

        /// <summary>
        /// Collapses a <see cref="DirectoryPath"/> containing ellipses.
        /// </summary>
        /// <returns>A collapsed <see cref="DirectoryPath"/>.</returns>
        public DirectoryPath Collapse() => new DirectoryPath(Provider, Collapse(this));
        
        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="DirectoryPath"/>.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A <see cref="DirectoryPath"/>.</returns>
        public static implicit operator DirectoryPath(string path)
        {
            return FromString(path);
        }

        /// <summary>
        /// Performs a conversion from <see cref="System.String"/> to <see cref="DirectoryPath"/>.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A <see cref="DirectoryPath"/>.</returns>
        public static DirectoryPath FromString(string path) => new DirectoryPath(path);
    }
}
