using System;
using System.IO;
using System.Threading;
using Wyam.Common.IO;

namespace Wyam.Core.IO.FileProviders.Local
{
    public class LocalFileProvider : IFileProvider
    {
        public IFile GetFile(FilePath path) => new LocalFile(path);
        public IDirectory GetDirectory(DirectoryPath path) => new LocalDirectory(path);
    }
}