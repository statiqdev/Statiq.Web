using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Core;
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
    /// <summary>
    /// Reads the content of files from the file system into the content of new documents. For each 
    /// output document, several metadata values are set with information about the file. Note 
    /// that this module is best at the beginning of a pipeline because it will be executed once 
    /// for each input document, even if you only specify a search path. If you want to add 
    /// additional files to a current pipeline, you should enclose your ReadFiles modules with Concat.
    /// </summary>
    /// <metadata name="SourceFileRoot">The absolute root search path without any nested directories 
    /// (I.e., the path that was searched, and possibly descended, for the given pattern).</metadata>
    /// <metadata name="SourceFilePath">The full absolute path of the file (including file name).</metadata>
    /// <metadata name="SourceFilePathBase">The full absolute path of the file (including file name) 
    /// without the file extension.</metadata>
    /// <metadata name="SourceFileBase">The file name without any extension. Equivalent 
    /// to <c>Path.GetFileNameWithoutExtension(SourceFilePath)</c>.</metadata>
    /// <metadata name="SourceFileExt">The extension of the file. Equivalent 
    /// to <c>Path.GetExtension(SourceFilePath)</c>.</metadata>
    /// <metadata name="SourceFileName">The full file name. Equivalent 
    /// to <c>Path.GetFileName(SourceFilePath)</c>.</metadata>
    /// <metadata name="SourceFileDir">The full absolute directory of the file. 
    /// Equivalent to <c>Path.GetDirectoryName(SourceFilePath).</c></metadata>
    /// <metadata name="RelativeFilePath">The relative path to the file (including file name)
    /// from the Wyam input folder.</metadata>
    /// <metadata name="RelativeFilePathBase">The relative path to the file (including file name)
    /// from the Wyam input folder without the file extension.</metadata>
    /// <metadata name="RelativeFileDir">The relative directory of the file 
    /// from the Wyam input folder.</metadata>
    public class ReadFiles : IModule
    {
        private readonly DocumentConfig _path;
        private SearchOption _searchOption = System.IO.SearchOption.AllDirectories;
        private Func<string, bool> _predicate = null;
        private string[] _extensions; 

        // The delegate should return a string
        public ReadFiles(DocumentConfig path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            _path = path;
        }

        public ReadFiles(string searchPattern)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException(nameof(searchPattern));
            }

            _path = (x, y) => searchPattern;
        }

        public ReadFiles WithSearchOption(SearchOption searchOption)
        {
            _searchOption = searchOption;
            return this;
        }

        public ReadFiles FromAllDirectories()
        {
            _searchOption = System.IO.SearchOption.AllDirectories;
            return this;
        }

        public ReadFiles FromTopDirectoryOnly()
        {
            _searchOption = System.IO.SearchOption.TopDirectoryOnly;
            return this;
        }

        public ReadFiles Where(Func<string, bool> predicate)
        {
            Func<string, bool> currentPredicate = _predicate;
            _predicate = currentPredicate == null ? predicate : x => currentPredicate(x) && predicate(x);
            return this;
        }

        public ReadFiles WithExtensions(params string[] extensions)
        {
            _extensions = extensions.Select(x => x.StartsWith(".") ? x : "." + x).ToArray();
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().SelectMany(input =>
            {
                string path = _path.Invoke<string>(input, context);
                if (path != null)
                {
                    path = Path.Combine(context.InputFolder, PathHelper.NormalizePath(path));
                    path = Path.Combine(Path.GetFullPath(Path.GetDirectoryName(path)), Path.GetFileName(path));
                    string fileRoot = Path.GetDirectoryName(path);
                    if (fileRoot != null && Directory.Exists(fileRoot))
                    {
                        return Directory.EnumerateFiles(fileRoot, Path.GetFileName(path), _searchOption)
                            .AsParallel()
                            .Where(x => (_predicate == null || _predicate(x)) && (_extensions == null || _extensions.Contains(Path.GetExtension(x))))
                            .Select(file =>
                            {
                                context.Trace.Verbose("Read file {0}", file);
                                return input.Clone(file, File.OpenRead(file), new []
                                {
                                    Metadata.Create(MetadataKeys.SourceFileRoot, fileRoot),
                                    Metadata.Create(MetadataKeys.SourceFileBase, Path.GetFileNameWithoutExtension(file)),
                                    Metadata.Create(MetadataKeys.SourceFileExt, Path.GetExtension(file)),
                                    Metadata.Create(MetadataKeys.SourceFileName, Path.GetFileName(file)),
                                    Metadata.Create(MetadataKeys.SourceFileDir, Path.GetDirectoryName(file)),
                                    Metadata.Create(MetadataKeys.SourceFilePath, file),
                                    Metadata.Create(MetadataKeys.SourceFilePathBase, PathHelper.RemoveExtension(file)),
                                    Metadata.Create(MetadataKeys.RelativeFilePath, PathHelper.GetRelativePath(context.InputFolder, file)),
                                    Metadata.Create(MetadataKeys.RelativeFilePathBase, PathHelper.RemoveExtension(PathHelper.GetRelativePath(context.InputFolder, file))),
                                    Metadata.Create(MetadataKeys.RelativeFileDir, Path.GetDirectoryName(PathHelper.GetRelativePath(context.InputFolder, file)))
                                });
                            });
                    }
                }
                return (IEnumerable<IDocument>) Array.Empty<IDocument>();
            });
        }
    }
}
