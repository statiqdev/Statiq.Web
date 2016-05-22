using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wyam.Common.IO;

namespace Wyam.Core.IO.FileProviders.FileProvider
{
    // Initially based on code from Cake (http://cakebuild.net/)
    internal class FileDirectory : IDirectory
    {
        private readonly DirectoryInfo _directory;
        private readonly DirectoryPath _path;

        public DirectoryPath Path => _path;

        NormalizedPath IFileSystemEntry.Path => _path;

        public bool Exists => _directory.Exists;

        public FileDirectory(DirectoryPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (path.IsRelative)
            {
                throw new ArgumentException("Path must be absolute", nameof(path));
            }

            _path = path;
            _directory = new DirectoryInfo(_path.Collapse().FullPath);
        }

        public void Create() => File.Retry(() => _directory.Create());

        public void Delete(bool recursive) => File.Retry(() => _directory.Delete(recursive));

        public IEnumerable<IDirectory> GetDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            File.Retry(() => _directory.GetDirectories("*", searchOption).Select(directory => new FileDirectory(directory.FullName)));

        public IEnumerable<IFile> GetFiles(SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            File.Retry(() => _directory.GetFiles("*", searchOption).Select(file => new FileFile(file.FullName)));

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

            return new FileDirectory(_path.Combine(path));
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

            return new FileFile(_path.CombineFile(path));
        }
    }
}
