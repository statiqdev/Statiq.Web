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
        private readonly List<Tuple<string, SearchOption>> _directories = new List<Tuple<string, SearchOption>>(); 
        private readonly List<string> _byFile = new List<string>();
        private readonly List<string> _byName = new List<string>();

        public IAssemblyCollection LoadDirectory(string path, SearchOption searchOption = SearchOption.AllDirectories)
        {
            _directories.Add(new Tuple<string, SearchOption>(path, searchOption));
            return this;
        }

        public IAssemblyCollection LoadFile(string path)
        {
            _byFile.Add(path);
            return this;
        }

        public IAssemblyCollection Load(string name)
        {
            _byName.Add(name);
            return this;
        }

        public List<Tuple<string, SearchOption>> Directories
        {
            get { return _directories; }
        }

        public List<string> ByFile
        {
            get { return _byFile; }
        }

        public List<string> ByName
        {
            get { return _byName; }
        }
    }
}
