using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpScss;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;
using Wyam.Common.Util;

namespace Wyam.Sass
{
    /// <summary>
    /// Compiles Sass CSS files to CSS stylesheets.
    /// </summary>
    /// <remarks>
    /// The content of the input document is compiled to CSS and the content of the output document contains the compiled CSS stylesheet.
    /// </remarks>
    /// <example>
    /// This is a pipeline that compiles two Sass CSS files, one for Bootstrap (which contains a lot of includes) and a second for custom CSS.
    /// <code>
    /// Pipelines.Add("Sass",
    ///     ReadFiles("master.scss"),
    ///     Concat(ReadFiles("foundation.scss")),
    ///     Sass().WithCompactOutputStyle(),
    ///     WriteFiles(".css")
    /// );
    /// </code>
    /// </example>
    /// <metadata cref="Keys.SourceFilePath" usage="Input">The default key to use for determining the input document path.</metadata>
    /// <metadata cref="Keys.RelativeFilePath" usage="Input">If <see cref="Keys.SourceFilePath"/> is unavailable, this is used to guess at the source file path.</metadata>
    /// <metadata cref="Keys.RelativeFilePath" usage="Output">Relative path to the output CSS (or map) file.</metadata>
    /// <metadata cref="Keys.WritePath" usage="Output" />
    /// <category>Templates</category>
    public class Sass : IModule
    {
        private DocumentConfig _inputPath = DefaultInputPath;
        private readonly List<string> _includePaths = new List<string>();
        private bool _includeSourceComments = false;
        private ScssOutputStyle _outputStyle = ScssOutputStyle.Compact;
        private bool _generateSourceMap = false;

        /// <summary>
        /// Specifies a delegate that should be used to get the input path for each
        /// input document. This allows the Sass processor to search the right
        /// file system and paths for include files. By default, the <see cref="Keys.RelativeFilePath"/>
        /// metadata value is used for the input document path.
        /// </summary>
        /// <param name="inputPath">A delegate that should return a <see cref="FilePath"/>.</param>
        /// <returns>The current instance.</returns>
        public Sass WithInputPath(DocumentConfig inputPath)
        {
            _inputPath = inputPath ?? throw new ArgumentNullException(nameof(inputPath));
            return this;
        }

        /// <summary>
        /// Adds a list of paths to search while processing includes.
        /// </summary>
        /// <param name="paths">The paths to include.</param>
        /// <returns>The current instance.</returns>
        public Sass WithIncludePaths(IEnumerable<string> paths)
        {
            _includePaths.AddRange(paths);
            return this;
        }

        /// <summary>
        /// Sets whether the source comments are included (by default they are not).
        /// </summary>
        /// <param name="includeSourceComments"><c>true</c> to include source comments.</param>
        /// <returns>The current instance.</returns>
        public Sass IncludeSourceComments(bool includeSourceComments = true)
        {
            _includeSourceComments = includeSourceComments;
            return this;
        }

        /// <summary>
        /// Sets the output style to compact.
        /// </summary>
        /// <returns>The current instance.</returns>
        public Sass WithCompactOutputStyle()
        {
            _outputStyle = ScssOutputStyle.Compact;
            return this;
        }

        /// <summary>
        /// Sets the output style to expanded.
        /// </summary>
        /// <returns>The current instance.</returns>
        public Sass WithExpandedOutputStyle()
        {
            _outputStyle = ScssOutputStyle.Expanded;
            return this;
        }

        /// <summary>
        /// Sets the output style to compressed.
        /// </summary>
        /// <returns>The current instance.</returns>
        public Sass WithCompressedOutputStyle()
        {
            _outputStyle = ScssOutputStyle.Compressed;
            return this;
        }

        /// <summary>
        /// Sets the output style to nested.
        /// </summary>
        /// <returns>The current instance.</returns>
        public Sass WithNestedOutputStyle()
        {
            _outputStyle = ScssOutputStyle.Nested;
            return this;
        }

        /// <summary>
        /// Specifies whether a source map should be generated (the default
        /// behavior is <c>false</c>).
        /// </summary>
        /// <param name="generateSourceMap"><c>true</c> to generate a source map.</param>
        /// <returns>The current instance.</returns>
        public Sass GenerateSourceMap(bool generateSourceMap = true)
        {
            _generateSourceMap = generateSourceMap;
            return this;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel()
                .SelectMany(context, input =>
                {
                    Trace.Verbose($"Processing Sass for {input.SourceString()}");

                    FilePath inputPath = _inputPath.Invoke<FilePath>(input, context);
                    if (inputPath == null || !inputPath.IsAbsolute)
                    {
                        inputPath = context.FileSystem.GetInputFile(new FilePath(Path.GetRandomFileName())).Path;
                        Trace.Warning($"No input path found for document {input.SourceString()}, using {inputPath.FileName.FullPath}");
                    }

                    string content = input.Content;

                    // Sass conversion
                    FileImporter importer = new FileImporter(context.FileSystem);
                    ScssOptions options = new ScssOptions
                    {
                        OutputStyle = _outputStyle,
                        GenerateSourceMap = _generateSourceMap,
                        SourceComments = _includeSourceComments,
                        InputFile = inputPath.FullPath,
                        TryImport = importer.TryImport
                    };
                    options.IncludePaths.AddRange(_includePaths);
                    ScssResult result = Scss.ConvertToCss(content, options);

                    // Process the result
                    DirectoryPath relativeDirectory = context.FileSystem.GetContainingInputPath(inputPath);
                    FilePath relativePath = relativeDirectory?.GetRelativePath(inputPath) ?? inputPath.FileName;

                    FilePath cssPath = relativePath.ChangeExtension("css");
                    IDocument cssDocument = context.GetDocument(
                        input,
                        context.GetContentStream(result.Css ?? string.Empty),
                        new MetadataItems
                        {
                            { Keys.RelativeFilePath, cssPath },
                            { Keys.WritePath, cssPath }
                        });

                    IDocument sourceMapDocument = null;
                    if (_generateSourceMap && result.SourceMap != null)
                    {
                        FilePath sourceMapPath = relativePath.ChangeExtension("map");
                        sourceMapDocument = context.GetDocument(
                            input,
                            context.GetContentStream(result.SourceMap),
                            new MetadataItems
                            {
                                { Keys.RelativeFilePath, sourceMapPath },
                                { Keys.WritePath, sourceMapPath }
                            });
                    }

                    return new[] { cssDocument, sourceMapDocument };
                })
                .Where(x => x != null);
        }

        private static object DefaultInputPath(IDocument document, IExecutionContext context)
        {
            FilePath path = document.FilePath(Keys.SourceFilePath);
            if (path == null)
            {
                path = document.FilePath(Keys.RelativeFilePath);
                if (path != null)
                {
                    IFile inputFile = context.FileSystem.GetInputFile(path);
                    return inputFile.Exists ? inputFile.Path : null;
                }
            }
            return path;
        }
    }
}
