using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.IO;

namespace Wyam.Core.Configuration
{
    internal class AssemblyCollection : IAssemblyCollection
    {
        public List<Tuple<DirectoryPath, SearchOption>> Directories { get; } = new List<Tuple<DirectoryPath, SearchOption>>();
        public List<FilePath> ByFile { get; } = new List<FilePath>();
        public List<string> ByName { get; } = new List<string>();

        public IAssemblyCollection LoadDirectory(DirectoryPath path, SearchOption searchOption = SearchOption.AllDirectories)
        {
            Directories.Add(new Tuple<DirectoryPath, SearchOption>(path, searchOption));
            return this;
        }

        public IAssemblyCollection LoadFile(FilePath path)
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
