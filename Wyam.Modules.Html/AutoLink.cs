using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using Wyam.Common;
using IDocument = Wyam.Common.IDocument;

namespace Wyam.Modules.Html
{
    public class AutoLink : IModule
    {
        // Key = text to replace, Value = url
        private readonly Func<IDocument, IDictionary<string, string>> _links;
        private string _querySelector = "p";

        public AutoLink(IDictionary<string, string> links)
        {
            _links = x => links;
        }

        public AutoLink(Func<IDocument, IDictionary<string, string>> links)
        {
            _links = links;
        }

        public AutoLink SetQuerySelector(string querySelector)
        {
            _querySelector = querySelector;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            HtmlParser parser = new HtmlParser();
            return inputs.Select(x =>
            {
                IDictionary<string, string> links = _links(x);
                try
                {
                    // Enumerate all elements that match the query selector not already in a link element
                    List<KeyValuePair<IText, string>> replacements = new List<KeyValuePair<IText, string>>();
                    IHtmlDocument htmlDocument = parser.Parse(x.Stream);
                    foreach (IElement element in htmlDocument.QuerySelectorAll(_querySelector)
                        .Where(e => e.Ancestors().All(a => a.NodeName != "a")))
                    {
                        // Enumerate all descendant text nodes not already in a link element
                        foreach (IText text in element.Descendents().OfType<IText>()
                            .Where(t => t.Ancestors().All(a => a.NodeName != "a")))
                        {
                            string newText = text.Text;
                            foreach (KeyValuePair<string, string> link in links)
                            {
                                newText = newText.Replace(link.Key, $"<a href=\"{link.Value}\">{link.Key}</a>");
                            }
                            if (newText != text.Text)
                            {
                                // Only perform replacement if the text content changed
                                replacements.Add(new KeyValuePair<IText, string>(text, newText));
                            }
                        }
                    }

                    // Perform the replacements if there were any, otherwise just return the same document
                    if (replacements.Count > 0)
                    {
                        foreach (KeyValuePair<IText, string> replacement in replacements)
                        {
                            replacement.Key.Replace(parser.ParseFragment(replacement.Value, replacement.Key.ParentElement).ToArray());
                        }
                        return x.Clone(htmlDocument.ToHtml());
                    }
                    return x;
                }
                catch (Exception ex)
                {
                    context.Trace.Warning("Exception while parsing HTML for {0}: {1}", x.Source, ex.Message);
                    return x;
                }
            });
        }
    }
}
