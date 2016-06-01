using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Html;
using AngleSharp.Parser.Html;
using Wyam.Common;
using Wyam.Common.Configuration;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;
using IDocument = Wyam.Common.Documents.IDocument;

namespace Wyam.Html
{
    /// <summary>
    /// Replaces occurrences of specified strings with HTML links.
    /// </summary>
    /// <remarks>
    /// This module is smart enough to only look in specified HTML 
    /// elements (p by default). You can supply an alternate query selector to 
    /// narrow the search scope to different container elements or to those elements that contain 
    /// (or don't contain) a CSS class, etc. It also won't generate an HTML link if the replacement 
    /// text is already found in another link.
    /// </remarks>
    /// <category>Content</category>
    public class AutoLink : IModule
    {
        // Key = text to replace, Value = url
        private readonly ConfigHelper<IDictionary<string, string>> _links;
        private readonly IDictionary<string, string> _extraLinks = new Dictionary<string, string>();
        private string _querySelector = "p";
        private bool _matchOnlyWholeWord = false;

        /// <summary>
        /// Creates the module without any initial mappings. Use <c>AddLink(...)</c> to add mappings with fluent methods.
        /// </summary>
        public AutoLink()
        {
            _links = new ConfigHelper<IDictionary<string, string>>(new Dictionary<string, string>());
        }

        /// <summary>
        /// Specifies a dictionary of link mappings. The keys specify strings to search for in the HTML content 
        /// and the values specify what should be placed in the <c>href</c> attribute. This uses the same 
        /// link mappings for all input documents.
        /// </summary>
        /// <param name="links">A dictionary of link mappings.</param>
        public AutoLink(IDictionary<string, string> links)
        {
            _links = new ConfigHelper<IDictionary<string, string>>(links ?? new Dictionary<string, string>());
        }

        /// <summary>
        /// Specifies a dictionary of link mappings given an <c>IExecutionContext</c>. The return value is expected 
        /// to be a <c>IDictionary&lt;string, string&gt;</c>. The keys specify strings to search for in the HTML content 
        /// and the values specify what should be placed in the <c>href</c> attribute. This uses the same 
        /// link mappings for all input documents.
        /// </summary>
        /// <param name="links">A delegate that returns a dictionary of link mappings.</param>
        public AutoLink(ContextConfig links)
        {
            _links = new ConfigHelper<IDictionary<string, string>>(links, new Dictionary<string, string>());
        }

        /// <summary>
        /// Specifies a dictionary of link mappings given an <c>IDocument</c> and <c>IExecutionContext</c>. The return 
        /// value is expected to be a <c>IDictionary&lt;string, string&gt;</c>. The keys specify strings to search for in the 
        /// HTML content and the values specify what should be placed in the <c>href</c> attribute. This allows you 
        /// to specify a different mapping for each input document.
        /// </summary>
        /// <param name="links">A delegate that returns a dictionary of link mappings.</param>
        public AutoLink(DocumentConfig links)
        {
            _links = new ConfigHelper<IDictionary<string, string>>(links, new Dictionary<string, string>());
        }

        /// <summary>
        /// Allows you to specify an alternate query selector.
        /// </summary>
        /// <param name="querySelector">The query selector to use.</param>
        /// <returns>The current instance.</returns>
        public AutoLink WithQuerySelector(string querySelector)
        {
            _querySelector = querySelector ?? "p";
            return this;
        }

        /// <summary>
        /// Adds an additional link to the mapping. This can be used whether or not you specify a mapping in the constructor.
        /// </summary>
        /// <param name="text">The text to search for.</param>
        /// <param name="link">The link to insert.</param>
        /// <returns>The current instance.</returns>
        public AutoLink WithLink(string text, string link)
        {
            _extraLinks[text] = link;
            return this;
        }

        /// <summary>
        /// Forces the string search to only consider whole words (it will not add a link in the middle of a word).
        /// </summary>
        /// <param name="matchOnlyWholeWord">If set to <c>true</c> the module will only insert links at work boundaries.</param>
        /// <returns>The current instance.</returns>
        public AutoLink WithMatchOnlyWholeWord(bool matchOnlyWholeWord = true)
        {
            _matchOnlyWholeWord = matchOnlyWholeWord;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            HtmlParser parser = new HtmlParser();            
            return inputs.AsParallel().Select(input =>
            {
                try
                {
                    // Get the links
                    IDictionary<string, string> links = _links.GetValue(input, context, v => _extraLinks
                        .Concat(v.Where(l => !_extraLinks.ContainsKey(l.Key)))
                        .ToDictionary(z => z.Key, z => $"<a href=\"{z.Value}\">{z.Key}</a>"));

                    // Enumerate all elements that match the query selector not already in a link element
                    List<KeyValuePair<IText, string>> replacements = new List<KeyValuePair<IText, string>>();
                    IHtmlDocument htmlDocument;
                    using (Stream stream = input.GetStream())
                    {
                        htmlDocument = parser.Parse(stream);
                    }
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
                        return context.GetDocument(input, htmlDocument.ToHtml());
                    }
                    return input;
                }
                catch (Exception ex)
                {
                    Trace.Warning("Exception while parsing HTML for {0}: {1}", input.SourceString(), ex.Message);
                    return input;
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

                if (lastNode.IsRoot && CheckAdditonalConditions(s, matchIdx, i-1))
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

        private bool CheckAdditonalConditions(string stringToCheck, int matchStartIndex, int matchEndIndex)
        {
            return !_matchOnlyWholeWord || (
                (matchEndIndex >= stringToCheck.Length -1|| !char.IsLetterOrDigit(stringToCheck[matchEndIndex+1])) 
                && (matchStartIndex - 1 < 0 || !char.IsLetterOrDigit(stringToCheck[matchStartIndex - 1]))
                );
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
