using Microsoft.AspNetCore.Html;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;

namespace Wyam.Docs
{
    /// <summary>
    /// Extensions used by the views.
    /// </summary>
    public static class Extensions
    {
        public static HtmlString Name(this IMetadata metadata)
        {
            string name = metadata.String("DisplayName");
            return name == null
                ? new HtmlString(string.Empty)
                : new HtmlString(System.Net.WebUtility.HtmlEncode(name.ToString()).ToString()
                    .Replace(".", "<wbr>.")
                    .Replace("(", "<wbr>(")
                    .Replace(")", ")<wbr>")
                    .Replace("&lt;", "<wbr>&lt;")
                    .Replace("&gt;", "&gt;<wbr>"));
        }

        public static HtmlString GetTypeLink(this IExecutionContext context, IMetadata metadata)
        {
            if (metadata.String("Kind") == "TypeParameter")
            {
                IDocument declaringType = metadata.Get<IDocument>("DeclaringType");
                if (declaringType != null)
                {
                    return declaringType.ContainsKey("WritePath")
                        ? new HtmlString($"<a href=\"{context.GetLink(declaringType.FilePath("WritePath"))}#typeparam-{metadata["Name"]}\">{metadata.Name()}</a>")
                        : metadata.Name();
                }
            }
            return metadata.ContainsKey("WritePath")
                ? new HtmlString($"<a href=\"{context.GetLink(metadata.FilePath("WritePath"))}\">{metadata.Name()}</a>")
                : metadata.Name();
        }
    }
}