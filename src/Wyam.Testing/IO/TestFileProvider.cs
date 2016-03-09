using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.IO;

namespace Wyam.Testing.IO
{
    public class TestFileProvider : IFileProvider
    {
        public List<string> Directories { get; } = new List<string>();
        public List<string> Files { get; } = new List<string>();

        public IDirectory GetDirectory(DirectoryPath path) => 
            new TestDirectory(this, path.FullPath);

        public IFile GetFile(FilePath path) =>
            new TestFile(this, path.FullPath);
    }
}
