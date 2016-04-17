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
        /// <summary>
        /// Gets or sets the path where NuGet packages will be downloaded and cached.
        /// </summary>
        /// <value>
        /// The packages path.
        /// </value>
        DirectoryPath PackagesPath { get; set; }

        IRepository Repository(string packageSource);
    }
}
