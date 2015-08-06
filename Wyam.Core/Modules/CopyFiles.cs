using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Core.Documents;
using Wyam.Core.Helpers;

namespace Wyam.Core.Modules
{
    // Copies files from the input path to the corresponding output path
    // Sets the same metadata as ReadFiles, but doesn't set any content
    public class CopyFiles : IModule
    {
        private readonly Func<IDocument, string> _sourcePath;
        private Func<string, string> _destinationPath;
        private SearchOption _searchOption = System.IO.SearchOption.AllDirectories;
        private Func<string, bool> _where = null;

        public CopyFiles(Func<IDocument, string> sourcePath)
        {
            if (sourcePath == null)
            {
                throw new ArgumentNullException("sourcePath");
            }

            _sourcePath = sourcePath;
        }

        public CopyFiles(string searchPattern)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }

            _sourcePath = m => searchPattern;
        }

        public CopyFiles SearchOption(SearchOption searchOption)
        {
            _searchOption = searchOption;
            return this;
        }

        public CopyFiles AllDirectories()
        {
            _searchOption = System.IO.SearchOption.AllDirectories;
            return this;
        }

        public CopyFiles TopDirectoryOnly()
        {
            _searchOption = System.IO.SearchOption.TopDirectoryOnly;
            return this;
        }

        public CopyFiles Where(Func<string, bool> predicate)
        {
            _where = predicate;
            return this;
        }

        // Input to function is the full file path (including file name), should return a full file path (including file name)
        public CopyFiles To(Func<string, string> destinationPath)
        {
            if (destinationPath == null)
            {
                throw new ArgumentNullException("destinationPath");
            }

            _destinationPath = destinationPath;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            foreach (IDocument input in inputs)
            {
                string path = _sourcePath(input);
                if (path != null)
                {
                    path = Path.Combine(context.InputFolder, path);
                    string fileRoot = Path.GetDirectoryName(path);
                    if (fileRoot != null && Directory.Exists(fileRoot))
                    {
                        foreach (string file in Directory.EnumerateFiles(fileRoot, Path.GetFileName(path), _searchOption).Where(x => _where == null || _where(x)))
                        {
                            string destination = _destinationPath == null
                                ? Path.Combine(context.OutputFolder, PathHelper.GetRelativePath(Path.GetDirectoryName(path), Path.GetDirectoryName(file)), Path.GetFileName(file)) 
                                : _destinationPath(file);
                            string destinationDirectory = Path.GetDirectoryName(destination);
                            if (!Directory.Exists(destinationDirectory))
                            {
                                Directory.CreateDirectory(destinationDirectory);
                            }
                            File.Copy(file, destination, true);
                            context.Trace.Verbose("Copied file {0} to {1}", file, destination);
                            yield return input.Clone(new Dictionary<string, object>
                            {
                                {MetadataKeys.SourceFilePath, file},
                                {MetadataKeys.DestinationFilePath, destination}
                            });
                        }
                    }
                }
            }
        }
    }
}
