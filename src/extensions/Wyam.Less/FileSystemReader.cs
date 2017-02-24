using dotless.Core.Input;
using Wyam.Common.IO;

namespace Wyam.Less
{
    internal class FileSystemReader : IFileReader
    {
        private readonly IReadOnlyFileSystem _fileSystem;

        public FileSystemReader(IReadOnlyFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public byte[] GetBinaryFileContents(string fileName)
        {
            IFile file = _fileSystem.GetInputFile(fileName);
            using (var stream = file.OpenRead())
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int)stream.Length);
                return buffer;
            }
        }

        public string GetFileContents(string fileName) =>
            _fileSystem.GetInputFile(fileName).ReadAllText();

        public bool DoesFileExist(string fileName) =>
            _fileSystem.GetInputFile(fileName).Exists;

        public bool UseCacheDependencies => true;
    }
}