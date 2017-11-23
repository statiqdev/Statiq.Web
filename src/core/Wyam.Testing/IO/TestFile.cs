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
        private readonly FilePath _path;

        public TestFile(TestFileProvider fileProvider, FilePath path)
        {
            _fileProvider = fileProvider;
            _path = path.Collapse();
        }

        public FilePath Path => _path;

        NormalizedPath IFileSystemEntry.Path => Path;

        public bool Exists => _fileProvider.Files.ContainsKey(_path.FullPath);

        public IDirectory Directory => new TestDirectory(_fileProvider, _path.Directory);

        public long Length => _fileProvider.Files[_path.FullPath].Length;

        private void CreateDirectory(bool createDirectory, IFile file)
        {
            if (!createDirectory && !_fileProvider.Directories.Contains(file.Path.Directory.Collapse().FullPath))
            {
                throw new IOException($"Directory {file.Path.Directory.FullPath} does not exist");
            }
            if (createDirectory)
            {
                DirectoryPath parent = file.Path.Directory.Collapse();
                while (parent != null)
                {
                    _fileProvider.Directories.Add(parent.FullPath);
                    parent = parent.Parent;
                }
            }
        }

        public void CopyTo(IFile destination, bool overwrite = true, bool createDirectory = true)
        {
            CreateDirectory(createDirectory, destination);

            if (overwrite)
            {
                _fileProvider.Files[destination.Path.FullPath] = new StringBuilder(_fileProvider.Files[_path.FullPath].ToString());
            }
            else
            {
                _fileProvider.Files.TryAdd(destination.Path.FullPath, new StringBuilder(_fileProvider.Files[_path.FullPath].ToString()));
            }
        }

        public void MoveTo(IFile destination)
        {
            if (!_fileProvider.Files.ContainsKey(_path.FullPath))
            {
                throw new FileNotFoundException();
            }
            StringBuilder builder;
            if (_fileProvider.Files.TryRemove(_path.FullPath, out builder))
            {
                _fileProvider.Files.TryAdd(destination.Path.FullPath, builder);
            }
        }

        public void Delete()
        {
            StringBuilder builder;
            _fileProvider.Files.TryRemove(_path.FullPath, out builder);
        }

        public string ReadAllText()
        {
            return _fileProvider.Files[_path.FullPath].ToString();
        }

        public void WriteAllText(string contents, bool createDirectory = true)
        {
            CreateDirectory(createDirectory, this);
            _fileProvider.Files[_path.FullPath] = new StringBuilder(contents);
        }

        public Stream OpenRead()
        {
            StringBuilder builder;
            if (!_fileProvider.Files.TryGetValue(_path.FullPath, out builder))
            {
                throw new FileNotFoundException();
            }
            byte[] bytes = Encoding.UTF8.GetBytes(builder.ToString());
            return new MemoryStream(bytes);
        }

        public Stream OpenWrite(bool createDirectory = true)
        {
            CreateDirectory(createDirectory, this);
            return new StringBuilderStream(_fileProvider.Files.AddOrUpdate(_path.FullPath, new StringBuilder(), (x, y) => y));
        }

        public Stream OpenAppend(bool createDirectory = true)
        {
            CreateDirectory(createDirectory, this);
            StringBuilderStream stream = new StringBuilderStream(_fileProvider.Files.AddOrUpdate(_path.FullPath, new StringBuilder(), (x, y) => y));

            // Start appending at the end of the stream.
            stream.Position = stream.Length;
            return stream;
        }

        public Stream Open(bool createDirectory = true)
        {
            CreateDirectory(createDirectory, this);
            return new StringBuilderStream(_fileProvider.Files.AddOrUpdate(_path.FullPath, new StringBuilder(), (x, y) => y));
        }
    }
}