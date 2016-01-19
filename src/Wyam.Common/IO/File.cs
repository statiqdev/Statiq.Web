using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.IO
{
    // Initially based on code from Cake (http://cakebuild.net/)
    internal sealed class File : IFile
    {
        private readonly FileInfo _file;
        private readonly FilePath _path;

        public FilePath Path => _path;

        Path IFileSystemInfo.Path => _path;

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
            FileSystemRetry.Retry(() => _file.CopyTo(destination.FullPath, overwrite));
        }

        public void Move(FilePath destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            FileSystemRetry.Retry(() => _file.MoveTo(destination.FullPath));
        }

        public void Delete() => FileSystemRetry.Retry(() => _file.Delete());

        public Stream Open(FileMode fileMode, FileAccess fileAccess, FileShare fileShare) => 
            FileSystemRetry.Retry(() => _file.Open(fileMode, fileAccess, fileShare));
    }
}
