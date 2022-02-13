using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Statiq.Common;

namespace Statiq.Web
{
    public abstract class ValidateLinks : HtmlAnalyzer
    {
        // Get <a>, <link>, <image>, and <script> links
        protected static IEnumerable<(string, IEnumerable<IElement>)> GetLinks(
            IHtmlDocument htmlDocument,
            Common.IDocument document,
            IAnalyzerContext context,
            bool absolute)
        {
            // Get the correct destination path for the document, taking into account a link root and dropping the file
            // This will be combined with the link to determine the actual output path
            NormalizedPath documentDestinationDirectory = document.Destination;
            if (!documentDestinationDirectory.IsNull)
            {
                NormalizedPath linkRoot = context.Settings.GetPath(Keys.LinkRoot);
                if (!linkRoot.IsNullOrEmpty)
                {
                    documentDestinationDirectory = linkRoot.Combine(documentDestinationDirectory);
                }
                documentDestinationDirectory = documentDestinationDirectory.Parent;
            }

            string baseHref = htmlDocument.GetElementsByTagName("base").FirstOrDefault()?.GetAttribute("href");
            return htmlDocument.Links.Where(e => !e.HasAttribute("data-no-validate")).Select(e => (e.GetAttribute("href"), (IElement)e))
                .Concat(htmlDocument.GetElementsByTagName("link").Where(e => e.HasAttribute("href") && !e.HasAttribute("data-no-validate")).Select(e => (e.GetAttribute("href"), (IElement)e)))
                .Concat(htmlDocument.Images.Where(e => !e.HasAttribute("data-no-validate")).Select(e => (e.GetAttribute("src"), (IElement)e)))
                .Concat(htmlDocument.Scripts.Where(e => !e.HasAttribute("data-no-validate")).Select(e => (e.Source, (IElement)e)))
                .GroupBy(x => x.Item1, x => x.Item2)
                .Select(g => (GetLink(g.Key, g, baseHref, document, documentDestinationDirectory, context, absolute), (IEnumerable<IElement>)g))
                .Where(x => x.Item1 is object);
        }

        private static string GetLink(
            string link,
            IEnumerable<IElement> elements,
            string baseUri,
            Common.IDocument document,
            NormalizedPath documentDestinationDirectory,
            IAnalyzerContext context,
            bool absolute)
        {
            if (link.IsNullOrEmpty())
            {
                return null;
            }

            // Parse the link
            if (!Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out Uri uri))
            {
                AddAnalyzerResult("Invalid URI", elements, document, context);
                return null;
            }
            link = uri.ToString();

            // Double-slash link means use http:// or https:// depending on current protocol
            // The Uri class treats these as relative, but they're really absolute
            if (link.StartsWith("//"))
            {
                return absolute ? link : null;
            }

            // If this is a relative Uri, add the base path or adjust it relative to the document destination
            if (!uri.IsAbsoluteUri)
            {
                // If we have a base Url, add it and try again (which might make the link absolute if the base is absolute)
                if (!baseUri.IsNullOrEmpty())
                {
                    return GetLink(baseUri.TrimEnd('/') + "/" + link.TrimStart('/'), elements, null, document, documentDestinationDirectory, context, absolute);
                }

                // If it's a relative URI, remove the query and/or fragment
                int removeIndex = link.IndexOfAny(new char[] { '?', '#' });
                if (removeIndex > -1)
                {
                    link = link.Remove(removeIndex);
                    if (link.IsNullOrEmpty())
                    {
                        return null;
                    }
                }

                // If we've got a document destination, adjust the path relative to that
                if (!documentDestinationDirectory.IsNull && !Uri.TryCreate(documentDestinationDirectory.Combine(link).FullPath, UriKind.Relative, out uri))
                {
                    AddAnalyzerResult("Invalid relative URI", elements, document, context);
                    return null;
                }
                link = uri.ToString();
            }

            // Return the URI if it's the right type
            return uri.IsAbsoluteUri == absolute ? link : null;
        }

        protected static void AddAnalyzerResult(string message, IEnumerable<IElement> elements, Common.IDocument document, IAnalyzerContext context)
        {
            foreach (IElement element in elements)
            {
                context.AddAnalyzerResult(document, $"{message}: {element.OuterHtml}");
            }
        }
    }
}
