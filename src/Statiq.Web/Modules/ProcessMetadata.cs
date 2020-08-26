using System.Linq;
using Statiq.Common;
using Statiq.Core;
using Statiq.Yaml;

namespace Statiq.Web.Modules
{
    /// <summary>
    /// Parent module that contains the modules used for processing front matter and sidecar files.
    /// </summary>
    public class ProcessMetadata : ForAllDocuments
    {
        public ProcessMetadata(string frontMatterStartDelimiter = null, string frontMatterEndDelimiter = null)
            : base(
                new ExecuteIf(Config.FromSetting(WebKeys.ApplyDirectoryMetadata, true))
                {
                    new ApplyDirectoryMetadata()
                },
                new ExecuteIf(Config.FromSetting(WebKeys.ProcessSidecarFiles, true))
                {
                    new ProcessSidecarFile(Config.FromDocument((doc, ctx) =>
                    {
                        NormalizedPath relativePath = doc.Source.IsNull ? NormalizedPath.Null : doc.Source.GetRelativeInputPath();
                        return relativePath.IsNull
                            ? NormalizedPath.Null
                            : ParseDataContent.SupportedExtensions
                                .Select(x => relativePath.InsertPrefix("_").ChangeExtension(x))
                                .FirstOrDefault(x => ctx.FileSystem.GetInputFile(x).Exists);
                    }))
                    {
                        new ParseDataContent()
                    }
                },
                new ExecuteIf(Config.FromDocument(doc => doc.MediaTypeEquals(MediaTypes.CSharp)))
                {
                    // Special case to pre-process C# files with comment-style front matter delimiters first
                    new ExtractFrontMatter("*/", GetFrontMatterModules()).RequireStartDelimiter("/*")
                },
                new ExtractFrontMatter(GetFrontMatterModules()),
                new SetMetadata(WebKeys.Published, Config.FromDocument((doc, ctx) => doc.GetPublishedDate(ctx, ctx.GetBool(WebKeys.PublishedUsesLastModifiedDate)))))
        {
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