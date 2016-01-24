using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam.Core.IO
{
    // Initially based on code from Cake (http://cakebuild.net/)
    internal sealed class FileSystem : IConfigurableFileSystem
    {
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

        public IDirectoryPathCollection InputPaths { get; } = new DirectoryPathCollection { "input" };

        IReadOnlyList<DirectoryPath> IFileSystem.InputPaths => InputPaths;

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
                new File(RootPath.CombineDirectory(inputPath).CombineFile(path).Collapse())) : new File(path);

        public IDirectory GetInputDirectory(DirectoryPath path) =>
            path.IsRelative ? GetInput(inputPath => 
                new Directory(RootPath.CombineDirectory(inputPath).CombineDirectory(path).Collapse())) : new Directory(path);

        private T GetInput<T>(Func<DirectoryPath, T> factory) where T : IFileSystemInfo
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
            new File(RootPath.CombineDirectory(OutputPath).CombineFile(path).Collapse());

        public IDirectory GetOutputDirectory(DirectoryPath path) =>
            new Directory(RootPath.CombineDirectory(OutputPath).CombineDirectory(path).Collapse());

        public IFile GetRootFile(FilePath path) =>
            new File(RootPath.CombineFile(path).Collapse());

        public IDirectory GetRootDirectory(DirectoryPath path) =>
            new Directory(RootPath.CombineDirectory(path).Collapse());

        public IFile GetFile(FilePath path)
        {
            if (path.IsRelative)
            {
                throw new ArgumentException("The path must be absolute");
            }
            return new File(path.Collapse());
        }

        public IDirectory GetDirectory(DirectoryPath path)
        {
            if (path.IsRelative)
            {
                throw new ArgumentException("The path must be absolute");
            }
            return new Directory(path.Collapse());
        }

        // *** Retry logic (used by File and Directory)

        private static readonly TimeSpan InitialInterval = TimeSpan.FromMilliseconds(200);
        private static readonly TimeSpan IntervalDelta = TimeSpan.FromMilliseconds(200);
        
        private const int RetryCount = 3;

        public static T Retry<T>(Func<T> func)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    TimeSpan? interval = ShouldRetry(retryCount, ex);
                    if (!interval.HasValue)
                    {
                        throw;
                    }
                    Thread.Sleep(interval.Value);
                }
                retryCount++;
            }
        }

        public static void Retry(Action action)
        {
            Retry<object>(() =>
            {
                action();
                return null;
            });
        }

        private static TimeSpan? ShouldRetry(int retryCount, Exception exception) =>
            (exception is IOException || exception is UnauthorizedAccessException) && retryCount < RetryCount
                ? (TimeSpan?)InitialInterval.Add(TimeSpan.FromMilliseconds(IntervalDelta.TotalMilliseconds * retryCount)) : null;
    }
}
