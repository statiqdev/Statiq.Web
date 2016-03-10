using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
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

        public bool Exists => _fileProvider.Files.ContainsKey(_path);

        public IDirectory Directory => new TestDirectory(_fileProvider, System.IO.Path.GetDirectoryName(_path));

        public long Length => _fileProvider.Files[_path].Length;

        public void Copy(FilePath destination, bool overwrite)
        {
            if (overwrite)
            {
                _fileProvider.Files[destination.FullPath] = new StringBuilder(_fileProvider.Files[_path].ToString());
            }
            else
            {
                _fileProvider.Files.TryAdd(destination.FullPath, new StringBuilder(_fileProvider.Files[_path].ToString()));
            }
        }

        public void Move(FilePath destination)
        {
            if (!_fileProvider.Files.ContainsKey(_path))
            {
                throw new FileNotFoundException();
            }
            StringBuilder builder;
            if (_fileProvider.Files.TryRemove(_path, out builder))
            {
                _fileProvider.Files.TryAdd(destination.FullPath, builder);
            }
        }

        public void Delete()
        {
            StringBuilder builder;
            _fileProvider.Files.TryRemove(_path, out builder);
        }

        public string ReadAllText()
        {
            return _fileProvider.Files[_path].ToString();
        }

        public Stream OpenRead()
        {
            StringBuilder builder;
            if (!_fileProvider.Files.TryGetValue(_path, out builder))
            {
                throw new FileNotFoundException();
            }
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(builder.ToString());
                writer.Flush();
            }
            stream.Position = 0;
            return stream;
        }

        public Stream OpenWrite()
        {
            return new StringBuilderStream(_fileProvider.Files.AddOrUpdate(_path, new StringBuilder(), (x, y) => new StringBuilder()));
        }

        public Stream OpenAppend()
        {
            return new StringBuilderStream(_fileProvider.Files.AddOrUpdate(_path, new StringBuilder(), (x, y) => y));
        }
    }
}