using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.IO
{
    public interface IFileProvider
    {
        IFile GetFile(FilePath path);
        IDirectory GetDirectory(DirectoryPath path);
    }
}
