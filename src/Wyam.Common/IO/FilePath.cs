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
    /// Represents a file path.
    /// </summary>
    public sealed class FilePath : NormalizedPath
    {
        /// <summary>
        /// Gets a value indicating whether this path has a file extension.
        /// </summary>
        /// <value>
        /// <c>true</c> if this file path has a file extension; otherwise, <c>false</c>.
        /// </value>
        public bool HasExtension => System.IO.Path.HasExtension(FullPath);

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePath"/> class.
        /// The path will be considered absolute if the underlying OS file system
        /// considers it absolute.
        /// </summary>
        /// <param name="path">The path.</param>
        public FilePath(string path)
            : base(path, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePath"/> class..
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="absolute">Explicitly sets this path as absolute (or not).</param>
        public FilePath(string path, bool absolute)
            : base(path, absolute)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePath" /> class
        /// with the specified provider.
        /// The path will be considered absolute if the underlying OS file system
        /// considers it absolute.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="path">The path.</param>
        public FilePath(string provider, string path)
            : base(provider, path, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePath" /> class
        /// with the specified provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="path">The path.</param>
        /// <param name="absolute">Explicitly sets this path as absolute (or not).</param>
        public FilePath(string provider, string path, bool absolute)
            : base(provider, path, absolute)
        {
        }

        /// <summary>
        /// Gets the directory part of the path.
        /// </summary>
        /// <value>The directory part of the path.</value>
        public DirectoryPath Directory
        {
            get
            {
                string directory = System.IO.Path.GetDirectoryName(FullPath);
                if (string.IsNullOrWhiteSpace(directory))
                {
                    directory = ".";
                }
                return new DirectoryPath(Provider, directory);
            }
        }

        /// <summary>
        /// Gets the filename.
        /// </summary>
        /// <value>The filename.</value>
        public FilePath FileName => 
            new FilePath(System.IO.Path.GetFileName(FullPath));

        /// <summary>
        /// Gets the filename without it's extension.
        /// </summary>
        /// <value>The filename without it's extension.</value>
        public FilePath FileNameWithoutExtension => 
            new FilePath(System.IO.Path.GetFileNameWithoutExtension(FullPath));

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        /// <value>The file extension (including the preceding ".").</value>
        public string Extension
        {
            get
            {
                string extension = System.IO.Path.GetExtension(FullPath);
                return string.IsNullOrWhiteSpace(extension) ? null : extension;
            }
        }

        /// <summary>
        /// Changes the file extension of the path.
        /// </summary>
        /// <param name="extension">The new extension.</param>
        /// <returns>A new <see cref="FilePath"/> with a new extension.</returns>
        public FilePath ChangeExtension(string extension) => 
            new FilePath(Provider, System.IO.Path.ChangeExtension(FullPath, extension));

        /// <summary>
        /// Appends a file extension to the path.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <returns>A new <see cref="FilePath"/> with an appended extension.</returns>
        public FilePath AppendExtension(string extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }
            if (!extension.StartsWith(".", StringComparison.OrdinalIgnoreCase))
            {
                extension = string.Concat(".", extension);
            }
            return new FilePath(Provider, string.Concat(FullPath, extension));
        }

        /// <summary>
        /// Collapses a <see cref="FilePath"/> containing ellipses.
        /// </summary>
        /// <returns>A collapsed <see cref="FilePath"/>.</returns>
        public FilePath Collapse() => new FilePath(Provider, Collapse(this));

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="FilePath"/>.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A <see cref="FilePath"/>.</returns>
        public static implicit operator FilePath(string path)
        {
            return FromString(path);
        }

        /// <summary>
        /// Performs a conversion from <see cref="System.String"/> to <see cref="FilePath"/>.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A <see cref="FilePath"/>.</returns>
        public static FilePath FromString(string path) => 
            string.IsNullOrWhiteSpace(path) ? null : new FilePath(path);
    }
}
