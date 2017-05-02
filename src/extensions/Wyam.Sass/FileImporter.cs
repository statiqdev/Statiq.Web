using Wyam.Common.Execution;
using Wyam.Common.IO;

namespace Wyam.Sass
{
    internal class FileImporter
    {
        private readonly IExecutionContext _context;
        private readonly FilePath _inputPath;

        public FileImporter(IExecutionContext context, FilePath inputPath)
        {
            _context = context;
            _inputPath = inputPath;
        }

        public bool TryImport(string requestedFile, string parentPath, out string scss, out string map)
        {
            FilePath parentFilePath = new FilePath(_inputPath.FileProvider, parentPath);
            FilePath filePath = parentFilePath.Directory.CombineFile(new FilePath(_inputPath.FileProvider, requestedFile));
            IFile file = _context.FileSystem.GetFile(filePath);
            scss = null;
            map = null;
            if (file.Exists)
            {
                scss = file.ReadAllText();
                return true;
            }
            return false;
        }
    }
}