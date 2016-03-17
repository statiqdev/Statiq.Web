using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;
using Wyam.Common.Util;

namespace Wyam.Testing.IO
{
    public class TestFileProvider : IFileProvider
    {
        public ConcurrentHashSet<string> Directories { get; } = new ConcurrentHashSet<string>();
        public ConcurrentDictionary<string, StringBuilder> Files { get; } = new ConcurrentDictionary<string, StringBuilder>();

        public IDirectory GetDirectory(DirectoryPath path) => 
            new TestDirectory(this, path.Collapse().FullPath);

        public IFile GetFile(FilePath path) =>
            new TestFile(this, path.Collapse().FullPath);

        public void AddDirectory(string path) => Directories.Add(path);

        public void AddFile(string path, string content = "") => Files[path] = new StringBuilder(content);
    }
}
