using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dotless.Core;
using dotless.Core.configuration;
using dotless.Core.Cache;
using dotless.Core.Parser.Infrastructure.Nodes;
using dotless.Core.Parser.Tree;
using dotless.Core.Plugins;
using Microsoft.Practices.ServiceLocation;
using Pandora.Fluent;
using ReflectionMagic;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Tracing;
using Wyam.Common.Util;

namespace Wyam.Less
{
    /// <summary>
    /// Compiles Less CSS files to CSS stylesheets.
    /// </summary>
    /// <remarks>
    /// The content of the input document is compiled to CSS and the content of the output document contains the compiled CSS stylesheet.
    /// </remarks>
    /// <example>
    /// This is a pipeline that compiles two Less CSS files, one for Bootstrap (which contains a lot of includes) and a second for custom CSS.
    /// <code>
    /// Pipelines.Add("Less",
    ///     ReadFiles("master.less"),
    ///     Concat(ReadFiles("bootstrap.less")),
    ///     Less(),
    ///     WriteFiles(".css")
    /// );
    /// </code>
    /// </example>
    /// <metadata cref="Keys.RelativeFilePath" usage="Input" />
    /// <metadata cref="Keys.RelativeFilePath" usage="Output">Relative path to the output CSS (or map) file.</metadata>
    /// <metadata cref="Keys.WritePath" usage="Output" />
    /// <category>Templates</category>
    public class Less : IModule
    {
        private DocumentConfig _inputPath = (doc, ctx) => doc.FilePath(Keys.RelativeFilePath);

        /// <summary>
        /// Specifies a delegate that should be used to get the input path for each
        /// input document. This allows the Sass processor to search the right
        /// file system and paths for include files. By default, the <see cref="Keys.RelativeFilePath"/>
        /// metadata value is used for the input document path.
        /// </summary>
        /// <param name="inputPath">A delegate that should return a <see cref="FilePath"/>.</param>
        /// <returns>The current instance.</returns>
        public Less WithInputPath(DocumentConfig inputPath)
        {
            _inputPath = inputPath ?? throw new ArgumentNullException(nameof(inputPath));
            return this;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            DotlessConfiguration config = DotlessConfiguration.GetDefault();
            config.Logger = typeof(LessLogger);
            EngineFactory engineFactory = new EngineFactory(config);
            FileSystemReader fileSystemReader = new FileSystemReader(context.FileSystem);

            return inputs.AsParallel().Select(context, input =>
            {
                Trace.Verbose("Processing Less for {0}", input.SourceString());
                ILessEngine engine = engineFactory.GetEngine();

                // TODO: Get rid of RefelectionMagic and this ugly hack as soon as dotless gets better external DI support
                engine.AsDynamic().Underlying.Cache = new LessCache(context.ExecutionCache);
                engine.AsDynamic().Underlying.Underlying.Parser.Importer.FileReader = fileSystemReader;

                // Less conversion
                FilePath path = _inputPath.Invoke<FilePath>(input, context);
                if (path != null)
                {
                    engine.CurrentDirectory = path.Directory.FullPath;
                }
                else
                {
                    engine.CurrentDirectory = string.Empty;
                    path = new FilePath(Path.GetRandomFileName());
                    Trace.Warning($"No input path found for document {input.SourceString()}, using {path.FileName.FullPath}");
                }
                string content = engine.TransformToCss(input.Content, path.FileName.FullPath);

                // Process the result
                FilePath cssPath = path.ChangeExtension("css");
                return context.GetDocument(
                    input,
                    context.GetContentStream(content),
                    new MetadataItems
                    {
                        { Keys.RelativeFilePath, cssPath },
                        { Keys.WritePath, cssPath }
                    });
            });
        }
    }
}
