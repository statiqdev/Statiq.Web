using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public static HtmlString Name(this IMetadata metadata) => new HtmlString(FormatName(metadata.String(CodeAnalysisKeys.DisplayName)));

        public static HtmlString GetTypeLink(this IExecutionContext context, IMetadata metadata) => context.GetTypeLink(metadata, null, true);

        public static HtmlString GetTypeLink(this IExecutionContext context, IMetadata metadata, bool linkTypeArguments) => context.GetTypeLink(metadata, null, linkTypeArguments);

        public static HtmlString GetTypeLink(this IExecutionContext context, IMetadata metadata, string name) => context.GetTypeLink(metadata, name, true);

        public static HtmlString GetTypeLink(this IExecutionContext context, IMetadata metadata, string name, bool linkTypeArguments)
        {
            name = name ?? metadata.String(CodeAnalysisKeys.DisplayName);

            // Link nullable types to their type argument
            if (metadata.String("Name") == "Nullable")
            {
                IDocument nullableType = metadata.DocumentList(CodeAnalysisKeys.TypeArguments)?.FirstOrDefault();
                if (nullableType != null)
                {
                    return context.GetTypeLink(nullableType, name);
                }
            }

            // If it wasn't nullable, format the name
            name = FormatName(name);

            // Link the type and type parameters seperatly for generic types
            IReadOnlyList<IDocument> typeArguments = metadata.DocumentList(CodeAnalysisKeys.TypeArguments);
            if (typeArguments?.Count > 0)
            {
                // Link to the original definition of the generic type
                metadata = metadata.Document(CodeAnalysisKeys.OriginalDefinition) ?? metadata;

                if (linkTypeArguments)
                {
                    // Get the type argument positions
                    int begin = name.IndexOf("<wbr>&lt;") + 9;
                    int openParen = name.IndexOf("&gt;<wbr>(");
                    int end = name.LastIndexOf("&gt;<wbr>", openParen == -1 ? name.Length : openParen);  // Don't look past the opening paren if there is one

                    // Remove existing type arguments and insert linked type arguments (do this first to preserve original indexes)
                    name = name
                        .Remove(begin, end - begin)
                        .Insert(begin, string.Join(", <wbr>", typeArguments.Select(x => context.GetTypeLink(x, true).Value)));

                    // Insert the link for the type
                    if (metadata.ContainsKey(Keys.WritePath))
                    {
                        name = name.Insert(begin - 9, "</a>").Insert(0, $"<a href=\"{context.GetLink(metadata.FilePath(Keys.WritePath))}\">");
                    }

                    return new HtmlString(name);
                }
            }

            // If it's a type parameter, create an anchor link to the declaring type's original definition
            if (metadata.String("Kind") == "TypeParameter")
            {
                IDocument declaringType = metadata.Document(CodeAnalysisKeys.DeclaringType)?.Document(CodeAnalysisKeys.OriginalDefinition);
                if (declaringType != null)
                {
                    return new HtmlString(declaringType.ContainsKey(Keys.WritePath)
                        ? $"<a href=\"{context.GetLink(declaringType.FilePath(Keys.WritePath))}#typeparam-{metadata["Name"]}\">{name}</a>"
                        : name);
                }
            }

            return new HtmlString(metadata.ContainsKey(Keys.WritePath)
                ? $"<a href=\"{context.GetLink(metadata.FilePath(Keys.WritePath))}\">{name}</a>"
                : name);
        }

        // https://stackoverflow.com/a/3143036/807064
        private static IEnumerable<string> SplitAndKeep(this string s, params char[] delims)
        {
            int start = 0, index;

            while ((index = s.IndexOfAny(delims, start)) != -1)
            {
                if (index - start > 0)
                {
                    yield return s.Substring(start, index - start);
                }

                yield return s.Substring(index, 1);
                start = index + 1;
            }

            if (start < s.Length)
            {
                yield return s.Substring(start);
            }
        }

        private static string FormatName(string name)
        {
            if (name == null)
            {
                return string.Empty;
            }

            // Encode and replace .()<> with word break opportunities
            name = WebUtility.HtmlEncode(name)
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

            return replaced ? string.Join("<wbr>", segments) : name;
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