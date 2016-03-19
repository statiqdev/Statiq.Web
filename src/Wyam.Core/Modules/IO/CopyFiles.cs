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
    /// Copies the content of files from one path on to another path.
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
    public class CopyFiles : IModule, IAsNewDocuments
    {
        private readonly DocumentConfig _patternsDelegate;
        private readonly string[] _patterns;
        private Func<IFile, FilePath> _destinationPath;
        private Func<IFile, bool> _predicate = null;

        /// <summary>
        /// Copies all files that match the specified globbing patterns and/or absolute paths. This allows you to specify different 
        /// patterns and/or paths depending on the input document.
        /// When this constructor is used, the module is evaluated once for every input document, which may result in copying the same file
        /// more than once (and may also result in IO conflicts since copying is typically done in parallel). It is recommended you only
        /// specify a function-based source path if there will be no overlap between the path returned from each input document.
        /// </summary>
        /// <param name="patterns">A delegate that returns one or more globbing patterns and/or absolute paths.</param>
        public CopyFiles(DocumentConfig patterns)
        {
            if (patterns == null)
            {
                throw new ArgumentNullException(nameof(patterns));
            }

            _patternsDelegate = patterns;
        }

        /// <summary>
        /// Copies all files that match the specified globbing patterns and/or absolute paths. When this constructor is used, the module is 
        /// evaluated only once against empty input document. This makes it possible to string multiple CopyFiles modules together in one pipeline. 
        /// Keep in mind that the result of the whole pipeline in this case will be documents representing the files copied only by the last CopyFiles 
        /// module in the pipeline (since the output documents of the previous CopyFiles modules will have been consumed by the last one).
        /// </summary>
        /// <param name="patterns">The globbing patterns and/or absolute paths to read.</param>
        public CopyFiles(params string[] patterns)
        {
            if (patterns == null)
            {
                throw new ArgumentNullException(nameof(patterns));
            }

            _patterns = patterns;
        }

        /// <summary>
        /// Specifies a predicate that must be satisfied for the file to be copied.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the file should be copied.</param>
        public CopyFiles Where(Func<IFile, bool> predicate)
        {
            Func<IFile, bool> currentPredicate = _predicate;
            _predicate = currentPredicate == null ? predicate : x => currentPredicate(x) && predicate(x);
            return this;
        }

        /// <summary>
        /// Specifies an alternate destination path for each file (by default files are copied to their 
        /// same relative path in the output directory). The output of the function should be the full 
        /// file path (including file name) of the destination file. If the delegate returns 
        /// <c>null</c> for a particular file, that file will not be copied.
        /// </summary>
        /// <param name="destinationPath">A delegate that specifies an alternate destination.</param>
        public CopyFiles To(Func<IFile, FilePath> destinationPath)
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
            return _patterns != null
                ? Execute(null, _patterns, context)
                : inputs.AsParallel().SelectMany(input => 
                    Execute(input, _patternsDelegate.Invoke<string[]>(input, context), context));
        }

        private IEnumerable<IDocument> Execute(IDocument input, string[] patterns, IExecutionContext context)
        {
            if (patterns != null)
            {
                return context.FileSystem
                    .GetInputFiles(patterns)
                    .AsParallel()
                    .Where(x => _predicate == null || _predicate(x))
                    .Select(file =>
                    {
                        // Get the destination file
                        IFile destination = null;
                        if (_destinationPath == null)
                        {
                            DirectoryPath inputPath = context.FileSystem.GetContainingInputPath(file.Path);
                            FilePath relativePath = inputPath?.GetRelativePath(file.Path) ?? file.Path.FileName;
                            destination = context.FileSystem.GetOutputFile(relativePath);
                        }
                        else
                        {
                            destination = context.FileSystem.GetOutputFile(_destinationPath(file));
                        }

                        // Copy to the destination
                        file.CopyTo(destination);
                        Trace.Verbose("Copied file {0} to {1}", file.Path.FullPath, destination.Path.FullPath);

                        // Return the document
                        return context.GetDocument(input, new MetadataItems
                        {
                            { Keys.SourceFilePath, file.Path.FullPath },
                            { Keys.DestinationFilePath, destination.Path.FullPath }
                        });
                    })
                    .Where(x => x != null);
            }
            return Array.Empty<IDocument>();
        } 
    }
}
