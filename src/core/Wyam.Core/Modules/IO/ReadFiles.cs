using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;
using Wyam.Common.Util;
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
    /// <metadata cref="Keys.SourceFileRoot" usage="Output" />
    /// <metadata cref="Keys.SourceFileBase" usage="Output" />
    /// <metadata cref="Keys.SourceFileExt" usage="Output" />
    /// <metadata cref="Keys.SourceFileName" usage="Output" />
    /// <metadata cref="Keys.SourceFileDir" usage="Output" />
    /// <metadata cref="Keys.SourceFilePath" usage="Output" />
    /// <metadata cref="Keys.SourceFilePathBase" usage="Output" />
    /// <metadata cref="Keys.RelativeFilePath" usage="Output" />
    /// <metadata cref="Keys.RelativeFilePathBase" usage="Output" />
    /// <metadata cref="Keys.RelativeFileDir" usage="Output" />
    /// <category>Input/Output</category>
    public class ReadFiles : IModule, IAsNewDocuments
    {
        private readonly ContextConfig _contextPatterns;
        private readonly DocumentConfig _documentPatterns;
        private Func<IFile, bool> _predicate = null;

        /// <summary>
        /// Reads all files that match the specified globbing patterns and/or absolute paths. This allows you to
        /// specify different patterns and/or paths depending on the context.
        /// </summary>
        /// <param name="patterns">A delegate that returns one or more globbing patterns and/or absolute paths.</param>
        public ReadFiles(ContextConfig patterns)
        {
            if (patterns == null)
            {
                throw new ArgumentNullException(nameof(patterns));
            }

            _contextPatterns = patterns;
        }

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

            _documentPatterns = patterns;
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

            _contextPatterns = _ => patterns;
        }

        /// <summary>
        /// Specifies a predicate that must be satisfied for the file to be read.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the file should be read.</param>
        /// <returns>The current module instance.</returns>
        public ReadFiles Where(Func<IFile, bool> predicate)
        {
            Func<IFile, bool> currentPredicate = _predicate;
            _predicate = currentPredicate == null ? predicate : x => currentPredicate(x) && predicate(x);
            return this;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return _contextPatterns != null
                ? Execute(null, _contextPatterns.Invoke<string[]>(context, "while getting patterns"), context)
                : inputs.AsParallel().SelectMany(context, input =>
                    Execute(input, _documentPatterns.Invoke<string[]>(input, context, "while getting patterns"), context));
        }

        private IEnumerable<IDocument> Execute(IDocument input, string[] patterns, IExecutionContext context)
        {
            if (patterns != null)
            {
                return context.FileSystem
                    .GetInputFiles(patterns)
                    .AsParallel()
                    .Where(file => _predicate == null || _predicate(file))
                    .Select(file =>
                    {
                        Trace.Verbose($"Read file {file.Path.FullPath}");
                        DirectoryPath inputPath = context.FileSystem.GetContainingInputPath(file.Path);
                        FilePath relativePath = inputPath?.GetRelativePath(file.Path) ?? file.Path.FileName;
                        FilePath fileNameWithoutExtension = file.Path.FileNameWithoutExtension;
                        return context.GetDocument(input, file.Path, file.OpenRead(), new MetadataItems
                        {
                            { Keys.SourceFileRoot, inputPath ?? file.Path.Directory },
                            { Keys.SourceFileBase, fileNameWithoutExtension },
                            { Keys.SourceFileExt, file.Path.Extension },
                            { Keys.SourceFileName, file.Path.FileName },
                            { Keys.SourceFileDir, file.Path.Directory },
                            { Keys.SourceFilePath, file.Path },
                            { Keys.SourceFilePathBase, fileNameWithoutExtension == null
                                ? null : file.Path.Directory.CombineFile(file.Path.FileNameWithoutExtension) },
                            { Keys.RelativeFilePath, relativePath },
                            { Keys.RelativeFilePathBase, fileNameWithoutExtension == null
                                ? null : relativePath.Directory.CombineFile(file.Path.FileNameWithoutExtension) },
                            { Keys.RelativeFileDir, relativePath.Directory }
                        });
                    });
            }
            return Array.Empty<IDocument>();
        }
    }
}
