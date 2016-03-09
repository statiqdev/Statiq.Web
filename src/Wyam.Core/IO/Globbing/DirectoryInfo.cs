using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Wyam.Common.IO;

namespace Wyam.Core.IO.Globbing
{
    //internal class DirectoryInfo : DirectoryInfoBase
    //{
    //    private readonly IDirectory _directory;
    //    private readonly bool _isParentPath;

    //    public DirectoryInfo(IDirectory directory, bool isParentPath = false)
    //    {
    //        _directory = directory;
    //        _isParentPath = isParentPath;
    //    }

    //    public override IEnumerable<FileSystemInfoBase> EnumerateFileSystemInfos()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override DirectoryInfoBase GetDirectory(string path)
    //    {
    //        if (string.Equals(path, "..", StringComparison.Ordinal))
    //        {
    //            // Get the parent directory

    //        }
    //    }

    //    public override FileInfoBase GetFile(string path)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override string Name => _isParentPath ? ".." : _directory.Path.Name;

    //    public override string FullName => _directory.Path.FullPath;

    //    public override DirectoryInfoBase ParentDirectory => 
    //}
}
