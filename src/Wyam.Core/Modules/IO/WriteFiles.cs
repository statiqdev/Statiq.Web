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
using Wyam.Core.Documents;
using Wyam.Core.Meta;

namespace Wyam.Core.Modules.IO
{
    /// <summary>
    /// Writes the content of each input document to the file system.
    /// </summary>
    /// <remarks>
    /// If the metadata keys <c>WriteFileName</c> (which requires <c>RelativeFileDir</c> to be 
    /// set, usually by the ReadFiles module), <c>WriteExtension</c> (which 
    /// requires <c>RelativeFilePath</c> to be set, usually by the <see cref="ReadFiles"/> module) 
    /// or <c>WritePath</c> are set on an input document, that value will be used instead 
    /// of what's specified in the module. For example, if you have a bunch 
    /// of Razor .cshtml files that need to be rendered to .html files but one of them 
    /// should be output as a .xml file instead, define the <c>WriteExtension</c> metadata value 
    /// in the front matter of the page.
    /// </remarks>
    /// <metadata name="DestinationFilePath" type="FilePath">The full absolute path (including file name) 
    /// of the destination file.</metadata>
    /// <metadata name="DestinationFilePathBase" type="FilePath">The full absolute path (including file name) 
    /// of the destination file without the file extension.</metadata>
    /// <metadata name="DestinationFileBase" type="FilePath">The file name without any extension. Equivalent 
    /// to <c>Path.GetFileNameWithoutExtension(DestinationFilePath)</c>.</metadata>
    /// <metadata name="DestinationFileExt" type="string">The extension of the file. Equivalent 
    /// to <c>Path.GetExtension(DestinationFilePath)</c>.</metadata>
    /// <metadata name="DestinationFileName" type="FilePath">The full file name. Equivalent 
    /// to <c>Path.GetFileName(DestinationFilePath)</c>.</metadata>
    /// <metadata name="DestinationFileDir" type="DirectoryPath">The full absolute directory of the file. 
    /// Equivalent to <c>Path.GetDirectoryName(DestinationFilePath)</c>.</metadata>
    /// <metadata name="RelativeFilePath" type="FilePath">The relative path to the file (including file name)
    /// from the Wyam output folder.</metadata>
    /// <metadata name="RelativeFilePathBase" type="FilePath">The relative path to the file (including file name)
    /// from the Wyam output folder without the file extension.</metadata>
    /// <metadata name="RelativeFileDir" type="DirectoryPath">The relative directory of the file 
    /// from the Wyam output folder.</metadata>
    /// <category>Input/Output</category>
    public class WriteFiles : IModule
    {
        private readonly DocumentConfig _path;
        private readonly bool _warnOnWriteMetadata;
        private bool _useWriteMetadata = true;
        private bool _ignoreEmptyContent = true;
        private Func<IDocument, IExecutionContext, bool> _predicate = null;

        /// <summary>
        /// Uses a delegate to describe where to write the content of each document. 
        /// The output of the function should be either a full path to the disk 
        /// location (including file name) or a path relative to the output folder.
        /// </summary>
        /// <param name="path">A delegate that returns a <see cref="FilePath"/> with the desired path.</param>
        public WriteFiles(DocumentConfig path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            _path = path;
            _warnOnWriteMetadata = true;
        }

        /// <summary>
        /// Writes the document content to disk with the specified extension with the same 
        /// base file name and relative path as the input file. This requires metadata 
        /// for <c>RelativeFilePath</c> to be set (which is done by default by the <see cref="ReadFiles"/> module).
        /// </summary>
        /// <param name="extension">The extension to use for writing the file.</param>
        public WriteFiles(string extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            _path = (x, y) =>
            {
                FilePath fileRelative = x.FilePath(Keys.RelativeFilePath);
                return fileRelative?.ChangeExtension(extension);
            };
            _warnOnWriteMetadata = true;
        }

        /// <summary>
        /// Writes the document content to disk with the same file name and relative path 
        /// as the input file. This requires metadata for <c>RelativeFilePath</c> to be set,
        /// which is done by the <see cref="ReadFiles"/> module or can be set manually.
        /// </summary>
        public WriteFiles()
        {
            _path = (x, y) => x.FilePath(Keys.RelativeFilePath);
        }
        
        /// <summary>
        /// By default the metadata values for <c>WritePath</c>, <c>WriteFileName</c>, and <c>WriteExtension</c> 
        /// are checked and used first, even if a delegate is specified in the constructor. This method can be used 
        /// to turn off the default behavior and always rely on the delegate for obtaining the write location.
        /// </summary>
        /// <param name="useWriteMetadata">If set to <c>false</c>, metadata of the input document will not be used.</param>
        public WriteFiles UseWriteMetadata(bool useWriteMetadata = true)
        {
            _useWriteMetadata = useWriteMetadata;
            return this;
        }

        /// <summary>
        /// Ignores documents with empty content, which is the default behavior.
        /// </summary>
        /// <param name="ignoreEmptyContent">If set to <c>true</c>, documents with empty content will be ignored.</param>
        public WriteFiles IgnoreEmptyContent(bool ignoreEmptyContent = true)
        {
            _ignoreEmptyContent = ignoreEmptyContent;
            return this;
        }

        /// <summary>
        /// Specifies a predicate that must be satisfied for the file to be written.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the file should be written.</param>
        public WriteFiles Where(DocumentConfig predicate)
        {
            Func<IDocument, IExecutionContext, bool> currentPredicate = _predicate;
            _predicate = currentPredicate == null
                ? (Func<IDocument, IExecutionContext, bool>)(predicate.Invoke<bool>)
                : ((x, c) => currentPredicate(x, c) && predicate.Invoke<bool>(x, c));
            return this;
        }

        protected bool ShouldProcess(IDocument input, IExecutionContext context)
        {
            return _predicate == null || _predicate(input, context);
        }

        protected IFile GetOutputFile(IDocument input, IExecutionContext context)
        {
            FilePath path = null;

            if (_useWriteMetadata)
            {
                string metadataKey = null;

                // WritePath
                path = input.FilePath(Keys.WritePath);
                if (path != null)
                {
                    metadataKey = Keys.WritePath;
                }

                // WriteFileName
                DirectoryPath relativeFileDir = input.DirectoryPath(Keys.RelativeFileDir);
                FilePath writeFileName = input.FilePath(Keys.WriteFileName);
                if (path == null 
                    && relativeFileDir != null
                    && writeFileName != null)
                {
                    metadataKey = Keys.WriteFileName;
                    path = relativeFileDir.CombineFile(writeFileName);
                }

                // WriteExtension
                FilePath relativeFilePath = input.FilePath(Keys.RelativeFilePath);
                string writeExtension = input.String(Keys.WriteExtension);
                if (path == null 
                    && relativeFilePath != null
                    && !string.IsNullOrWhiteSpace(writeExtension))
                {
                    metadataKey = Keys.WriteExtension;
                    path = relativeFilePath.ChangeExtension(writeExtension);
                }

                // Warn if needed
                if (metadataKey != null && _warnOnWriteMetadata)
                {
                    Trace.Warning("An extension or delegate was specified for the WriteFiles module, but the metadata key {0} took precedence for the document with source {1}."
                        + " Call UseWriteMetadata(false) to prevent the special write metadata keys from overriding WriteFiles constructor values.",
                        metadataKey, input.SourceString());
                }
            }

            // Func
            if (path == null)
            {
                path = _path.Invoke<FilePath>(input, context);
            }

            return path != null ? context.FileSystem.GetOutputFile(path) : null;
        }

        public virtual IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs
                .AsParallel()
                .Where(input => ShouldProcess(input, context))
                .Select(input =>
                {
                    IFile output = GetOutputFile(input, context);
                    if (output != null)
                    {
                        using (Stream inputStream = input.GetStream())
                        {
                            if (_ignoreEmptyContent && inputStream.Length == 0)
                            {
                                return input;
                            }
                            using (Stream outputStream = output.OpenWrite())
                            {
                                inputStream.CopyTo(outputStream);
                            }
                        }
                        Trace.Verbose("Wrote file {0}", output.Path.FullPath);
                        FilePath relativePath = context.FileSystem.GetOutputPath().GetRelativePath(output.Path) ?? output.Path.FileName;
                        FilePath fileNameWithoutExtension = output.Path.FileNameWithoutExtension;
                        return context.GetDocument(input, output.OpenRead(), new MetadataItems
                        {
                            { Keys.DestinationFileBase, fileNameWithoutExtension },
                            { Keys.DestinationFileExt, output.Path.Extension },
                            { Keys.DestinationFileName, output.Path.FileName },
                            { Keys.DestinationFileDir, output.Path.Directory },
                            { Keys.DestinationFilePath, output.Path },
                            { Keys.DestinationFilePathBase, fileNameWithoutExtension == null
                                ? null : output.Path.Directory.CombineFile(output.Path.FileNameWithoutExtension) },
                            { Keys.RelativeFilePath, relativePath },
                            { Keys.RelativeFilePathBase, fileNameWithoutExtension == null
                                ? null : relativePath.Directory.CombineFile(output.Path.FileNameWithoutExtension) },
                            { Keys.RelativeFileDir, relativePath.Directory }
                        });
                    }
                    return input;
                })
                .Where(x => x != null);
        }
    }
}
