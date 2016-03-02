using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam.Core.IO
{
    // Initially based on code from Cake (http://cakebuild.net/)
    internal class FileSystem : IFileSystem
    {
        public bool IsCaseSensitive { get; set; }

        // TODO: Add a new default LocalProvider
        public IFileProviderCollection FileProviders => new FileProviderCollection(new LocalFileProvider());

        IReadOnlyFileProviderCollection IReadOnlyFileSystem.FileProviders => FileProviders;

        public PathComparer PathComparer { get; private set; }

        private DirectoryPath _rootPath = System.IO.Directory.GetCurrentDirectory();
        private DirectoryPath _outputPath = "output";
        
        public DirectoryPath RootPath
        {
            get { return _rootPath; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(RootPath));
                }
                if (value.IsRelative)
                {
                    throw new ArgumentException("The root path must not be relative");
                }
                _rootPath = value;
            }
        }

        public PathCollection<DirectoryPath> InputPaths { get; private set; }

        IReadOnlyList<DirectoryPath> IReadOnlyFileSystem.InputPaths => InputPaths;

        public DirectoryPath OutputPath
        {
            get { return _outputPath; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(OutputPath));
                }
                _outputPath = value;
            }
        }

        public IFile GetInputFile(FilePath path) =>
            path.IsRelative ? GetInput(inputPath => 
                GetFile(RootPath.Combine(inputPath).CombineFile(path).Collapse())) : GetFile(path);

        public IDirectory GetInputDirectory(DirectoryPath path) =>
            path.IsRelative ? GetInput(inputPath => 
                GetDirectory(RootPath.Combine(inputPath).Combine(path).Collapse())) : GetDirectory(path);

        public IReadOnlyList<IDirectory> GetInputDirectories() =>
            InputPaths.Select(GetRootDirectory).ToImmutableArray();

        private T GetInput<T>(Func<DirectoryPath, T> factory) where T : IFileSystemEntry
        {
            T notFound = default(T);
            foreach (DirectoryPath inputPath in InputPaths.Reverse())
            {
                T info = factory(inputPath);
                if (notFound == null)
                {
                    notFound = info;
                }
                if (info.Exists)
                {
                    return info;
                }
            }
            if (notFound == null)
            {
                throw new InvalidOperationException("The input paths collection must have at least one path");
            }
            return notFound;
        }

        public IFile GetOutputFile(FilePath path) =>
            GetFile(RootPath.Combine(OutputPath).CombineFile(path).Collapse());

        public IDirectory GetOutputDirectory(DirectoryPath path) =>
            GetDirectory(RootPath.Combine(OutputPath).Combine(path).Collapse());

        public IDirectory GetOutputDirectory() =>
            GetRootDirectory(OutputPath);

        public IFile GetRootFile(FilePath path) =>
            GetFile(RootPath.CombineFile(path).Collapse());

        public IDirectory GetRootDirectory(DirectoryPath path) =>
            GetDirectory(RootPath.Combine(path).Collapse());

        public IDirectory GetRootDirectory() =>
            GetDirectory(RootPath.Collapse());

        public IFile GetFile(FilePath path) => 
            GetFileProvider(path).GetFile(path.Collapse());

        public IDirectory GetDirectory(DirectoryPath path) =>
            GetFileProvider(path).GetDirectory(path.Collapse());

        public IFileProvider GetFileProvider(NormalizedPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (path.IsRelative)
            {
                throw new ArgumentException("The path must be absolute");
            }
            IFileProvider fileProvider;
            if (!FileProviders.TryGet(path.Provider, out fileProvider))
            {
                throw new ArgumentException($"Provider {path.Provider} could not be found", nameof(path));
            }
            return fileProvider;
        }

        public FileSystem()
        {
            PathComparer = new PathComparer(this);
            InputPaths = new PathCollection<DirectoryPath>(
                new[]
                {
                    DirectoryPath.FromString("input")
                }, PathComparer);
        }
    }
}
