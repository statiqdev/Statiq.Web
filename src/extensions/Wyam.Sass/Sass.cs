using System;
using System.Collections.Generic;
using System.Linq;
using LibSass.Compiler;
using LibSass.Compiler.Options;
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
    /// <category>Templates</category>
    public class Sass : IModule
    {
        private readonly List<string> _includePaths = new List<string>();
        private bool _includeSourceComments = true;
        private SassOutputStyle _outputStyle = SassOutputStyle.Compact;

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
        /// Adds a path to search while processing includes.
        /// </summary>
        /// <param name="path">The path to include.</param>
        /// <returns>The current instance.</returns>
        public Sass WithIncludePath(string path)
        {
            _includePaths.Add(path);
            return this;
        }

        /// <summary>
        /// Sets whether the source comments are included
        /// </summary>
        /// <param name="includeSourceComments">The default value is <c>true</c></param>
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
            _outputStyle = SassOutputStyle.Compact;
            return this;
        }


        /// <summary>
        /// Sets the output style to expanded.
        /// </summary>
        /// <returns>The current instance.</returns>
        public Sass WithExpandedOutputStyle()
        {
            _outputStyle = SassOutputStyle.Expanded;
            return this;
        }

        /// <summary>
        /// Sets the output style to compressed.
        /// </summary>
        /// <returns>The current instance.</returns>
        public Sass WithCompressedOutputStyle()
        {
            _outputStyle = SassOutputStyle.Compressed;
            return this;
        }

        /// <summary>
        /// Sets the output style to nested.
        /// </summary>
        /// <returns>The current instance.</returns>
        public Sass WithNestedOutputStyle()
        {
            _outputStyle = SassOutputStyle.Nested;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().Select(context, input =>
            {
                Trace.Verbose("Processing Sass for {0}", input.SourceString());

                FilePath path = input.FilePath(Keys.SourceFilePath);
                SassOptions sassOptions = new SassOptions
                {
                    Data = input.Content,
                    IncludeSourceComments = _includeSourceComments,
                    OutputStyle = _outputStyle,
                    IncludePaths = path == null
                        ? _includePaths.ToArray()
                        : new List<string>(_includePaths) { path.Directory.FullPath }.ToArray()
                };

                SassCompiler sassCompiler = new SassCompiler(sassOptions);
                SassResult sassResult ;
                try
                {
                    sassResult = sassCompiler.Compile();
                }
                catch (Exception ex)
                {
                    Trace.Warning("Exception while compiling sass file {0}: {}", input.SourceString(), ex.ToString());
                    return context.GetDocument(input, input.SourceString());
                }

                if (sassResult.ErrorStatus != 0)
                {
                    Trace.Warning("Exception while compiling sass file {0}: {1}", input.SourceString(), sassResult.ErrorMessage);
                    return context.GetDocument(input, input.SourceString());
                }

                return context.GetDocument(input, sassResult.Output);
            });
        }
    }
}
