using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dotless.Core;
using dotless.Core.configuration;
using dotless.Core.Cache;
using Microsoft.Practices.ServiceLocation;
using Pandora.Fluent;
using ReflectionMagic;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using Wyam.Common.Tracing;

namespace Wyam.Modules.Less
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
    /// <category>Templates</category>
    public class Less : IModule
    {
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            DotlessConfiguration config = DotlessConfiguration.GetDefault();
            config.Logger = typeof (LessLogger);
            EngineFactory engineFactory = new EngineFactory(config);
            
            return inputs.AsParallel().Select(input =>
            {
                Trace.Verbose("Processing Less for {0}", input.SourceString());
                ILessEngine engine = engineFactory.GetEngine();
                
                // TODO: Get rid of RefelectionMagic and this ugly hack as soon as dotless gets better external DI support
                engine.AsDynamic().Underlying.Cache = new LessCache(context.ExecutionCache);

                FilePath path = input.FilePath(Keys.SourceFilePath);
                string fileName = null;
                if (path != null)
                {
                    engine.CurrentDirectory = path.Directory.FullPath;
                    fileName = path.FileName.FullPath;
                }
                else
                {
                    engine.CurrentDirectory = context.FileSystem.InputPaths.Last().FullPath;
                    fileName = Path.GetRandomFileName();
                }
                string content = engine.TransformToCss(input.Content, fileName);
                return context.GetDocument(input, content);
            });
        }
    }
}
