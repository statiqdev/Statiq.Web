using System.Collections.Generic;
using System.Linq;
using Statiq.Common;
using Statiq.Core;
using Statiq.Web.Modules;
using Statiq.Yaml;

namespace Statiq.Web.Pipelines
{
    public class Data : Pipeline
    {
        public Data()
        {
            Dependencies.Add(nameof(DirectoryMetadata));

            InputModules = new ModuleList
            {
                new ReadFiles(Config.FromSetting<IEnumerable<string>>(WebKeys.DataFiles))
            };

            ProcessModules = new ModuleList
            {
                // Concat all documents from externally declared dependencies (exclude explicit dependencies above)
                new ConcatDocuments(Config.FromContext<IEnumerable<IDocument>>(ctx => ctx.Outputs.FromPipelines(ctx.Pipeline.GetAllDependencies(ctx).Except(Dependencies).ToArray()))),

                // Process directory metadata, sidecar files, and front matter
                new ProcessMetadata(),

                // Evaluate scripts
                new ExecuteIf(Config.FromDocument<bool>(WebKeys.Script))
                {
                    new EvaluateScript()
                },

                // Parse the actual document content
                new ParseDataContent(),

                // Clear the content so data documents can be safely sent to the content pipeline or rendered with a layout
                new ExecuteIf(Config.FromDocument(WebKeys.ClearContent, true))
                {
                    new SetContent(string.Empty)
                },

                // Filter out excluded documents
                new FilterDocuments(Config.FromDocument(doc => !doc.GetBool(WebKeys.Excluded))),

                // Filter out feed documents (they'll get processed by the Feed pipeline)
                // This has to come after the data content is parsed since that contains the feed metadata
                new FilterDocuments(Config.FromDocument(doc => !Feeds.IsFeed(doc))),

                // Enumerate metadata values
                new EnumerateValues(),

                // Set the destination and optimize filenames
                new SetDestination(),
                new ExecuteIf(Config.FromSetting(WebKeys.OptimizeDataFileNames, true))
                {
                    new OptimizeFileName()
                }
            };

            OutputModules = new ModuleList
            {
                new FilterDocuments(Config.FromDocument<bool>(WebKeys.ShouldOutput)),
                new WriteFiles()
            };
        }
    }
}
