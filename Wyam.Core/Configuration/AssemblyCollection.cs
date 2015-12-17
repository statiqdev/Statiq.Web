using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;

namespace Wyam.Core.Configuration
{
    internal class AssemblyCollection : IAssemblyCollection
    {
        public List<Tuple<string, SearchOption>> Directories { get; } = new List<Tuple<string, SearchOption>>();
        public List<string> ByFile { get; } = new List<string>();
        public List<string> ByName { get; } = new List<string>();

        public IAssemblyCollection LoadDirectory(string path, SearchOption searchOption = SearchOption.AllDirectories)
        {
            Directories.Add(new Tuple<string, SearchOption>(path, searchOption));
            return this;
        }

        public IAssemblyCollection LoadFile(string path)
        {
            ByFile.Add(path);
            return this;
        }

        public IAssemblyCollection Load(string name)
        {
            ByName.Add(name);
            return this;
        }
    }
}
