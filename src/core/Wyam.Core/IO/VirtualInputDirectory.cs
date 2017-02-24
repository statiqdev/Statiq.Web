using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam.Core.IO
{
    internal class VirtualInputDirectory : IDirectory
    {
        private readonly FileSystem _fileSystem;
        private readonly DirectoryPath _path;

        public VirtualInputDirectory(FileSystem fileSystem, DirectoryPath path)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (!path.IsRelative)
            {
                throw new ArgumentException("Virtual input paths should always be relative", nameof(path));
            }

            _fileSystem = fileSystem;
            _path = path;
        }

        public DirectoryPath Path => _path;

        NormalizedPath IFileSystemEntry.Path => Path;

        public IDirectory Parent
        {
            get
            {
                DirectoryPath parentPath = _path.Parent;
                if (parentPath == null)
                {
                    return null;
                }
                return new VirtualInputDirectory(_fileSystem, parentPath);
            }
        }

        public void Create()
        {
            throw new NotSupportedException("Can not create a virtual input directory");
        }

        public void Delete(bool recursive)
        {
            throw new NotSupportedException("Can not delete a virtual input directory");
        }

        // For the root (".") virtual directory, this should just return the child name,
        // but for all others it should include the child directory name
        public IEnumerable<IDirectory> GetDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // Get all the relative child directories
            HashSet<DirectoryPath> directories = new HashSet<DirectoryPath>();
            foreach (IDirectory directory in GetExistingDirectories())
            {
                foreach (IDirectory childDirectory in directory.GetDirectories(searchOption))
                {
                    directories.Add(_path.Combine(directory.Path.GetRelativePath(childDirectory.Path)));
                }
            }

            // Return a new virtual directory for each one
            return directories.Select(x => new VirtualInputDirectory(_fileSystem, x));
        }

        public IEnumerable<IFile> GetFiles(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // Get all the files for each input directory, replacing earlier ones with later ones
            Dictionary<FilePath, IFile> files = new Dictionary<FilePath, IFile>();
            foreach (IDirectory directory in GetExistingDirectories())
            {
                foreach (IFile file in directory.GetFiles(searchOption))
                {
                    files[directory.Path.GetRelativePath(file.Path)] = file;
                }
            }
            return files.Values;
        }

        public IDirectory GetDirectory(DirectoryPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (!path.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(path));
            }

            return new VirtualInputDirectory(_fileSystem, _path.Combine(path));
        }

        public IFile GetFile(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (!path.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(path));
            }

            return _fileSystem.GetInputFile(_path.CombineFile(path));
        }

        /// <summary>
        /// Gets a value indicating whether any of the input paths contain this directory.
        /// </summary>
        /// <value>
        /// <c>true</c> if this directory exists at one of the input paths; otherwise, <c>false</c>.
        /// </value>
        public bool Exists => GetExistingDirectories().Any();

        private IEnumerable<IDirectory> GetExistingDirectories() =>
            _fileSystem.InputPaths
                .Select(x => _fileSystem.GetRootDirectory(x.Combine(_path)))
                .Where(x => x.Exists);
    }
}
