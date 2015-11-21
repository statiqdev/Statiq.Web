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
    /// location in the output folder, but this doesn't have to be the case. Also note that this 
    /// module is evaluated for each input document, so it's typically used as the first (and 
    /// often only) module in a pipeline. Otherwise, you would probably copy the same files multiple 
    /// times (once for each input document).
    /// </remarks>
    /// <metadata name="SourceFilePath" type="string">The full path (including file name) of the source file.</metadata>
    /// <metadata name="DestinationFilePath" type="string">The full path (including file name) of the destination file.</metadata>
    /// <category>Input/Output</category>
    public class CopyFiles : IModule
    {
        private readonly DocumentConfig _sourcePath;
        private Func<string, string> _destinationPath;
        private SearchOption _searchOption = System.IO.SearchOption.AllDirectories;
        private Func<string, bool> _predicate = null;
        private string[] _withoutExtensions;

        /// <summary>
        /// Copies all files that match the specified path. This allows you to specify different search paths depending on the input document.
        /// </summary>
        /// <param name="sourcePath">A delegate that returns a <c>string</c> with the desired search path.</param>
        public CopyFiles(DocumentConfig sourcePath)
        {
            if (sourcePath == null)
            {
                throw new ArgumentNullException(nameof(sourcePath));
            }

            _sourcePath = sourcePath;
        }

        /// <summary>
        /// Copies all files that match the specified search pattern.
        /// </summary>
        /// <param name="searchPattern">The search pattern to use.</param>
        public CopyFiles(string searchPattern)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException(nameof(searchPattern));
            }

            _sourcePath = (x, y) => searchPattern;
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
            return inputs.AsParallel().SelectMany(input =>
            {
                string path = _sourcePath.Invoke<string>(input, context);
                if (path != null)
                {
                    bool isPathUnderInputFolder = false;

                    if (!Path.IsPathRooted(path))
                    {
                        path = PathHelper.CombineToFullPath(context.InputFolder, path);
                        isPathUnderInputFolder = path.StartsWith(context.InputFolder);
                    }
                    
                    string fileRoot = Path.GetDirectoryName(path);
                    if (fileRoot != null && Directory.Exists(fileRoot))
                    {
                        return Directory.EnumerateFiles(fileRoot, Path.GetFileName(path), _searchOption)
                            .AsParallel()
                            .Where(x => (_predicate == null || _predicate(x)) && (_withoutExtensions == null || !_withoutExtensions.Contains(Path.GetExtension(x))))
                            .Select(file =>
                            {
                                string destination = null;

                                if( _destinationPath == null )
                                {
                                    string relativePath = isPathUnderInputFolder ? PathHelper.GetRelativePath(context.InputFolder, Path.GetDirectoryName(file)) : "";
                                    destination = Path.Combine(context.OutputFolder, relativePath, Path.GetFileName(file));
                                }
                                else
                                {
                                    destination = _destinationPath(file);
                                }

                                if (!string.IsNullOrWhiteSpace(destination))
                                {
                                    string destinationDirectory = Path.GetDirectoryName(destination);
                                    if (!Directory.Exists(destinationDirectory))
                                    {
                                        Directory.CreateDirectory(destinationDirectory);
                                    }
                                    File.Copy(file, destination, true);
                                    context.Trace.Verbose("Copied file {0} to {1}", file, destination);
                                    return input.Clone(new MetadataItems
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
                return null;
            });
        }
    }
}
