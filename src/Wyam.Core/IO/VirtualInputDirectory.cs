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

        public void Create()
        {
            throw new NotSupportedException("Can not create a virtual input directory");
        }

        public void Delete(bool recursive)
        {
            throw new NotSupportedException("Can not delete a virtual input directory");
        }

        public IEnumerable<IDirectory> GetDirectories(string filter, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // Get all the relative child directories
            HashSet<DirectoryPath> directories = new HashSet<DirectoryPath>();
            foreach (IDirectory directory in GetDirectories())
            {
                foreach (IDirectory childDirectory in directory.GetDirectories(filter, searchOption))
                {
                    directories.Add(directory.Path.GetRelativePath(childDirectory.Path));
                }
            }

            // Return a new virtual directory for each one
            return directories.Select(x => new VirtualInputDirectory(_fileSystem, x));
        }

        public IEnumerable<IFile> GetFiles(string filter, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // Get all the files for each input directory, replacing earlier ones with later ones
            Dictionary<FilePath, IFile> files = new Dictionary<FilePath, IFile>();
            foreach (IDirectory directory in GetDirectories())
            {
                foreach (IFile file in directory.GetFiles(filter, searchOption))
                {
                    files[directory.Path.GetRelativePath(file.Path)] = file;
                }
            }
            return files.Values;
        }

        public IFile GetFile(FilePath path)
        {
            return _fileSystem.GetInputFile(_path.CombineFile(path));
        }

        /// <summary>
        /// Gets a value indicating whether any of the input paths contain this directory.
        /// </summary>
        /// <value>
        /// <c>true</c> if this directory exists at one of the input paths; otherwise, <c>false</c>.
        /// </value>
        public bool Exists => GetDirectories().Any();

        private IEnumerable<IDirectory> GetDirectories() => 
            _fileSystem.InputPaths.Select(x => _fileSystem.GetRootDirectory(x.Combine(_path)));
    }
}
