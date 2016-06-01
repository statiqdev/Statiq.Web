using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.FileProviders;
using Microsoft.Framework.Expiration.Interfaces;
using Wyam.Common.IO;
using IFileProvider = Microsoft.AspNet.FileProviders.IFileProvider;

namespace Wyam.Razor
{
    /// <summary>
    /// Looks up files using the Wyam virtual file system.
    /// </summary>
    public class WyamFileProvider : IFileProvider
    {
        private readonly IReadOnlyFileSystem _fileSystem;
        
        public WyamFileProvider(IReadOnlyFileSystem fileSystem)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }
            _fileSystem = fileSystem;
        }
        
        public IFileInfo GetFileInfo(string subpath)
        {
            if (string.IsNullOrEmpty(subpath))
            {
                return new NotFoundFileInfo(subpath);
            }
            if (subpath.StartsWith("/", StringComparison.Ordinal))
            {
                subpath = subpath.Substring(1);
            }
            IFile file = _fileSystem.GetInputFile(subpath);
            return new WyamFileInfo(file);
        }
        
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (subpath == null)
            {
                return new NotFoundDirectoryContents();
            }
            IDirectory directory = _fileSystem.GetInputDirectory(subpath);
            List<IFileInfo> fileInfos = new List<IFileInfo>();
            fileInfos.AddRange(directory.GetDirectories().Select(x => new WyamDirectoryInfo(x)));
            fileInfos.AddRange(directory.GetFiles().Select(x => new WyamFileInfo(x)));
            return new EnumerableDirectoryContents(fileInfos);
        }

        public IExpirationTrigger Watch(string filter)
        {
            throw new NotSupportedException();
        }

        private class WyamFileInfo : IFileInfo
        {
            private readonly IFile _file;

            public WyamFileInfo(IFile file)
            {
                _file = file;
            }

            public bool Exists => _file.Exists;

            public long Length => _file.Length;

            public string PhysicalPath => _file.Path.FullPath;

            public string Name => _file.Path.FileName.FullPath;

            public DateTimeOffset LastModified => DateTimeOffset.Now;

            public bool IsDirectory => false;

            public Stream CreateReadStream() => _file.OpenRead();
        }

        private class WyamDirectoryInfo : IFileInfo
        {
            private readonly IDirectory _directory;

            public WyamDirectoryInfo(IDirectory directory)
            {
                _directory = directory;
            }

            public bool Exists => _directory.Exists;

            public long Length => -1L;

            public string PhysicalPath => _directory.Path.FullPath;

            public string Name => _directory.Path.Name;

            public DateTimeOffset LastModified => DateTimeOffset.Now;

            public bool IsDirectory => true;
            
            public Stream CreateReadStream()
            {
                throw new InvalidOperationException("Cannot create a stream for a directory.");
            }
        }
    }
}
