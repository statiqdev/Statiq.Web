using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wyam.Common.IO;

namespace Wyam.Core.IO
{
    // Initially based on code from Cake (http://cakebuild.net/)
    internal sealed class Directory : IDirectory
    {
        private readonly DirectoryInfo _directory;
        private readonly DirectoryPath _path;

        public DirectoryPath Path => _path;

        Wyam.Common.IO.Path IFileSystemInfo.Path => _path;

        public bool Exists => _directory.Exists;

        public bool Hidden => (_directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;

        public Directory(DirectoryPath path)
        {
            _path = path;
            _directory = new DirectoryInfo(_path.FullPath);
        }

        public void Create() => FileSystem.Retry(() => _directory.Create());

        public void Delete(bool recursive) => FileSystem.Retry(() => _directory.Delete(recursive));

        public IEnumerable<IDirectory> GetDirectories(string filter, SearchOption searchOption = SearchOption.TopDirectoryOnly) => 
            FileSystem.Retry(() => _directory.GetDirectories(filter, searchOption).Select(directory => new Directory(directory.FullName)));

        public IEnumerable<IFile> GetFiles(string filter, SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            FileSystem.Retry(() => _directory.GetFiles(filter, searchOption).Select(file => new File(file.FullName)));

        public IFile GetFile(FilePath path)
        {
            return new File(_path.CombineFile(path).Collapse());
        }
    }
}
