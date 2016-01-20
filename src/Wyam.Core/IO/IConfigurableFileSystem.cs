using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam.Core.IO
{
    public interface IConfigurableFileSystem : IFileSystem
    {
        new DirectoryPath RootPath { get; set; }
        new IDirectoryPathCollection InputPaths { get; }
        new DirectoryPath OutputPath { get; set; }
    }
}
