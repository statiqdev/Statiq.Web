using System.Collections.Concurrent;
using Wyam.Common.Execution;
using Wyam.Common.IO;

namespace Wyam.Sass
{
    internal class FileImporter
    {
        // Maps each parent path to the containing path for use in nested imports
        // since the parent path may be relative in those cases
        private readonly ConcurrentDictionary<FilePath, FilePath> _parentAbsolutePaths
            = new ConcurrentDictionary<FilePath, FilePath>();

        private readonly IReadOnlyFileSystem _fileSystem;

        public FileImporter(IReadOnlyFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public bool TryImport(string requestedFile, string parentPath, out string scss, out string map)
        {
            scss = null;
            map = null;

            // Get the input relative path to the parent file
            FilePath parentFilePath = new FilePath(parentPath);
            FilePath requestedFilePath = new FilePath(requestedFile);
            if (parentFilePath.IsRelative && !_parentAbsolutePaths.TryGetValue(parentFilePath, out parentFilePath))
            {
                // Relative parent path and no available absolute path, try with the relative path
                parentFilePath = new FilePath(parentPath);
            }

            // Try to get the relative path to the parent file from inside the input virtual file system
            // But if the parent file isn't under an input path, just use it directly
            DirectoryPath containingInputPath = _fileSystem.GetContainingInputPath(parentFilePath);
            FilePath parentRelativePath = containingInputPath != null
                ? containingInputPath.GetRelativePath(parentFilePath)
                : parentFilePath;

            // Find the requested file
            // ...as specified
            FilePath filePath = parentRelativePath.Directory.CombineFile(requestedFilePath);
            if (GetFile(filePath, requestedFilePath, out scss))
            {
                return true;
            }

            // ...with extension (if not already)
            if (!filePath.HasExtension || filePath.Extension != ".scss")
            {
                FilePath extensionPath = filePath.AppendExtension(".scss");
                if (GetFile(extensionPath, requestedFilePath, out scss))
                {
                    return true;
                }

                // ...and with underscore prefix (if not already)
                if (!extensionPath.FileName.FullPath.StartsWith("_"))
                {
                    extensionPath = extensionPath.Directory.CombineFile("_" + extensionPath.FileName.FullPath);
                    if (GetFile(extensionPath, requestedFilePath, out scss))
                    {
                        return true;
                    }
                }
            }

            // ...with underscore prefix (if not already)
            if (!filePath.FileName.FullPath.StartsWith("_"))
            {
                filePath = filePath.Directory.CombineFile("_" + filePath.FileName.FullPath);
                if (GetFile(filePath, requestedFilePath, out scss))
                {
                    return true;
                }
            }

            return false;
        }

        private bool GetFile(FilePath filePath, FilePath requestedFilePath, out string scss)
        {
            scss = null;
            IFile file = _fileSystem.GetInputFile(filePath);
            if (file.Exists)
            {
                if (requestedFilePath.IsRelative)
                {
                    _parentAbsolutePaths.AddOrUpdate(requestedFilePath, file.Path, (x, y) => file.Path);
                }
                scss = file.ReadAllText();
                return true;
            }
            return false;
        }
    }
}