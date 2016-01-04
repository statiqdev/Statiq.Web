using System.IO;

namespace Wyam.Common.Configuration
{
    public interface IAssemblyCollection
    {
        IAssemblyCollection LoadDirectory(string path, SearchOption searchOption = SearchOption.AllDirectories);
        IAssemblyCollection LoadFile(string path);
        IAssemblyCollection Load(string name);
    }
}
