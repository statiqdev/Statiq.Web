using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam.Testing.IO
{
    /// <summary>
    /// A file system for testing that uses a single file provider.
    /// </summary>
    public class TestFileSystem : IFileSystem
    {
        /// <summary>
        /// The file provider to use for this file system.
        /// </summary>
        public TestFileProvider FileProvider { get; set; } = new TestFileProvider();

        /// <inheritdoc />
        public IFileProviderCollection FileProviders
        {
            get { throw new NotImplementedException(); }
        }

        IReadOnlyFileProviderCollection IReadOnlyFileSystem.FileProviders => FileProviders;

        /// <inheritdoc />
        public DirectoryPath RootPath { get; set; } = new DirectoryPath("/");

        /// <inheritdoc />
        public PathCollection<DirectoryPath> InputPaths { get; set; } = new PathCollection<DirectoryPath>(new[]
        {
            new DirectoryPath("theme"),
            new DirectoryPath("input")
        });

        IReadOnlyList<DirectoryPath> IReadOnlyFileSystem.InputPaths => InputPaths;

        /// <inheritdoc />
        public DirectoryPath OutputPath { get; set; } = "output";

        /// <inheritdoc />
        public DirectoryPath TempPath { get; set; } = "temp";

        /// <inheritdoc />
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

        /// <inheritdoc />
        public IEnumerable<IFile> GetInputFiles(params string[] patterns)
        {
            return GetInputFiles((IEnumerable<string>)patterns);
        }

        /// <inheritdoc />
        public IEnumerable<IFile> GetInputFiles(IEnumerable<string> patterns)
        {
            return GetFiles(GetInputDirectory(), patterns);
        }

        /// <inheritdoc />
        public IDirectory GetInputDirectory(DirectoryPath path = null) =>
            path == null
                ? new TestDirectory(FileProvider, ".")
                : (path.IsRelative ? new TestDirectory(FileProvider, path) : GetDirectory(path));

        /// <inheritdoc />
        public IReadOnlyList<IDirectory> GetInputDirectories() =>
            InputPaths.Select(GetRootDirectory).ToImmutableArray();

        /// <inheritdoc />
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
                        && (path.FullPath == x.Collapse().FullPath || path.FullPath.StartsWith(x.Collapse().FullPath + "/")));
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

        /// <inheritdoc />
        public FilePath GetOutputPath(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return RootPath.Combine(OutputPath).CombineFile(path);
        }

        /// <inheritdoc />
        public DirectoryPath GetOutputPath(DirectoryPath path = null) =>
            path == null
                ? RootPath.Combine(OutputPath)
                : RootPath.Combine(OutputPath).Combine(path);

        /// <inheritdoc />
        public IFile GetOutputFile(FilePath path) =>
            GetFile(GetOutputPath(path));

        /// <inheritdoc />
        public IDirectory GetOutputDirectory(DirectoryPath path = null) =>
            GetDirectory(GetOutputPath(path));

        /// <inheritdoc />
        public FilePath GetTempPath(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return RootPath.Combine(TempPath).CombineFile(path);
        }

        /// <inheritdoc />
        public DirectoryPath GetTempPath(DirectoryPath path = null) =>
            path == null
                ? RootPath.Combine(TempPath)
                : RootPath.Combine(TempPath).Combine(path);

        /// <inheritdoc />
        public IFile GetTempFile(FilePath path) =>
            GetFile(GetTempPath(path));

        /// <inheritdoc />
        public IFile GetTempFile() => GetTempFile(System.IO.Path.ChangeExtension(System.IO.Path.GetRandomFileName(), "tmp"));

        /// <inheritdoc />
        public IDirectory GetTempDirectory(DirectoryPath path = null) =>
            GetDirectory(GetTempPath(path));

        /// <inheritdoc />
        public IFile GetRootFile(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return GetFile(RootPath.CombineFile(path));
        }

        /// <inheritdoc />
        public IDirectory GetRootDirectory(DirectoryPath path = null) =>
            path == null
            ? GetDirectory(RootPath)
            : GetDirectory(RootPath.Combine(path));

        /// <inheritdoc />
        public IFile GetFile(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return GetFileProvider(path).GetFile(path);
        }

        /// <inheritdoc />
        public IDirectory GetDirectory(DirectoryPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return GetFileProvider(path).GetDirectory(path);
        }

        /// <inheritdoc />
        public IEnumerable<IFile> GetFiles(params string[] patterns) =>
            GetFiles(GetRootDirectory(), patterns);

        /// <inheritdoc />
        public IEnumerable<IFile> GetFiles(IEnumerable<string> patterns) =>
            GetFiles(GetRootDirectory(), patterns);

        /// <inheritdoc />
        public IEnumerable<IFile> GetFiles(IDirectory directory, params string[] patterns) =>
            GetFiles(directory, (IEnumerable<string>)patterns);

        /// <inheritdoc />
        public IEnumerable<IFile> GetFiles(IDirectory directory, IEnumerable<string> patterns) => Array.Empty<IFile>();

        /// <inheritdoc />
        public IFileProvider GetFileProvider(NormalizedPath path) => FileProvider;
    }
}
