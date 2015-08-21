using System;
using System.CodeDom;
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
        private readonly IDictionary<string, string> _extraLinks = new Dictionary<string, string>();
        private string _querySelector = "p";


        public AutoLink()
        {
            _contextLinks = c => new Dictionary<string, string>();
        }

        public AutoLink(IDictionary<string, string> links)
        {
            if (links == null)
            {
                throw new ArgumentNullException(nameof(links));
            }

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
            if (querySelector == null)
            {
                throw new ArgumentNullException(nameof(querySelector));
            }

            _querySelector = querySelector;
            return this;
        }

        public AutoLink AddLink(string text, string link)
        {
            _extraLinks[text] = link;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            HtmlParser parser = new HtmlParser();

            // Order by longest so substring matches don't take precedence
            IDictionary<string, string> links = null;
            IDictionary<string, string> contextLinks = _contextLinks?.Invoke(context);
            if (contextLinks != null)
            {
                links = _extraLinks
                    .Concat(contextLinks.Where(l => !_extraLinks.ContainsKey(l.Key)))
                    .ToDictionary(z => z.Key, z => $"<a href=\"{z.Value}\">{z.Key}</a>");
            }

            return inputs.AsParallel().Select(x =>
            {
                try
                {
                    // If we didn't get the links from the context, get them from the document
                    if (links == null)
                    {
                        IDictionary<string, string> documentLinks = _documentLinks(x, context);
                        links = _extraLinks
                            .Concat(documentLinks.Where(l => !_extraLinks.ContainsKey(l.Key)))
                            .ToDictionary(z => z.Key, z => $"<a href=\"{z.Value}\">{z.Key}</a>");
                    }

                    // Enumerate all elements that match the query selector not already in a link element
                    List<KeyValuePair<IText, string>> replacements = new List<KeyValuePair<IText, string>>();
                    IHtmlDocument htmlDocument = parser.Parse(x.Stream);
                    foreach (IElement element in htmlDocument.QuerySelectorAll(_querySelector).Where(t => !t.Ancestors<IHtmlAnchorElement>().Any()))
                    {
                        // Enumerate all descendant text nodes not already in a link element
                        foreach (IText text in element.Descendents().OfType<IText>().Where(t => !t.Ancestors<IHtmlAnchorElement>().Any()))
                        {
                            string newText = ReplaceStrings(text.Text, links);
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

        private string ReplaceStrings(string s, IDictionary<string, string> map)
        {
            Trie<char> lookup = new Trie<char>(map.Keys);
            StringBuilder builder = new StringBuilder();
            int lastIdx = -1;
            Trie<char>.Node lastNode = lookup.Root;
            int matchIdx = -1;
            HashSet<Trie<char>.Node> badMatches = new HashSet<Trie<char>.Node>();
            for (int i = 0; i < s.Length + 1; i++)
            {
                if (i < s.Length)
                {
                    char chr = s[i];
                    if (lastNode.HasNext(chr))
                    {
                        // Partial match
                        Trie<char>.Node matchNode = lastNode.Children[chr];
                        if (!badMatches.Contains(matchNode))
                        {
                            lastNode = matchNode;
                            if (matchIdx == -1)
                            {
                                matchIdx = i;
                            }
                            continue;
                        }
                    }
                }

                if (lastNode.IsRoot)
                {
                    // Complete match
                    string key = new string(lastNode.Cumulative.ToArray());
                    builder.Append(map[key]);
                    lastIdx = i - 1;
                }
                else
                {
                    // No match
                    if (matchIdx != -1)
                    {
                        // Backtrack to the last match start and don't consider this match
                        i = matchIdx - 1;
                        matchIdx = -1;
                        badMatches.Add(lastNode);
                        lastNode = lookup.Root;
                        continue;
                    }
                    builder.Append(i < s.Length ? s.Substring(lastIdx + 1, i - lastIdx) : s.Substring(lastIdx + 1));
                    lastIdx = i;
                }
                badMatches.Clear();
                matchIdx = -1;
                lastNode = lookup.Root;
            }
            return builder.ToString();
        }

        private class Trie<T> where T : IComparable<T>
        {
            public Node Root { get; }

            public Trie(IEnumerable<IEnumerable<T>> elems)
            {
                Root = new Node(new T[0]);
                foreach (var elem in elems)
                {
                    LoadSingle(elem);
                }
            }

            private void LoadSingle(IEnumerable<T> word)
            {
                Node lastNode = Root;
                foreach (var chr in word)
                {
                    Node node;
                    if (!lastNode.Children.TryGetValue(chr, out node))
                    {
                        node = new Node(lastNode.Cumulative.Concat(new[] { chr }));
                        lastNode.Children[chr] = node;
                    }
                    lastNode = node;
                }
                lastNode.IsRoot = true;
            }

            public class Node
            {
                public IEnumerable<T> Cumulative { get; }
                public bool IsRoot { get; set; }
                public IDictionary<T, Node> Children { get; }

                public bool HasNext(T elem)
                {
                    return Children.Keys.Any(k => k.Equals(elem));
                }

                public Node(IEnumerable<T> cumulative)
                {
                    Cumulative = cumulative;
                    Children = new Dictionary<T, Node>();
                }
            }
        }
    }
}
