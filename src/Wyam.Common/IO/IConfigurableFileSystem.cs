using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Common.IO
{
    public interface IConfigurableFileSystem : IFileSystem
    {
        new DirectoryPath RootPath { get; set; }
        new IDirectoryPathCollection InputPaths { get; }
        new DirectoryPath OutputPath { get; set; }
    }
}
