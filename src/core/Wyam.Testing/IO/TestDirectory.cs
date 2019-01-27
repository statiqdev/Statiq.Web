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
        private readonly DirectoryPath _path;

        public TestDirectory(TestFileProvider fileProvider, DirectoryPath path)
        {
            _fileProvider = fileProvider;
            _path = path.Collapse();
        }

        public DirectoryPath Path => _path;

        NormalizedPath IFileSystemEntry.Path => Path;

        public bool Exists => _fileProvider.Directories.Contains(_path.FullPath);

        public IDirectory Parent
        {
            get
            {
                DirectoryPath parentPath = _path.Parent;
                if (parentPath == null)
                {
                    return null;
                }
                return new TestDirectory(_fileProvider, parentPath);
            }
        }

        public bool IsCaseSensitive => true;

        public void Create() => _fileProvider.Directories.Add(_path.FullPath);

        public void Delete(bool recursive) => _fileProvider.Directories.Remove(_path.FullPath);

        public IEnumerable<IDirectory> GetDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                string adjustedPath = _path.FullPath.EndsWith("/", StringComparison.Ordinal)
                    ? _path.FullPath.Substring(0, _path.FullPath.Length - 1)
                    : _path.FullPath;
                return _fileProvider.Directories
                    .Where(x => x.StartsWith(adjustedPath + "/")
                        && adjustedPath.Count(c => c == '/') == x.Count(c => c == '/') - 1
                        && _path.FullPath != x)
                    .Select(x => new TestDirectory(_fileProvider, x));
            }
            return _fileProvider.Directories
                .Where(x => x.StartsWith(_path.FullPath + "/") && _path.FullPath != x)
                .Select(x => new TestDirectory(_fileProvider, x));
        }

        public IEnumerable<IFile> GetFiles(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                string adjustedPath = _path.FullPath.EndsWith("/", StringComparison.Ordinal)
                    ? _path.FullPath.Substring(0, _path.FullPath.Length - 1)
                    : _path.FullPath;
                return _fileProvider.Files.Keys
                    .Where(x => x.StartsWith(adjustedPath)
                        && adjustedPath.Count(c => c == '/') == x.Count(c => c == '/') - 1)
                    .Select(x => new TestFile(_fileProvider, x));
            }
            return _fileProvider.Files.Keys
                .Where(x => x.StartsWith(_path.FullPath))
                .Select(x => new TestFile(_fileProvider, x));
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

            return new TestDirectory(_fileProvider, _path.Combine(path));
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

            return new TestFile(_fileProvider, _path.CombineFile(path));
        }
    }
}