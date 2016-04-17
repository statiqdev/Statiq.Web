using System.IO;
using Wyam.Common.IO;

namespace Wyam.Common.Configuration
{
    public interface IAssemblyCollection
    {
        IAssemblyCollection LoadDirectory(DirectoryPath path, SearchOption searchOption = SearchOption.AllDirectories);
        IAssemblyCollection LoadFile(FilePath path);
        IAssemblyCollection Load(string name);
    }
}
