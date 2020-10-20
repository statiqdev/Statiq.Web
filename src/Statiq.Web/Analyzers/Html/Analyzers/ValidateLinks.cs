using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Statiq.Common;
using Statiq.Html;

namespace Statiq.Web
{
    public abstract class ValidateLinks : HtmlAnalyzer
    {
        // Get <a>, <link>, <image>, and <script> links
        protected static IEnumerable<(Uri, IEnumerable<IElement>)> GetLinks(
            IHtmlDocument htmlDocument,
            Common.IDocument document,
            IAnalyzerContext context,
            bool absolute) =>
            htmlDocument.Links.Where(e => !e.HasAttribute("data-no-validate")).Select(e => (e.GetAttribute("href"), (IElement)e))
                .Concat(htmlDocument.GetElementsByTagName("link").Where(e => e.HasAttribute("href") && !e.HasAttribute("data-no-validate")).Select(e => (e.GetAttribute("href"), (IElement)e)))
                .Concat(htmlDocument.Images.Where(e => !e.HasAttribute("data-no-validate")).Select(e => (e.GetAttribute("src"), (IElement)e)))
                .Concat(htmlDocument.Scripts.Where(e => !e.HasAttribute("data-no-validate")).Select(e => (e.Source, (IElement)e)))
                .GroupBy(x => x.Item1, x => x.Item2)
                .Select(g => (GetLink(g.Key, g, htmlDocument.BaseUrl, document, context, absolute), (IEnumerable<IElement>)g))
                .Where(x => x.Item1 is object);

        private static Uri GetLink(
            string link,
            IEnumerable<IElement> elements,
            AngleSharp.Url baseUrl,
            Common.IDocument document,
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

            // Double-slash link means use http:// or https:// depending on current protocol
            // The Uri class treats these as relative, but they're really absolute
            if (uri.ToString().StartsWith("//"))
            {
                return absolute ? uri : null;
            }

            // If this is a relative Uri, add the base path or adjust it relative to the document destination
            if (!uri.IsAbsoluteUri)
            {
                // If we have a base Url, add it and try again (which might make the link absolute if the base is absolute)
                if (baseUrl is object)
                {
                    if (baseUrl.Scheme.Equals("about", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!baseUrl.Path.IsNullOrEmpty())
                        {
                            // The base URL is relative, so prepend it to this path
                            return GetLink(baseUrl.Path.TrimEnd('/') + "/" + uri.ToString().TrimStart('/'), elements, null, document, context, absolute);
                        }
                    }
                    else
                    {
                        // The base URL is absolute, so append the path to it
                        return GetLink(baseUrl.ToString().TrimEnd('/') + "/" + uri.ToString().TrimStart('/'), elements, null, document, context, absolute);
                    }
                }

                // If we've got a document destination, adjust the path relative to that
                if (!document.Destination.IsNull && !Uri.TryCreate(document.Destination.Parent.Combine(link).FullPath, UriKind.Relative, out uri))
                {
                    AddAnalyzerResult("Invalid relative URI", elements, document, context);
                    return null;
                }
            }

            // Return the URI if it's the right type
            return uri.IsAbsoluteUri == absolute ? uri : null;
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
