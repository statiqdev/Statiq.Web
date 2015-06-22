using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dotless.Core;
using dotless.Core.configuration;
using Pandora.Fluent;
using Wyam.Abstractions;

namespace Wyam.Modules.Less
{
    public class Less : IModule
    {
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            DotlessConfiguration config = DotlessConfiguration.GetDefault();
            config.RootPath = context.InputFolder;
            config.Logger = typeof (LessLogger);
            EngineFactory engineFactory = new EngineFactory(config);
            return inputs.Select(x =>
            {
                ILessEngine engine = engineFactory.GetEngine();
                string path = x.Get<string>(MetadataKeys.SourceFilePath, null);
                string fileName = null;
                if (path != null)
                {
                    engine.CurrentDirectory = Path.GetDirectoryName(path);
                    fileName = Path.GetFileName(path);
                }
                string content = engine.TransformToCss(x.Content, fileName);
                return x.Clone(content);
            });
        }
    }
}
