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
    internal class LocalFile : IFile
    {
        private readonly FileInfo _file;
        private readonly FilePath _path;

        public FilePath Path => _path;

        NormalizedPath IFileSystemEntry.Path => _path;

        public IDirectory Directory => new LocalDirectory(_path.GetDirectory());

        public bool Exists => _file.Exists;

        public bool Hidden => (_file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;

        public long Length => _file.Length;

        public LocalFile(FilePath path)
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
            _file = new FileInfo(path.FullPath);
        }

        public void Copy(FilePath destination, bool overwrite)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            if (destination.IsRelative)
            {
                throw new ArgumentException("Destination must be absolute", nameof(destination));
            }

            LocalFileProvider.Retry(() => _file.CopyTo(destination.FullPath, overwrite));
        }

        public void Move(FilePath destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            if (destination.IsRelative)
            {
                throw new ArgumentException("Destination must be absolute", nameof(destination));
            }

            LocalFileProvider.Retry(() => _file.MoveTo(destination.FullPath));
        }

        public void Delete() => LocalFileProvider.Retry(() => _file.Delete());

        public string ReadAllText() =>
            LocalFileProvider.Retry(() => File.ReadAllText(_file.FullName));

        public Stream OpenRead() =>
            LocalFileProvider.Retry(() => _file.OpenRead());

        public Stream OpenWrite() =>
            LocalFileProvider.Retry(() => _file.OpenWrite());
        
        public Stream Open(FileMode fileMode, FileAccess fileAccess, FileShare fileShare) =>
            LocalFileProvider.Retry(() => _file.Open(fileMode, fileAccess, fileShare));
    }
}
