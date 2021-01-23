using System.Collections.Generic;
using System.Linq;
using Statiq.Common;
using Statiq.Core;
using Statiq.Web.Modules;
using Statiq.Yaml;

namespace Statiq.Web.Pipelines
{
    public class Inputs : Pipeline
    {
        public Inputs(Templates templates)
        {
            string[] templateAndScriptMediaTypes = templates.Keys.Concat(MediaTypes.CSharp).ToArray();

            Dependencies.Add(nameof(DirectoryMetadata));

            InputModules = new ModuleList
            {
                // Read files in one place so that the documents have a unified ID and we can apply metadata once
                // Also add headless CMS support or other input sources here (probably at the end of ProcessModules as a concat if they come over with metadata)
                new ReadFiles(Config.FromSetting<IEnumerable<string>>(WebKeys.InputFiles))
            };

            ProcessModules = new ModuleList
            {
                // Apply directory metadata
                new ExecuteIf(Config.FromSetting(WebKeys.ApplyDirectoryMetadata, true))
                {
                    new ApplyDirectoryMetadata()
                },

                // Apply side car files
                new ExecuteIf(Config.FromSetting(WebKeys.ProcessSidecarFiles, true))
                {
                    new ProcessSidecarFile(Config.FromDocument((doc, ctx) =>
                    {
                        NormalizedPath relativePath = doc.Source.IsNull ? NormalizedPath.Null : doc.Source.GetRelativeInputPath();
                        if (relativePath.IsNull)
                        {
                            return NormalizedPath.Null;
                        }
                        relativePath = relativePath.InsertPrefix("_");
                        return templates.GetMediaTypes(ContentType.Data, Phase.Process)
                            .SelectMany(MediaTypes.GetExtensions)
                            .SelectMany(x => new NormalizedPath[]
                            {
                                relativePath.ChangeExtension(x),
                                relativePath.AppendExtension(x)
                            })
                            .FirstOrDefault(x => ctx.FileSystem.GetInputFile(x).Exists);
                    }))
                    {
                        templates.GetModule(ContentType.Data, Phase.Process)
                    }
                },

                // Set media type from directory and sidecar files
                new ExecuteIf(Config.FromDocument(doc => doc.ContainsKey(WebKeys.MediaType)))
                {
                    new SetMediaType(Config.FromDocument(doc => doc.GetString(WebKeys.MediaType)))
                },

                // Process front matter for media types where we have an existing template (and scripts)
                new ExecuteIf(Config.FromDocument(doc => templateAndScriptMediaTypes.Any(x => doc.MediaTypeEquals(x))))
                {
                    // Apply front matter
                    new ExecuteIf(Config.FromDocument(doc => doc.MediaTypeEquals(MediaTypes.CSharp)))
                    {
                        // Special case to pre-process C# files with comment-style front matter delimiters first
                        new ExtractFrontMatter("*/", GetFrontMatterModules()).RequireStartDelimiter("/*")
                    },
                    new ExtractFrontMatter(GetFrontMatterModules()),

                    // Set new media type from metadata (in case front matter changed it)
                    new ExecuteIf(Config.FromDocument(doc => doc.ContainsKey(WebKeys.MediaType)))
                    {
                        new SetMediaType(Config.FromDocument(doc => doc.GetString(WebKeys.MediaType)))
                    },

                    // Set some standard metadata
                    new SetMetadata(WebKeys.Published, Config.FromDocument((doc, ctx) => doc.GetPublishedDate(ctx, ctx.GetBool(WebKeys.PublishedUsesLastModifiedDate)))),

                    // Enumerate metadata values (remove the enumerate key so following modules won't enumerate again)
                    new EnumerateValues(),
                    new SetMetadata(Keys.Enumerate, (string)null),

                    // Evaluate scripts (but defer if the script is for an archive)
                    // Script return document should either have media type or content type set, or script metadata should have content type, otherwise script output will be treated as an asset
                    new ProcessScripts(true)
                },

                // Set content type based on media type if not already explicitly set as metadata
                new SetContentType(templates),

                // If it's data, go ahead and process the data content (which might actually change the content type to something else
                // This also lets us filter out feeds in the data pipeline without evaluating the data twice
                new ExecuteIf(Config.FromDocument(doc => doc.Get<ContentType>(WebKeys.ContentType) == ContentType.Data))
                {
                    templates.GetModule(ContentType.Data, Phase.Process),
                },

                // Enumerate values one more time in case data content or the script output some (remove the enumerate key so following modules won't enumerate again)
                new EnumerateValues(),
                new SetMetadata(Keys.Enumerate, (string)null),

                // Filter out excluded documents
                new FilterDocuments(Config.FromDocument(doc => !doc.GetBool(WebKeys.Excluded))),
            };
        }

        private static IModule[] GetFrontMatterModules()
        {
            ModuleList modules = new ModuleList();

            // TODO: accept other types of front matter based on first char
            modules.Add(new ParseYaml());

            return modules.ToArray();
        }
    }
}
