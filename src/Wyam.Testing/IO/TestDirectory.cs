using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wyam.Common.IO;

namespace Wyam.Testing.IO
{
    public class TestDirectory : IDirectory
    {
        private readonly TestFileProvider _fileProvider;
        private readonly string _path;

        public TestDirectory(TestFileProvider fileProvider, string path)
        {
            _fileProvider = fileProvider;
            _path = path;
        }

        public DirectoryPath Path => new DirectoryPath(_path);

        NormalizedPath IFileSystemEntry.Path => Path;

        public bool Exists => _fileProvider.Directories.Contains(_path);

        public IDirectory Parent
        {
            get
            {
                string parent = System.IO.Path.GetDirectoryName(_path);
                return parent == null ? null : new TestDirectory(_fileProvider, parent);
            }
        }

        public void Create()
        {
            throw new NotImplementedException();
        }

        public void Delete(bool recursive)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IDirectory> GetDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                return _fileProvider.Directories
                    .Where(x => x.StartsWith(_path) && _path.Count(c => c == '/') == x.Count(c => c == '/') - 1)
                    .Select(x => new TestDirectory(_fileProvider, x));
            }
            return _fileProvider.Directories
                .Where(x => x.StartsWith(_path) && _path != x)
                .Select(x => new TestDirectory(_fileProvider, x));
        }

        public IEnumerable<IFile> GetFiles(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                return _fileProvider.Files
                    .Where(x => x.StartsWith(_path) && _path.Count(c => c == '/') == x.Count(c => c == '/') - 1)
                    .Select(x => new TestFile(_fileProvider, x));
            }
            return _fileProvider.Files
                .Where(x => x.StartsWith(_path))
                .Select(x => new TestFile(_fileProvider, x));
        }

        public IFile GetFile(FilePath path) => 
            new TestFile(_fileProvider, System.IO.Path.Combine(_path, path.FullPath));
    }
}