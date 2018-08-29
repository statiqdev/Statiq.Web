using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Html;
using Wyam.CodeAnalysis;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Util;
using Wyam.Html;

namespace Wyam.Docs
{
    /// <summary>
    /// Extensions used by the views.
    /// </summary>
    public static class Extensions
    {
        public static HtmlString Name(this IMetadata metadata) => FormatName(metadata.String(CodeAnalysisKeys.DisplayName));

        private static HtmlString FormatName(string name)
        {
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

        public static HtmlString GetTypeLink(this IExecutionContext context, IMetadata metadata) => context.GetTypeLink(metadata, metadata.Name());

        public static HtmlString GetTypeLink(this IExecutionContext context, IMetadata metadata, string name) => context.GetTypeLink(metadata, name == null ? null : FormatName(name));

        public static HtmlString GetTypeLink(this IExecutionContext context, IMetadata metadata, HtmlString name)
        {
            // Link nullable types to their type argument
            if (metadata.String("Name") == "Nullable")
            {
                IDocument nullableType = metadata.DocumentList(CodeAnalysisKeys.TypeArguments)?.FirstOrDefault();
                if (nullableType != null)
                {
                    return context.GetTypeLink(nullableType, name);
                }
            }

            if (metadata.String("Kind") == "TypeParameter")
            {
                IDocument declaringType = metadata.Document(CodeAnalysisKeys.DeclaringType);
                if (declaringType != null)
                {
                    return declaringType.ContainsKey(Keys.WritePath)
                        ? new HtmlString($"<a href=\"{context.GetLink(declaringType.FilePath(Keys.WritePath))}#typeparam-{metadata["Name"]}\">{metadata.Name()}</a>")
                        : metadata.Name();
                }
            }

            return metadata.ContainsKey(Keys.WritePath)
                ? new HtmlString($"<a href=\"{context.GetLink(metadata.FilePath(Keys.WritePath))}\">{name ?? metadata.Name()}</a>")
                : (name ?? metadata.Name());
        }

        /// <summary>
        /// Generates links to each heading on a page and returns a string containing all of the links.
        /// </summary>
        public static string GenerateInfobarHeadings(this IExecutionContext context, IDocument document)
        {
            StringBuilder content = new StringBuilder();
            IReadOnlyList<IDocument> headings = document.DocumentList(HtmlKeys.Headings);
            if (headings != null)
            {
                foreach (IDocument heading in headings)
                {
                    string id = heading.String(HtmlKeys.Id);
                    if (id != null)
                    {
                        content.AppendLine($"<p><a href=\"#{id}\">{heading.Content}</a></p>");
                    }
                }
            }
            if (content.Length > 0)
            {
                content.Insert(0, "<h6>On This Page</h6>");
                content.AppendLine("<hr class=\"infobar-hidden\" />");
                return content.ToString();
            }
            return null;
        }
    }
}