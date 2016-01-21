using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam.Common.NuGet
{
    public interface IPackagesCollection : IRepository
    {
        // Sets the path where NuGet packages will be downloaded and cached
        DirectoryPath PackagesPath { get; set; }
        DirectoryPath ContentPath { get; set; }

        IRepository Repository(string packageSource);
    }
}
