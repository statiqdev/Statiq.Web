using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Core.Documents;
using Metadata = Wyam.Common.Documents.Metadata;

namespace Wyam.Core.Modules
{
    // Copies files from the input path to the corresponding output path
    // Sets the same metadata as ReadFiles, but doesn't set any content
    public class CopyFiles : IModule
    {
        private readonly DocumentConfig _sourcePath;
        private Func<string, string> _destinationPath;
        private SearchOption _searchOption = System.IO.SearchOption.AllDirectories;
        private Func<string, bool> _predicate = null;
        private string[] _withoutExtensions;

        // The delegate should return a string
        public CopyFiles(DocumentConfig sourcePath)
        {
            if (sourcePath == null)
            {
                throw new ArgumentNullException(nameof(sourcePath));
            }

            _sourcePath = sourcePath;
        }

        public CopyFiles(string searchPattern)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException(nameof(searchPattern));
            }

            _sourcePath = (x, y) => searchPattern;
        }

        public CopyFiles WithSearchOption(SearchOption searchOption)
        {
            _searchOption = searchOption;
            return this;
        }

        public CopyFiles FromAllDirectories()
        {
            _searchOption = System.IO.SearchOption.AllDirectories;
            return this;
        }

        public CopyFiles FromTopDirectoryOnly()
        {
            _searchOption = System.IO.SearchOption.TopDirectoryOnly;
            return this;
        }

        public CopyFiles Where(Func<string, bool> predicate)
        {
            Func<string, bool> currentPredicate = _predicate;
            _predicate = currentPredicate == null ? predicate : x => currentPredicate(x) && predicate(x);
            return this;
        }

        public CopyFiles WithoutExtensions(params string[] withoutExtensions)
        {
            _withoutExtensions = withoutExtensions.Select(x => x.StartsWith(".") ? x : "." + x).ToArray();
            return this;
        }

        // Input to function is the full file path (including file name), should return a full file path (including file name)
        public CopyFiles To(Func<string, string> destinationPath)
        {
            if (destinationPath == null)
            {
                throw new ArgumentNullException(nameof(destinationPath));
            }

            _destinationPath = destinationPath;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().SelectMany(input =>
            {
                string path = _sourcePath.Invoke<string>(input, context);
                if (path != null)
                {
                    path = Path.Combine(context.InputFolder, path);
                    string fileRoot = Path.GetDirectoryName(path);
                    if (fileRoot != null && Directory.Exists(fileRoot))
                    {
                        return Directory.EnumerateFiles(fileRoot, Path.GetFileName(path), _searchOption)
                            .AsParallel()
                            .Where(x => (_predicate == null || _predicate(x)) && (_withoutExtensions == null || !_withoutExtensions.Contains(Path.GetExtension(x))))
                            .Select(file =>
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
                                return input.Clone(new []
                                {
                                    Metadata.Create(MetadataKeys.SourceFilePath, file),
                                    Metadata.Create(MetadataKeys.DestinationFilePath, destination)
                                });
                            });
                    }
                }
                return null;
            });
        }
    }
}
