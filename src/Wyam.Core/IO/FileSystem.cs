using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.IO;
using Wyam.Core.IO.FileProviders.Local;
using Wyam.Core.IO.Globbing;

namespace Wyam.Core.IO
{
    // Initially based on code from Cake (http://cakebuild.net/)
    internal class FileSystem : IFileSystem
    {
        private DirectoryPath _rootPath = System.IO.Directory.GetCurrentDirectory();
        private DirectoryPath _outputPath = "output";

        public FileSystem()
        {
            FileProviders = new FileProviderCollection(new LocalFileProvider());
            InputPaths = new PathCollection<DirectoryPath>(new[]
            {
                new DirectoryPath("theme"), 
                new DirectoryPath("input")
            });
        }
        
        public IFileProviderCollection FileProviders { get; }

        IReadOnlyFileProviderCollection IReadOnlyFileSystem.FileProviders => FileProviders;
        
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

        public PathCollection<DirectoryPath> InputPaths { get; }

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

        public IFile GetInputFile(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.IsRelative)
            {
                IFile notFound = null;
                foreach (DirectoryPath inputPath in InputPaths.Reverse())
                {
                    IFile file = GetFile(RootPath.Combine(inputPath).CombineFile(path));
                    if (notFound == null)
                    {
                        notFound = file;
                    }
                    if (file.Exists)
                    {
                        return file;
                    }
                }
                if (notFound == null)
                {
                    throw new InvalidOperationException("The input paths collection must have at least one path");
                }
                return notFound;
            }
            return GetFile(path);
        }

        public IEnumerable<IFile> GetInputFiles(params string[] patterns)
        {
            return GetInputFiles((IEnumerable<string>)patterns);
        }

        public IEnumerable<IFile> GetInputFiles(IEnumerable<string> patterns)
        {
            return GetFiles(GetInputDirectory(), patterns);
        }

        public IDirectory GetInputDirectory(DirectoryPath path = null) => 
            path == null
                ? new VirtualInputDirectory(this, ".")
                : (path.IsRelative ? new VirtualInputDirectory(this, path) : GetDirectory(path));

        public IReadOnlyList<IDirectory> GetInputDirectories() =>
            InputPaths.Select(GetRootDirectory).ToImmutableArray();

        public DirectoryPath GetContainingInputPath(NormalizedPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (path.IsAbsolute)
            {
                return InputPaths
                    .Reverse()
                    .Select(x => RootPath.Combine(x))
                    .FirstOrDefault(x => x.FileProvider == path.FileProvider 
                        && path.FullPath.StartsWith(x.Collapse().FullPath));
            }
            FilePath filePath = path as FilePath;
            if (filePath != null)
            {
                IFile file = GetInputFile(filePath);
                return file.Exists ? GetContainingInputPath(file.Path) : null;
            }
            DirectoryPath directoryPath = path as DirectoryPath;
            if (directoryPath != null)
            {
                return InputPaths
                    .Reverse()
                    .Select(x => new KeyValuePair<DirectoryPath, IDirectory>(x, GetRootDirectory(x.Combine(directoryPath))))
                    .Where(x => x.Value.Exists)
                    .Select(x => RootPath.Combine(x.Key))
                    .FirstOrDefault();
            }
            return null;
        }

        public FilePath GetOutputPath(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return RootPath.Combine(OutputPath).CombineFile(path);
        }

        public DirectoryPath GetOutputPath(DirectoryPath path = null) =>
            path == null
                ? RootPath.Combine(OutputPath)
                : RootPath.Combine(OutputPath).Combine(path);

        public IFile GetOutputFile(FilePath path) => 
            GetFile(GetOutputPath(path));

        public IDirectory GetOutputDirectory(DirectoryPath path = null) =>
            GetDirectory(GetOutputPath(path));
        
        public IFile GetRootFile(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return GetFile(RootPath.CombineFile(path));
        }

        public IDirectory GetRootDirectory(DirectoryPath path = null) => 
            path == null 
            ? GetDirectory(RootPath) 
            : GetDirectory(RootPath.Combine(path));
        
        public IFile GetFile(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return GetFileProvider(path).GetFile(path);
        }

        public IDirectory GetDirectory(DirectoryPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return GetFileProvider(path).GetDirectory(path);
        }

        public IEnumerable<IFile> GetFiles(params string[] patterns) =>
            GetFiles(GetRootDirectory(), patterns);

        public IEnumerable<IFile> GetFiles(IEnumerable<string> patterns) =>
            GetFiles(GetRootDirectory(), patterns);

        public IEnumerable<IFile> GetFiles(IDirectory directory, params string[] patterns) => 
            GetFiles(directory, (IEnumerable<string>) patterns);

        public IEnumerable<IFile> GetFiles(IDirectory directory, IEnumerable<string> patterns)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            return patterns
                .Where(x => x != null)
                .Select(x => x.Replace("\\", "/"))  // Normalize slashes in patterns
                .Select(x =>
                {
                    bool negated = x[0] == '!';
                    FilePath filePath = negated ? new FilePath(x.Substring(1)) : new FilePath(x);
                    if (filePath.IsAbsolute)
                    {
                        // The globber doesn't support absolute paths, so get the root directory of this path (including provider)
                        IDirectory rootDirectory = GetDirectory(filePath.Root);
                        FilePath relativeFilePath = filePath.RootRelative.Collapse();
                        return Tuple.Create(rootDirectory,
                            negated ? ('!' + relativeFilePath.FullPath) : relativeFilePath.FullPath);
                    }
                    return Tuple.Create(directory, x);
                })
                .GroupBy(x => x.Item1, x => x.Item2, new DirectoryEqualityComparer())
                .SelectMany(x => Globber.GetFiles(x.Key, x));
        }

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
            if (path.FileProvider == null)
            {
                throw new ArgumentException("The path has no provider");
            }
            IFileProvider fileProvider;
            if (!FileProviders.TryGet(path.FileProvider.Scheme, out fileProvider))
            {
                throw new KeyNotFoundException($"A provider for the scheme {path.FileProvider} could not be found");
            }
            return fileProvider;
        }
    }
}
