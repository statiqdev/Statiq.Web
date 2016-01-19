using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Pipelines;

namespace Wyam.Common.IO
{
    // Initially based on DirectoryPath in Cake.Core.IO (http://cakebuild.net/)
    /// <summary>
    /// Represents a directory path.
    /// </summary>
    public sealed class DirectoryPath : Path
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
        /// Combines the current path with the file name of a <see cref="FilePath"/>.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A combination of the current path and the file name of the provided <see cref="FilePath"/>.</returns>
        public FilePath GetFilePath(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            return new FilePath(System.IO.Path.Combine(FullPath, path.GetFilename().FullPath));
        }

        /// <summary>
        /// Combines the current path with a <see cref="FilePath"/>.
        /// The provided <see cref="FilePath"/> must be relative.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A combination of the current path and the provided <see cref="FilePath"/>.</returns>
        public FilePath CombineWithFilePath(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (!path.IsRelative)
            {
                throw new InvalidOperationException("Cannot combine a directory path with an absolute file path");
            }
            return new FilePath(System.IO.Path.Combine(FullPath, path.FullPath));
        }

        /// <summary>
        /// Combines the current path with another <see cref="DirectoryPath"/>.
        /// The provided <see cref="DirectoryPath"/> must be relative.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A combination of the current path and the provided <see cref="DirectoryPath"/>.</returns>
        public DirectoryPath Combine(DirectoryPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (!path.IsRelative)
            {
                throw new InvalidOperationException("Cannot combine a directory path with an absolute directory path");
            }
            return new DirectoryPath(System.IO.Path.Combine(FullPath, path.FullPath));
        }

        /// <summary>
        /// Makes the path absolute to another (absolute) path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>An absolute path.</returns>
        public DirectoryPath MakeAbsolute(DirectoryPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (path.IsRelative)
            {
                throw new ArgumentException("The provided path cannot be relative");
            }
            return IsRelative
                ? path.Combine(this).Collapse()
                : new DirectoryPath(FullPath);
        }

        /// <summary>
        /// Makes the path absolute (if relative) using the root path of the given execution context.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>An absolute path.</returns>
        public DirectoryPath MakeAbsolute(IExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return IsRelative
                ? context.RootPath.Combine(this).Collapse()
                : new DirectoryPath(FullPath);
        }

        /// <summary>
        /// Collapses a <see cref="DirectoryPath"/> containing ellipses.
        /// </summary>
        /// <returns>A collapsed <see cref="DirectoryPath"/>.</returns>
        public DirectoryPath Collapse() => new DirectoryPath(CollapsePath());

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
