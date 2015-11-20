using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Core.Documents;
using Wyam.Core.Modules.Control;
using Metadata = Wyam.Common.Documents.Metadata;

namespace Wyam.Core.Modules.IO
{
    /// <summary>
    /// Reads the content of files from the file system into the content of new documents.
    /// </summary>
    /// <remarks>
    /// For each output document, several metadata values are set with information about the file. Note 
    /// that this module is best at the beginning of a pipeline because it will be executed once 
    /// for each input document, even if you only specify a search path. If you want to add 
    /// additional files to a current pipeline, you should enclose your ReadFiles modules with <see cref="Concat"/>.
    /// </remarks>
    /// <metadata name="SourceFileRoot" type="string">The absolute root search path without any nested directories 
    /// (I.e., the path that was searched, and possibly descended, for the given pattern).</metadata>
    /// <metadata name="SourceFilePath" type="string">The full absolute path of the file (including file name).</metadata>
    /// <metadata name="SourceFilePathBase" type="string">The full absolute path of the file (including file name) 
    /// without the file extension.</metadata>
    /// <metadata name="SourceFileBase" type="string">The file name without any extension. Equivalent 
    /// to <c>Path.GetFileNameWithoutExtension(SourceFilePath)</c>.</metadata>
    /// <metadata name="SourceFileExt" type="string">The extension of the file. Equivalent 
    /// to <c>Path.GetExtension(SourceFilePath)</c>.</metadata>
    /// <metadata name="SourceFileName" type="string">The full file name. Equivalent 
    /// to <c>Path.GetFileName(SourceFilePath)</c>.</metadata>
    /// <metadata name="SourceFileDir" type="string">The full absolute directory of the file. 
    /// Equivalent to <c>Path.GetDirectoryName(SourceFilePath).</c></metadata>
    /// <metadata name="RelativeFilePath" type="string">The relative path to the file (including file name)
    /// from the Wyam input folder.</metadata>
    /// <metadata name="RelativeFilePathBase" type="string">The relative path to the file (including file name)
    /// from the Wyam input folder without the file extension.</metadata>
    /// <metadata name="RelativeFileDir" type="string">The relative directory of the file 
    /// from the Wyam input folder.</metadata>
    /// <category>Input/Output</category>
    public class ReadFiles : IModule
    {
        private readonly DocumentConfig _path;
        private SearchOption _searchOption = System.IO.SearchOption.AllDirectories;
        private Func<string, bool> _predicate = null;
        private string[] _extensions;

        /// <summary>
        /// Reads all files that match the specified path. This allows you to specify different search paths depending on the input.
        /// </summary>
        /// <param name="path">A delegate that returns a <c>string</c> with the search path.</param>
        public ReadFiles(DocumentConfig path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            _path = path;
        }

        /// <summary>
        /// Reads all files that match the specified search pattern.
        /// </summary>
        /// <param name="searchPattern">The search pattern to use.</param>
        public ReadFiles(string searchPattern)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException(nameof(searchPattern));
            }

            _path = (x, y) => searchPattern;
        }

        /// <summary>
        /// Specifies whether to search all directories or just the top directory.
        /// </summary>
        /// <param name="searchOption">The search option to use.</param>
        public ReadFiles WithSearchOption(SearchOption searchOption)
        {
            _searchOption = searchOption;
            return this;
        }

        /// <summary>
        /// Specifies that all directories should be searched.
        /// </summary>
        public ReadFiles FromAllDirectories()
        {
            _searchOption = System.IO.SearchOption.AllDirectories;
            return this;
        }

        /// <summary>
        /// Specifies that only the top-level directory should be searched.
        /// </summary>
        public ReadFiles FromTopDirectoryOnly()
        {
            _searchOption = System.IO.SearchOption.TopDirectoryOnly;
            return this;
        }

        /// <summary>
        /// Specifies a predicate that must be satisfied for the file to be 
        /// copied. The input to the predicate is the full path to the source file.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the file should be copied.</param>
        public ReadFiles Where(Func<string, bool> predicate)
        {
            Func<string, bool> currentPredicate = _predicate;
            _predicate = currentPredicate == null ? predicate : x => currentPredicate(x) && predicate(x);
            return this;
        }

        /// <summary>
        /// Specifies that only files with the given extensions should be read.
        /// </summary>
        /// <param name="extensions">The extensions to include.</param>
        /// <returns></returns>
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
                                    Common.Documents.Metadata.Create(MetadataKeys.SourceFileRoot, fileRoot),
                                    Common.Documents.Metadata.Create(MetadataKeys.SourceFileBase, Path.GetFileNameWithoutExtension(file)),
                                    Common.Documents.Metadata.Create(MetadataKeys.SourceFileExt, Path.GetExtension(file)),
                                    Common.Documents.Metadata.Create(MetadataKeys.SourceFileName, Path.GetFileName(file)),
                                    Common.Documents.Metadata.Create(MetadataKeys.SourceFileDir, Path.GetDirectoryName(file)),
                                    Common.Documents.Metadata.Create(MetadataKeys.SourceFilePath, file),
                                    Common.Documents.Metadata.Create(MetadataKeys.SourceFilePathBase, PathHelper.RemoveExtension(file)),
                                    Common.Documents.Metadata.Create(MetadataKeys.RelativeFilePath, PathHelper.GetRelativePath(context.InputFolder, file)),
                                    Common.Documents.Metadata.Create(MetadataKeys.RelativeFilePathBase, PathHelper.RemoveExtension(PathHelper.GetRelativePath(context.InputFolder, file))),
                                    Common.Documents.Metadata.Create(MetadataKeys.RelativeFileDir, Path.GetDirectoryName(PathHelper.GetRelativePath(context.InputFolder, file)))
                                });
                            });
                    }
                }
                return (IEnumerable<IDocument>) Array.Empty<IDocument>();
            });
        }
    }
}
