using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Common.Tracing;
using Wyam.Core.Documents;
using Wyam.Core.Meta;
using Wyam.Core.Modules.Control;

namespace Wyam.Core.Modules.IO
{
    /// <summary>
    /// Reads the content of files from the file system into the content of new documents.
    /// </summary>
    /// <remarks>
    /// For each output document, several metadata values are set with information about the file. This module will
    /// be executed once and input documents will be ignored if a search path is specified. Otherwise, if a delegate
    /// is specified the module will be executed once per input document and the resulting output documents will be
    /// aggregated. In either case, the input documents will not be returned as output of this module. If you want to add 
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
    public class ReadFiles : IModule, IAsNewDocuments
    {
        private readonly string _searchPattern;
        private readonly DocumentConfig _pathDelegate;
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

            _pathDelegate = path;
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

            _searchPattern = searchPattern;
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
            return _searchPattern != null
                ? Execute(null, _searchPattern, context)
                : inputs.AsParallel().SelectMany(input =>
                    Execute(input, _pathDelegate.Invoke<string>(input, context), context));
        }

        private IEnumerable<IDocument> Execute(IDocument input, string path, IExecutionContext context)
        {
            if (path != null)
            {
                path = System.IO.Path.Combine(context.InputFolder, PathHelper.NormalizePath(path));
                path = System.IO.Path.Combine(System.IO.Path.GetFullPath(System.IO.Path.GetDirectoryName(path)), System.IO.Path.GetFileName(path));
                string fileRoot = System.IO.Path.GetDirectoryName(path);
                if (fileRoot != null && Directory.Exists(fileRoot))
                {
                    return Directory.EnumerateFiles(fileRoot, System.IO.Path.GetFileName(path), _searchOption)
                        .AsParallel()
                        .Where(x => (_predicate == null || _predicate(x)) && (_extensions == null || _extensions.Contains(System.IO.Path.GetExtension(x))))
                        .Select(file =>
                        {
                            Trace.Verbose("Read file {0}", file);
                            string relativePath = PathHelper.GetRelativePath(context.InputFolder, file);
                            return context.GetDocument(input, file, SafeIOHelper.OpenRead(file), new MetadataItems
                            {
                                    { Keys.SourceFileRoot, fileRoot },
                                    { Keys.SourceFileBase, System.IO.Path.GetFileNameWithoutExtension(file) },
                                    { Keys.SourceFileExt, System.IO.Path.GetExtension(file) },
                                    { Keys.SourceFileName, System.IO.Path.GetFileName(file) },
                                    { Keys.SourceFileDir, System.IO.Path.GetDirectoryName(file) },
                                    { Keys.SourceFilePath, file },
                                    { Keys.SourceFilePathBase, PathHelper.RemoveExtension(file) },
                                    { Keys.RelativeFilePath, relativePath },
                                    { Keys.RelativeFilePathBase, PathHelper.RemoveExtension(relativePath) },
                                    { Keys.RelativeFileDir, System.IO.Path.GetDirectoryName(relativePath) }
                            });
                        });
                }
            }
            return Array.Empty<IDocument>();
        } 
    }
}
