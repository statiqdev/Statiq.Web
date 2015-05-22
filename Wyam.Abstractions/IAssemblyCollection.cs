using System.IO;

namespace Wyam.Abstractions
{
    public interface IAssemblyCollection
    {
        IAssemblyCollection AddDirectory(string path, SearchOption searchOption = SearchOption.AllDirectories);
        IAssemblyCollection AddFrom(string path);
        IAssemblyCollection Add(string name);
    }
}
