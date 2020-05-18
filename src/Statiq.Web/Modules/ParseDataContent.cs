using Statiq.Common;
using Statiq.Core;
using Statiq.Yaml;

namespace Statiq.Web.Modules
{
    /// <summary>
    /// Common parsing of data content based on media type.
    /// </summary>
    public class ParseDataContent : ForAllDocuments
    {
        public static string[] SupportedExtensions { get; } = new[] { "json", "yaml", "yml" };

        public ParseDataContent()
            : base(
                new ExecuteSwitch(Config.FromDocument(doc => doc.ContentProvider.MediaType))
                    .Case(MediaTypes.Json, new ParseJson())
                    .Case(MediaTypes.Yaml, new ParseYaml()))
        {
        }
    }
}