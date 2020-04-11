using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statiq.App;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;
using Statiq.Markdown;
using Statiq.Razor;
using Statiq.Yaml;

namespace Statiq.Web.Pipelines
{
    public class Data : Pipeline
    {
        public Data()
        {
            InputModules = new ModuleList
            {
                new ReadFiles("**/{!_,}*.{json,yaml,yml}")
            };

            ProcessModules = new ModuleList
            {
                // Concat all documents from externally declared dependencies (exclude explicit dependencies above)
                new ConcatDocuments(Config.FromContext<IEnumerable<IDocument>>(ctx => ctx.Outputs.FromPipelines(ctx.Pipeline.GetAllDependencies(ctx).Except(Dependencies).ToArray()))),
                new ExecuteSwitch(Config.FromDocument(doc => doc.ContentProvider.MediaType))
                    .Case(MediaTypes.Json, new ParseJson())
                    .Case(MediaTypes.Yaml, new ParseYaml()),
                new FilterDocuments(Config.FromDocument(doc => !Feeds.IsFeed(doc)))
            };

            OutputModules = new ModuleList
            {
                new FilterDocuments(Config.FromSetting<bool>(WebKeys.OutputData)),
                new WriteFiles()
            };
        }
    }
}
