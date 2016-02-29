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
        new bool IsCaseSensitive { get; set; }
        new DirectoryPath RootPath { get; set; }
        new PathCollection<DirectoryPath> InputPaths { get; }
        new DirectoryPath OutputPath { get; set; }
    }
}
