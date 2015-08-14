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

namespace Wyam.Modules.Less
{
    public class Less : IModule
    {
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            DotlessConfiguration config = DotlessConfiguration.GetDefault();
            config.Logger = typeof (LessLogger);
            EngineFactory engineFactory = new EngineFactory(config);
            
            return inputs.AsParallel().Select(x =>
            {
                context.Trace.Verbose("Processing Less for {0}", x.Source);
                ILessEngine engine = engineFactory.GetEngine();
                
                // TODO: Get rid of RefelectionMagic and this ugly hack as soon as dotless gets better external DI support
                engine.AsDynamic().Underlying.Cache = new LessCache(context.ExecutionCache);

                string path = x.Get<string>("SourceFilePath", null);
                string fileName = null;
                if (path != null)
                {
                    engine.CurrentDirectory = Path.GetDirectoryName(path);
                    fileName = Path.GetFileName(path);
                }
                else
                {
                    engine.CurrentDirectory = context.InputFolder;
                    fileName = Path.GetRandomFileName();
                }
                string content = engine.TransformToCss(x.Content, fileName);
                return x.Clone(content);
            });
        }
    }
}
