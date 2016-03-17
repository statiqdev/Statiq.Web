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
    /// be executed once and input documents will be ignored if search patterns are specified. Otherwise, if a delegate
    /// is specified, the module will be executed once per input document and the resulting output documents will be
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
        private readonly string[] _patterns;
        private readonly DocumentConfig _patternsDelegate;
        private Func<IFile, bool> _predicate = null;

        /// <summary>
        /// Reads all files that match the specified globbing patterns and/or absolute paths. This allows you to 
        /// specify different patterns and/or paths depending on the input.
        /// </summary>
        /// <param name="patterns">A delegate that returns one or more globbing patterns and/or absolute paths.</param>
        public ReadFiles(DocumentConfig patterns)
        {
            if (patterns == null)
            {
                throw new ArgumentNullException(nameof(patterns));
            }

            _patternsDelegate = patterns;
        }

        /// <summary>
        /// Reads all files that match the specified globbing patterns and/or absolute paths.
        /// </summary>
        /// <param name="patterns">The globbing patterns and/or absolute paths to read.</param>
        public ReadFiles(params string[] patterns)
        {
            if (patterns == null)
            {
                throw new ArgumentNullException(nameof(patterns));
            }
            if (patterns.Any(x => x == null))
            {
                throw new ArgumentNullException(nameof(patterns));
            }

            _patterns = patterns;
        }

        /// <summary>
        /// Specifies a predicate that must be satisfied for the file to be read.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the file should be read.</param>
        public ReadFiles Where(Func<IFile, bool> predicate)
        {
            Func<IFile, bool> currentPredicate = _predicate;
            _predicate = currentPredicate == null ? predicate : x => currentPredicate(x) && predicate(x);
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
                    .GetInputFiles(patterns.Where(p => p != null).ToArray())
                    .AsParallel()
                    .Where(x => _predicate == null || _predicate(x))
                    .Select(x =>
                    {
                        Trace.Verbose($"Read file {x.Path.FullPath}");
                        DirectoryPath inputPath = context.FileSystem.GetInputPath(x.Path);
                        FilePath relativePath = inputPath.GetRelativePath(x.Path);
                        return context.GetDocument(input, x.Path.FullPath, x.OpenRead(), new MetadataItems
                        {
                            {Keys.SourceFileRoot, inputPath.FullPath.ToString()},
                            {Keys.SourceFileBase, x.Path.FileNameWithoutExtension},
                            {Keys.SourceFileExt, x.Path.Extension},
                            {Keys.SourceFileName, x.Path.FileName},
                            {Keys.SourceFileDir, x.Path.Directory.FullPath},
                            {Keys.SourceFilePath, x.Path.FullPath},
                            {Keys.SourceFilePathBase, x.Path.Directory.CombineFile(x.Path.FileNameWithoutExtension).FullPath},
                            {Keys.RelativeFilePath, relativePath},
                            {Keys.RelativeFilePathBase, relativePath.Directory.CombineFile(x.Path.FileNameWithoutExtension).FullPath},
                            {Keys.RelativeFileDir, relativePath.Directory.FullPath}
                        });
                    });
            }
            return Array.Empty<IDocument>();
        }
    }
}
