using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        private bool _append;
        private Func<IDocument, IExecutionContext, bool> _predicate = null;
        private bool _onlyMetadata = false;

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
        /// Indicates that only metadata should be added to the document and a file should not
        /// actually be written to the file system. This is useful for preprocessing documents
        /// so they appear in a pipeline with the correct write metadata, while actually
        /// writing them later with a second <see cref="WriteFiles"/> module invocation.
        /// Only the following metadata values are written when this flag is turned on:
        /// <c>WritePath</c>, <c>RelativeFilePath</c>, <c>RelativeFilePathBase</c>,
        /// and <c>RelativeFileDir</c>. The <c>Destination...</c> metadata values are
        /// not added to the document when only setting metadata..
        /// </summary>
        /// <param name="onlyMetadata">If set to <c>true</c>, metadata will be added
        /// to the input document(s) without actually writing them to the file system.</param>
        public WriteFiles OnlyMetadata(bool onlyMetadata = true)
        {
            _onlyMetadata = onlyMetadata;
            return this;
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
        /// Appends content to each file instead of overwriting them.
        /// </summary>
        /// <param name="append">Appends to existing files if set to <c>true</c>.</param>
        /// <returns></returns>
        public WriteFiles Append(bool append = true)
        {
            _append = true;
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

        protected FilePath GetOutputPath(IDocument input, IExecutionContext context)
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
                    Trace.Warning($"An extension or delegate was specified for the WriteFiles module, but the metadata key {metadataKey} took precedence for the document with source {input.SourceString()}"
                        + $" resulting in an output path of {path}. Call UseWriteMetadata(false) to prevent the special write metadata keys from overriding WriteFiles constructor values.");
                }
            }

            // Fallback to the default behavior function
            return path ?? _path.Invoke<FilePath>(input, context, "while getting path");
        }

        public virtual IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            // Get the output file path for each file in sequence and set up action chains
            // Value = input source string(s) (for reporting a warning if not appending), write action
            ConcurrentBag<IDocument> outputs = new ConcurrentBag<IDocument>();
            Dictionary<FilePath, Tuple<List<string>, Action>> writesBySource = new Dictionary<FilePath, Tuple<List<string>, Action>>();
            foreach (IDocument input in inputs)
            {
                FilePath outputPath = ShouldProcess(input, context) ? GetOutputPath(input, context) : null;
                if (outputPath == null)
                {
                    // No output path or failed the predicate so just pass the input document through
                    outputs.Add(input);
                }
                else
                {
                    Tuple<List<string>, Action> value;
                    if (writesBySource.TryGetValue(outputPath, out value))
                    {
                        // This output source was already seen so nest the previous write action in a new one
                        value.Item1.Add(input.SourceString());
                        Action previousWrite = value.Item2;
                        value = new Tuple<List<string>, Action>(
                            value.Item1,
                            () =>
                            {
                                // Complete the previous write, then do the next one
                                previousWrite();
                                outputs.Add(Write(input, context, outputPath));
                            });
                    }
                    else
                    {
                        value = new Tuple<List<string>, Action>(
                            new List<string> {input.SourceString()},
                            () => outputs.Add(Write(input, context, outputPath)));
                    }
                    writesBySource[outputPath] = value;
                }
            }

            // Display a warning for any duplicated outputs if not appending
            if (!_append)
            {
                foreach (KeyValuePair<FilePath, Tuple<List<string>, Action>> kvp in writesBySource.Where(x => x.Value.Item1.Count > 1))
                {
                    string inputSources = Environment.NewLine + "  " + string.Join(Environment.NewLine + "  ", kvp.Value.Item1);
                    Trace.Warning($"Multiple documents output to {kvp.Key} (this probably wasn't intended):{inputSources}");
                }
            }

            // Run the write actions in parallel
            Parallel.Invoke(writesBySource.Values.Select(x => x.Item2).ToArray());

            // Aggregate and return the results
            return outputs;
        }
        
        private IDocument Write(IDocument input, IExecutionContext context, FilePath outputPath)
        {
            IFile output = context.FileSystem.GetOutputFile(outputPath);
            if (output != null)
            {
                using (Stream inputStream = input.GetStream())
                {
                    if (_ignoreEmptyContent && inputStream.Length == 0)
                    {
                        return input;
                    }
                    if (!_onlyMetadata)
                    {
                        using (Stream outputStream = _append ? output.OpenAppend() : output.OpenWrite())
                        {
                            inputStream.CopyTo(outputStream);
                        }
                    }
                }
                Trace.Verbose($"{(_onlyMetadata ? "Set metadata for" : "Wrote")} file {output.Path.FullPath} from {input.SourceString()}");
                FilePath relativePath = context.FileSystem.GetOutputPath().GetRelativePath(output.Path) ?? output.Path.FileName;
                FilePath fileNameWithoutExtension = output.Path.FileNameWithoutExtension;
                MetadataItems metadata = new MetadataItems
                {
                    { Keys.RelativeFilePath, relativePath },
                    { Keys.RelativeFilePathBase, fileNameWithoutExtension == null
                        ? null : relativePath.Directory.CombineFile(output.Path.FileNameWithoutExtension) },
                    { Keys.RelativeFileDir, relativePath.Directory }
                };
                if (_onlyMetadata)
                {
                    metadata.Add(Keys.WritePath, outputPath);
                }
                else
                {
                    metadata.AddRange(new MetadataItems
                    {
                        { Keys.DestinationFileBase, fileNameWithoutExtension },
                        { Keys.DestinationFileExt, output.Path.Extension },
                        { Keys.DestinationFileName, output.Path.FileName },
                        { Keys.DestinationFileDir, output.Path.Directory },
                        { Keys.DestinationFilePath, output.Path },
                        { Keys.DestinationFilePathBase, fileNameWithoutExtension == null
                            ? null : output.Path.Directory.CombineFile(output.Path.FileNameWithoutExtension) },
                    });
                }
                return _onlyMetadata
                    ? context.GetDocument(input, metadata)
                    : context.GetDocument(input, output.OpenRead(), metadata);
            }
            return input;
        }
    }
}
