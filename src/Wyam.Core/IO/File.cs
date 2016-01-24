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
    internal sealed class File : IFile
    {
        private readonly FileInfo _file;
        private readonly FilePath _path;

        public FilePath Path => _path;

        Wyam.Common.IO.Path IFileSystemInfo.Path => _path;

        public IDirectory Directory => new Directory(_path.GetDirectory());

        public bool Exists => _file.Exists;

        public bool Hidden => (_file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;

        public long Length => _file.Length;

        public File(FilePath path)
        {
            _path = path;
            _file = new FileInfo(path.FullPath);
        }

        public void Copy(FilePath destination, bool overwrite)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            FileSystem.Retry(() => _file.CopyTo(destination.FullPath, overwrite));
        }

        public void Move(FilePath destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            FileSystem.Retry(() => _file.MoveTo(destination.FullPath));
        }

        public void Delete() => FileSystem.Retry(() => _file.Delete());

        public string ReadAllText() =>
            FileSystem.Retry(() => System.IO.File.ReadAllText(_file.FullName));

        public Stream Open(FileMode fileMode) =>
            FileSystem.Retry(() => _file.Open(fileMode));

        public Stream Open(FileMode fileMode, FileAccess fileAccess, FileShare fileShare) => 
            FileSystem.Retry(() => _file.Open(fileMode, fileAccess, fileShare));
    }
}
