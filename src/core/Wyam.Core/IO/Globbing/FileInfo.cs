using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Wyam.Common.IO;

namespace Wyam.Core.IO.Globbing
{
    internal class FileInfo : FileInfoBase
    {
        private readonly IFile _file;

        public FileInfo(IFile file)
        {
            _file = file ?? throw new ArgumentNullException(nameof(file));
        }

        public override string Name => _file.Path.Collapse().FileName.FullPath;

        public override string FullName => _file.Path.Collapse().FullPath;

        public override DirectoryInfoBase ParentDirectory => new DirectoryInfo(_file.Directory);
    }
}
