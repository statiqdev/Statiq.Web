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
        private readonly Func<IExecutionContext, IDictionary<string, string>> _contextLinks;
        private readonly Func<IDocument, IExecutionContext, IDictionary<string, string>> _documentLinks;
        private string _querySelector = "p";

        public AutoLink(IDictionary<string, string> links)
        {
            _contextLinks = c => links;
        }

        public AutoLink(Func<IExecutionContext, IDictionary<string, string>> links)
        {
            if (links == null)
            {
                throw new ArgumentNullException(nameof(links));
            }

            _contextLinks = links;
        }

        public AutoLink(Func<IDocument, IExecutionContext, IDictionary<string, string>> links)
        {
            if (links == null)
            {
                throw new ArgumentNullException(nameof(links));
            }

            _documentLinks = links;
        }

        public AutoLink SetQuerySelector(string querySelector)
        {
            _querySelector = querySelector;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            HtmlParser parser = new HtmlParser();

            // Order by longest so substring matches don't take precedence
            List<KeyValuePair<string, string>> contextLinks = _contextLinks?.Invoke(context).OrderByDescending(y => y.Key.Length).ToList();

            return inputs.AsParallel().Select(x =>
            {
                try
                {
                    // If we didn't get the links from the context, get them from the document
                    List<KeyValuePair<string, string>> links = contextLinks ?? _documentLinks(x, context).OrderByDescending(y => y.Key.Length).ToList();

                    // Enumerate all elements that match the query selector not already in a link element
                    List<KeyValuePair<IText, string>> replacements = new List<KeyValuePair<IText, string>>();
                    IHtmlDocument htmlDocument = parser.Parse(x.Stream);
                    foreach (IElement element in htmlDocument.QuerySelectorAll(_querySelector).Where(t => !t.Ancestors<IHtmlAnchorElement>().Any()))
                    {
                        // Enumerate all descendant text nodes not already in a link element
                        foreach (IText text in element.Descendents().OfType<IText>().Where(t => !t.Ancestors<IHtmlAnchorElement>().Any()))
                        {
                            // Have to do this goofy double-replacement to make sure we don't end up replacing smaller substrings of larger search strings
                            Dictionary<string, string> substitutions = new Dictionary<string, string>();
                            string substiutionTemplate = "{c5335fb9-d2dd-40cb-a048-b49b8fcf6ba1";  // Just use an arbitrary GUID which should be unique enough
                            int c = 0;
                            string newText = text.Text;
                            foreach (KeyValuePair<string, string> link in links)
                            {
                                string linkSubstitution = substiutionTemplate + c + "}";
                                string replacedText = newText.Replace(link.Key, linkSubstitution);
                                if (replacedText != newText)
                                {
                                    substitutions[linkSubstitution] = $"<a href=\"{link.Value}\">{link.Key}</a>";
                                    newText = replacedText;
                                }
                                c++;
                            }
                            if (substitutions.Count > 0)
                            {
                                foreach (KeyValuePair<string, string> substitution in substitutions)
                                {
                                    newText = newText.Replace(substitution.Key, substitution.Value);
                                }
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
