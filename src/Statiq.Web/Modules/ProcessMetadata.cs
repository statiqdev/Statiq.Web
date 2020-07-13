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
        public ProcessMetadata()
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
                new ExtractFrontMatter(new ParseYaml()),
                new ParseDataContent())
        {
        }
    }
}