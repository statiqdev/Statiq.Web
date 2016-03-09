using System;
using System.IO;
using Wyam.Common.IO;

namespace Wyam.Testing.IO
{
    public class TestFile : IFile
    {
        private readonly TestFileProvider _fileProvider;
        private readonly string _path;

        public TestFile(TestFileProvider fileProvider, string path)
        {
            _fileProvider = fileProvider;
            _path = path;
        }

        public FilePath Path => new FilePath(_path);

        NormalizedPath IFileSystemEntry.Path => Path;

        public bool Exists => _fileProvider.Files.Contains(_path);

        public IDirectory Directory => new TestDirectory(_fileProvider, System.IO.Path.GetDirectoryName(_path));

        public long Length
        {
            get { throw new NotImplementedException(); }
        }

        public void Copy(FilePath destination, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public void Move(FilePath destination)
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public string ReadAllText()
        {
            throw new NotImplementedException();
        }

        public Stream OpenRead()
        {
            throw new NotImplementedException();
        }

        public Stream OpenWrite()
        {
            throw new NotImplementedException();
        }

        public Stream Open(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            throw new NotImplementedException();
        }
    }
}