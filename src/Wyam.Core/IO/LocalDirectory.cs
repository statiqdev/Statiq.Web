using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam.Core.IO
{
    // Initially based on code from Cake (http://cakebuild.net/)
    internal class LocalDirectory : IDirectory
    {
        private readonly DirectoryInfo _directory;
        private readonly DirectoryPath _path;

        public DirectoryPath Path => _path;

        NormalizedPath IFileSystemEntry.Path => _path;

        public IDirectory Parent
        {
            get
            {
                DirectoryInfo parent = _directory.Parent;
                return parent == null ? null : new LocalDirectory(parent.FullName);
            }
        }

        public bool Exists => _directory.Exists;

        public LocalDirectory(DirectoryPath path)
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
            _directory = new DirectoryInfo(_path.FullPath);
        }

        public void Create() => LocalFileProvider.Retry(() => _directory.Create());

        public void Delete(bool recursive) => LocalFileProvider.Retry(() => _directory.Delete(recursive));

        public IEnumerable<IDirectory> GetDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            LocalFileProvider.Retry(() => _directory.GetDirectories("*", searchOption).Select(directory => new LocalDirectory(directory.FullName)));

        public IEnumerable<IFile> GetFiles(SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            LocalFileProvider.Retry(() => _directory.GetFiles("*", searchOption).Select(file => new LocalFile(file.FullName)));

        public IFile GetFile(FilePath path)
        {
            return new LocalFile(_path.CombineFile(path).Collapse());
        }
    }
}
