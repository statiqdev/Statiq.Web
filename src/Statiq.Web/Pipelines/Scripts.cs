using System.Collections.Generic;
using System.Linq;
using Statiq.Common;
using Statiq.Core;
using Statiq.Web.Modules;
using Statiq.Yaml;

namespace Statiq.Web.Pipelines
{
    public class Scripts : Pipeline
    {
        public Scripts()
        {
            Dependencies.AddRange(nameof(Data), nameof(Content), nameof(Archives), nameof(DirectoryMetadata));

            InputModules = new ModuleList
            {
                new ReadFiles(Config.FromSetting<IEnumerable<string>>(WebKeys.ScriptFiles))
            };

            ProcessModules = new ModuleList
            {
                // Concat all documents from externally declared dependencies (exclude explicit dependencies above)
                new ConcatDocuments(Config.FromContext<IEnumerable<IDocument>>(ctx => ctx.Outputs.FromPipelines(ctx.Pipeline.GetAllDependencies(ctx).Except(Dependencies).ToArray()))),

                // Process directory metadata, sidecar files, front matter, and data content
                new ProcessMetadata(),

                // Filter out excluded documents
                new FilterDocuments(Config.FromDocument(doc => !doc.GetBool(WebKeys.Excluded))),

                // Filter out archive documents (they'll get processed by the Archives pipeline)
                new FilterDocuments(Config.FromDocument(doc => !Archives.IsArchive(doc))),

                // Enumerate metadata values
                new EnumerateValues(),

                // Set the destination and optimize filenames
                new SetDestination(),
                new ExecuteIf(Config.FromSetting(WebKeys.OptimizeScriptFileNames, true))
                {
                    new OptimizeFileName()
                },

                // Execute the script
                new EvaluateScript()
            };

            OutputModules = new ModuleList
            {
                new FilterDocuments(Config.FromDocument<bool>(WebKeys.ShouldOutput, true)),
                new WriteFiles()
            };
        }
    }
}
