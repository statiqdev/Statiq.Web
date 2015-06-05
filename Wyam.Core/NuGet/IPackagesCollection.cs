using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.NuGet
{
    public interface IPackagesCollection : IRepository
    {
        // Sets the path where NuGet packages will be downloaded and cached
        string Path { get; set; }

        IRepository Repository(string packageSource);
    }
}
