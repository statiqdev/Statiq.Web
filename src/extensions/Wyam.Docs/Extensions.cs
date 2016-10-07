using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            if (name == null)
            {
                return new HtmlString(string.Empty);
            }

            // Encode and replace .()<> with word break opportunities
            name = System.Net.WebUtility.HtmlEncode(name)
                .Replace(".", "<wbr>.")
                .Replace("(", "<wbr>(")
                .Replace(")", ")<wbr>")
                .Replace(", ", ", <wbr>")
                .Replace("&lt;", "<wbr>&lt;")
                .Replace("&gt;", "&gt;<wbr>");

            // Add additional break opportunities in long un-broken segments
            List<string> segments = name.Split(new[] { "<wbr>" }, StringSplitOptions.None).ToList();
            bool replaced = false;
            for (int c = 0; c < segments.Count; c++)
            {
                if (segments[c].Length > 20)
                {
                    segments[c] = new string(segments[c]
                        .SelectMany((x, i) => char.IsUpper(x) && i != 0 ? new[] { '<', 'w', 'b', 'r', '>', x } : new[] { x })
                        .ToArray());
                    replaced = true;
                }
            }

            return new HtmlString(replaced ? string.Join("<wbr>", segments) : name);
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