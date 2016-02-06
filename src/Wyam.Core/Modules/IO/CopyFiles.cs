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

namespace Wyam.Core.Modules.IO
{
    /// <summary>
    /// Copies the content of files from the file system from one location on disk to another location.
    /// </summary>
    /// <remarks>
    /// For each output document, several metadata values are set with information about the file. 
    /// By default, files are copied from the input folder (or a subfolder) to the same relative 
    /// location in the output folder, but this doesn't have to be the case. The output of this module are documents
    /// with metadata representing the files copied by the module. Note that the input documents are not output by this
    /// module.
    /// </remarks>
    /// <metadata name="SourceFilePath" type="string">The full path (including file name) of the source file.</metadata>
    /// <metadata name="DestinationFilePath" type="string">The full path (including file name) of the destination file.</metadata>
    /// <category>Input/Output</category>
    public class CopyFiles : IModule
    {
        private readonly DocumentConfig _sourcePathDelegate;
        private readonly string _searchPattern;
        private Func<string, string> _destinationPath;
        private SearchOption _searchOption = System.IO.SearchOption.AllDirectories;
        private Func<string, bool> _predicate = null;
        private string[] _withoutExtensions;

        /// <summary>
        /// Copies all files that match the specified path. This allows you to specify different search paths depending on the input document.
        /// When this constructor is used, the module is evaluated once for every input document, which may result in copying the same file
        /// more than once (and may also result in IO conflicts since copying is typically done in parallel). It is recommended you only
        /// specify a function-based source path if there will be no overlap between the path returned from each input document.
        /// </summary>
        /// <param name="sourcePath">A delegate that returns a <c>string</c> with the desired search path.</param>
        public CopyFiles(DocumentConfig sourcePath)
        {
            if (sourcePath == null)
            {
                throw new ArgumentNullException(nameof(sourcePath));
            }

            _sourcePathDelegate = sourcePath;
        }

        /// <summary>
        /// Copies all files that match the specified search pattern. When this constructor is used, the module is evaluated only once against
        /// an empty input document. This makes it possible to string multiple CopyFiles modules together in one pipeline. Keep in mind that the
        /// result of the pipeline in this case will be documents representing the files copied only by the last CopyFiles module in the pipeline
        /// (since the output documents of the previous CopyFiles modules will have been consumed by the last one).
        /// </summary>
        /// <param name="searchPattern">The search pattern to use.</param>
        public CopyFiles(string searchPattern)
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
        public CopyFiles WithSearchOption(SearchOption searchOption)
        {
            _searchOption = searchOption;
            return this;
        }

        /// <summary>
        /// Specifies that all directories should be searched.
        /// </summary>
        public CopyFiles FromAllDirectories()
        {
            _searchOption = System.IO.SearchOption.AllDirectories;
            return this;
        }

        /// <summary>
        /// Specifies that only the top-level directory should be searched.
        /// </summary>
        public CopyFiles FromTopDirectoryOnly()
        {
            _searchOption = System.IO.SearchOption.TopDirectoryOnly;
            return this;
        }

        /// <summary>
        /// Specifies a predicate that must be satisfied for the file to be 
        /// copied. The input to the predicate is the full path to the source file.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the file should be copied.</param>
        public CopyFiles Where(Func<string, bool> predicate)
        {
            Func<string, bool> currentPredicate = _predicate;
            _predicate = currentPredicate == null ? predicate : x => currentPredicate(x) && predicate(x);
            return this;
        }

        /// <summary>
        /// Specifies that files with the given extensions should not be copied. This allows you to exclude certain
        /// file type from the copy operation.
        /// </summary>
        /// <param name="withoutExtensions">The extensions to exclude.</param>
        /// <returns></returns>
        public CopyFiles WithoutExtensions(params string[] withoutExtensions)
        {
            _withoutExtensions = withoutExtensions.Select(x => x.StartsWith(".") ? x : "." + x).ToArray();
            return this;
        }

        /// <summary>
        /// Specifies an alternate destination path for each file (by default files are copied to their 
        /// same relative path in the output directory). The input to the function is the full source 
        /// file path (including file name) and the output should be the full file path (including 
        /// file name) of the destination file. If the delegate returns <c>null</c> for a particular
        /// file, that file will not be copied.
        /// </summary>
        /// <param name="destinationPath">A delegate that specifies an alternate destination.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
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
            return _searchPattern != null
                ? Execute(null, _searchPattern, context)
                : inputs.AsParallel().SelectMany(input => 
                    Execute(input, _sourcePathDelegate.Invoke<string>(input, context), context));
        }

        private IEnumerable<IDocument> Execute(IDocument input, string path, IExecutionContext context)
        {
            if (path != null)
            {
                bool isPathUnderInputFolder = false;

                if (!System.IO.Path.IsPathRooted(path))
                {
                    path = PathHelper.CombineToFullPath(context.InputFolder, path);
                    isPathUnderInputFolder = path.StartsWith(context.InputFolder);
                }

                string fileRoot = System.IO.Path.GetDirectoryName(path);
                if (fileRoot != null && Directory.Exists(fileRoot))
                {
                    return Directory.EnumerateFiles(fileRoot, System.IO.Path.GetFileName(path), _searchOption)
                        .AsParallel()
                        .Where(x => (_predicate == null || _predicate(x)) && (_withoutExtensions == null || !_withoutExtensions.Contains(System.IO.Path.GetExtension(x))))
                        .Select(file =>
                        {
                            string destination = null;

                            if (_destinationPath == null)
                            {
                                if (file != null)
                                {
                                    string relativePath = isPathUnderInputFolder ? PathHelper.GetRelativePath(context.InputFolder, System.IO.Path.GetDirectoryName(file)) : "";
                                    destination = System.IO.Path.Combine(context.OutputFolder, relativePath, System.IO.Path.GetFileName(file));
                                }
                            }
                            else
                            {
                                destination = _destinationPath(file);
                            }

                            if (!string.IsNullOrWhiteSpace(destination))
                            {
                                string destinationDirectory = System.IO.Path.GetDirectoryName(destination);
                                if (destinationDirectory != null && !Directory.Exists(destinationDirectory))
                                {
                                    Directory.CreateDirectory(destinationDirectory);
                                }
                                SafeIOHelper.Copy(file, destination, true);
                                Trace.Verbose("Copied file {0} to {1}", file, destination);
                                return context.GetDocument(input, new MetadataItems
                                {
                                        { Keys.SourceFilePath, file },
                                        { Keys.DestinationFilePath, destination }
                                });
                            }

                            return null;
                        })
                        .Where(x => x != null);
                }
            }
            return Array.Empty<IDocument>();
        } 
    }
}
