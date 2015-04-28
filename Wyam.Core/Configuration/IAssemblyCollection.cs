using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Core.Configuration
{
    public interface IAssemblyCollection
    {
        IAssemblyCollection AddDirectory(string path, SearchOption searchOption = SearchOption.AllDirectories);
        IAssemblyCollection AddFrom(string path);
        IAssemblyCollection Add(string name);
    }
}
