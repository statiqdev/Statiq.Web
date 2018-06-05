using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;

namespace Wyam.Common.IO
{
    /// <summary>
    /// Represents a file path.
    /// </summary>
    public sealed class FilePath : NormalizedPath
    {
        // Initially based on code from Cake (http://cakebuild.net/)

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePath"/> class.
        /// The path will be considered absolute if the underlying OS file system
        /// considers it absolute.
        /// </summary>
        /// <param name="path">The path.</param>
        public FilePath(string path)
            : base(path, PathKind.RelativeOrAbsolute)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePath"/> class..
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="pathKind">Specifies whether the path is relative, absolute, or indeterminate.</param>
        public FilePath(string path, PathKind pathKind)
            : base(path, pathKind)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePath" /> class
        /// with the specified file provider.
        /// The path will be considered absolute if the underlying OS file system
        /// considers it absolute.
        /// </summary>
        /// <param name="fileProvider">The file provider.</param>
        /// <param name="path">The path.</param>
        public FilePath(string fileProvider, string path)
            : base(fileProvider, path, PathKind.RelativeOrAbsolute)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePath" /> class
        /// with the specified file provider.
        /// </summary>
        /// <param name="fileProvider">The file provider.</param>
        /// <param name="path">The path.</param>
        /// <param name="pathKind">Specifies whether the path is relative, absolute, or indeterminate.</param>
        public FilePath(string fileProvider, string path, PathKind pathKind)
            : base(fileProvider, path, pathKind)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePath" /> class
        /// with the specified file provider.
        /// The path will be considered absolute if the underlying OS file system
        /// considers it absolute.
        /// </summary>
        /// <param name="fileProvider">The file provider.</param>
        /// <param name="path">The path.</param>
        public FilePath(Uri fileProvider, string path)
            : base(fileProvider, path, PathKind.RelativeOrAbsolute)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePath" /> class
        /// with the specified file provider.
        /// </summary>
        /// <param name="fileProvider">The file provider.</param>
        /// <param name="path">The path.</param>
        /// <param name="pathKind">Specifies whether the path is relative, absolute, or indeterminate.</param>
        public FilePath(Uri fileProvider, string path, PathKind pathKind)
            : base(fileProvider, path, pathKind)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePath" /> class
        /// with the specified file provider and/or path.
        /// </summary>
        /// <param name="path">The path (and file provider if this is an absolute URI).</param>
        public FilePath(Uri path)
            : base(path)
        {
        }

        /// <summary>
        /// Gets a value indicating whether this path has a file extension.
        /// </summary>
        /// <value>
        /// <c>true</c> if this file path has a file extension; otherwise, <c>false</c>.
        /// </value>
        public bool HasExtension => System.IO.Path.HasExtension(FullPath);

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
                return new DirectoryPath(FileProvider, directory);
            }
        }

        /// <summary>
        /// Gets the file path relative to it's root path.
        /// </summary>
        public FilePath RootRelative
        {
            get
            {
                if (!IsAbsolute)
                {
                    return this;
                }
                DirectoryPath root = Root;
                return root.FullPath == "."
                    ? this
                    : new FilePath(FullPath.Substring(root.FullPath.Length), PathKind.Relative);
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
        /// <value>The filename without it's extension, or <c>null</c> if the file has no name.</value>
        public FilePath FileNameWithoutExtension
        {
            get
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(FullPath);
                return string.IsNullOrEmpty(fileName) ? null : new FilePath(System.IO.Path.GetFileNameWithoutExtension(FullPath));
            }
        }

        /// <summary>
        /// Gets the file extension (including the preceding ".").
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
            new FilePath(FileProvider, System.IO.Path.ChangeExtension(FullPath, extension));

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
            return new FilePath(FileProvider, string.Concat(FullPath, extension));
        }

        /// <summary>
        /// Inserts a suffix into the file name before the extension.
        /// </summary>
        /// <param name="suffix">The suffix to insert.</param>
        /// <returns>A new <see cref="FilePath"/> with the specified suffix.</returns>
        public FilePath InsertSuffix(string suffix)
        {
            if (suffix == null)
            {
                throw new ArgumentNullException(nameof(suffix));
            }

            int extensionIndex = FullPath.LastIndexOf(".");
            if (extensionIndex == -1)
            {
                return new FilePath(FileProvider, string.Concat(FullPath, suffix));
            }
            return new FilePath(FileProvider, string.Concat(FullPath.Substring(0, extensionIndex), suffix, FullPath.Substring(extensionIndex)));
        }

        /// <summary>
        /// Inserts a prefix into the file name.
        /// </summary>
        /// <param name="prefix">The prefix to insert.</param>
        /// <returns>A new <see cref="FilePath"/> with the specified prefix.</returns>
        public FilePath InsertPrefix(string prefix)
        {
            if (prefix == null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            int nameIndex = FullPath.LastIndexOf("/");
            if (nameIndex == -1)
            {
                return new FilePath(FileProvider, string.Concat(prefix, FullPath));
            }
            return new FilePath(FileProvider, string.Concat(FullPath.Substring(0, nameIndex + 1), prefix, FullPath.Substring(nameIndex + 1)));
        }

        /// <summary>
        /// Collapses a <see cref="FilePath"/> containing ellipses.
        /// </summary>
        /// <returns>A collapsed <see cref="FilePath"/>.</returns>
        public FilePath Collapse() => new FilePath(FileProvider, Collapse(this));

        /// <summary>
        /// Performs an implicit conversion from <see cref="string"/> to <see cref="FilePath"/>.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A <see cref="FilePath"/>.</returns>
        public static implicit operator FilePath(string path)
        {
            return FromString(path);
        }

        /// <summary>
        /// Performs a conversion from <see cref="string"/> to <see cref="FilePath"/>.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A <see cref="FilePath"/>.</returns>
        public static FilePath FromString(string path) =>
            path == null ? null : new FilePath(path);

        /// <summary>
        /// Performs an implicit conversion from <see cref="Uri"/> to <see cref="FilePath"/>.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A <see cref="FilePath"/>.</returns>
        public static implicit operator FilePath(Uri path) => FromUri(path);

        /// <summary>
        /// Performs a conversion from <see cref="Uri"/> to <see cref="FilePath"/>.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A <see cref="FilePath"/>.</returns>
        public static FilePath FromUri(Uri path) =>
            path == null ? null : new FilePath(path);
    }
}
