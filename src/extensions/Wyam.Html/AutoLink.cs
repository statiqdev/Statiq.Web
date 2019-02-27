using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using Wyam.Common.Util;
using AngleSharp;

namespace Wyam.Html
{
    /// <summary>
    /// Replaces occurrences of specified strings with HTML links.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This module is smart enough to only look in specified HTML
    /// elements (p by default). You can supply an alternate query selector to
    /// narrow the search scope to different container elements or to those elements that contain
    /// (or don't contain) a CSS class, etc. It also won't generate an HTML link if the replacement
    /// text is already found in another link.
    /// </para>
    /// <para>
    /// Note that because this module parses the document
    /// content as standards-compliant HTML and outputs the formatted post-parsed DOM, you should
    /// only place this module after all other template processing has been performed.
    /// </para>
    /// </remarks>
    /// <category>Content</category>
    public class AutoLink : IModule
    {
        // Key = text to replace, Value = url
        private readonly ConfigHelper<IDictionary<string, string>> _links;
        private readonly IDictionary<string, string> _extraLinks = new Dictionary<string, string>();
        private string _querySelector = "p";
        private bool _matchOnlyWholeWord = false;
        private List<char> _startWordSeparators = new List<char>();
        private List<char> _endWordSeparators = new List<char>();

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
        /// Specifies a dictionary of link mappings given an <see cref="IExecutionContext"/>. The return value is expected
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
        /// Specifies a dictionary of link mappings given an <see cref="IDocument"/> and <see cref="IExecutionContext"/>. The return
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
        /// By default whole words are determined by testing for white space.
        /// </summary>
        /// <param name="matchOnlyWholeWord">If set to <c>true</c> the module will only insert links at word boundaries.</param>
        /// <returns>The current instance.</returns>
        public AutoLink WithMatchOnlyWholeWord(bool matchOnlyWholeWord = true)
        {
            _matchOnlyWholeWord = matchOnlyWholeWord;
            return this;
        }

        /// <summary>
        /// Adds additional word separator characters when limiting matches to whole words only.
        /// These additional characters are in addition to the default of splitting words at white space.
        /// </summary>
        /// <param name="wordSeparators">Additional word separators that should be considered for the start and end of a word.</param>
        /// <returns>The current instance.</returns>
        public AutoLink WithWordSeparators(params char[] wordSeparators)
        {
            _startWordSeparators.AddRange(wordSeparators);
            _endWordSeparators.AddRange(wordSeparators);
            return this;
        }

        /// <summary>
        /// Adds additional start word separator characters when limiting matches to whole words only.
        /// These additional characters are in addition to the default of splitting words at white space.
        /// </summary>
        /// <param name="startWordSeparators">Additional word separators that should be considered for the start of a word.</param>
        /// <returns>The current instance.</returns>
        public AutoLink WithStartWordSeparators(params char[] startWordSeparators)
        {
            _startWordSeparators.AddRange(startWordSeparators);
            return this;
        }

        /// <summary>
        /// Adds additional end word separator characters when limiting matches to whole words only.
        /// These additional characters are in addition to the default of splitting words at white space.
        /// </summary>
        /// <param name="endWordSeparators">Additional word separators that should be considered for the end of a word.</param>
        /// <returns>The current instance.</returns>
        public AutoLink WithEndWordSeparators(params char[] endWordSeparators)
        {
            _endWordSeparators.AddRange(endWordSeparators);
            return this;
        }

        /// <inheritdoc />
        public IEnumerable<Common.Documents.IDocument> Execute(IReadOnlyList<Common.Documents.IDocument> inputs, IExecutionContext context)
        {
            HtmlParser parser = new HtmlParser();
            return inputs.AsParallel().Select(context, input =>
            {
                try
                {
                    // Get the links and HTML decode the keys (if they're encoded) since the text nodes are decoded
                    IDictionary<string, string> links = _links.GetValue(input, context, v => _extraLinks
                        .Concat(v.Where(l => !_extraLinks.ContainsKey(l.Key)))
                        .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                        .ToDictionary(z => WebUtility.HtmlDecode(z.Key), z => $"<a href=\"{z.Value}\">{z.Key}</a>"));

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
                            if (ReplaceStrings(text, links, out string newText))
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

                        Stream contentStream = context.GetContentStream();
                        using (StreamWriter writer = contentStream.GetWriter())
                        {
                            htmlDocument.ToHtml(writer, ProcessingInstructionFormatter.Instance);
                            writer.Flush();
                            return context.GetDocument(input, contentStream);
                        }
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

        private bool ReplaceStrings(IText textNode, IDictionary<string, string> map, out string newText)
        {
            string text = textNode.Text;
            SubstringSegment originalSegment = new SubstringSegment(0, text.Length);
            List<Segment> segments = new List<Segment>()
            {
                originalSegment
            };

            // Perform replacements
            foreach (KeyValuePair<string, string> kvp in map.OrderByDescending(x => x.Key.Length))
            {
                int c = 0;
                while (c < segments.Count)
                {
                    int index = segments[c].IndexOf(kvp.Key, 0, ref text);
                    while (index >= 0)
                    {
                        if (CheckWordSeparators(
                            ref text,
                            segments[c].StartIndex,
                            segments[c].StartIndex + segments[c].Length - 1,
                            index,
                            index + kvp.Key.Length - 1))
                        {
                            // Insert the new content
                            Segment replacing = segments[c];
                            segments[c] = new ReplacedSegment(kvp.Value);

                            // Insert segment before the match
                            if (index > replacing.StartIndex)
                            {
                                segments.Insert(c, new SubstringSegment(replacing.StartIndex, index - replacing.StartIndex));
                                c++;
                            }

                            // Insert segment after the match
                            int startIndex = index + kvp.Key.Length;
                            int endIndex = replacing.StartIndex + replacing.Length;
                            if (startIndex < endIndex)
                            {
                                Segment segment = new SubstringSegment(startIndex, endIndex - startIndex);
                                if (c + 1 == segments.Count)
                                {
                                    segments.Add(segment);
                                }
                                else
                                {
                                    segments.Insert(c + 1, segment);
                                }
                            }

                            // Go to the next segment
                            index = -1;
                        }
                        else
                        {
                            index = segments[c].IndexOf(kvp.Key, index - segments[c].StartIndex + 1, ref text);
                        }
                    }
                    c++;
                }
            }

            // Join and escape non-replaced content
            if (segments.Count > 1 || (segments.Count == 1 && segments[0] != originalSegment))
            {
                newText = string.Concat(segments.Select(x => x.GetText(ref text)));
                return true;
            }

            newText = null;
            return false;
        }

        private bool CheckWordSeparators(ref string stringToCheck, int substringStartIndex, int substringEndIndex, int matchStartIndex, int matchEndIndex)
        {
            if (_matchOnlyWholeWord)
            {
                return (matchStartIndex <= substringStartIndex || char.IsWhiteSpace(stringToCheck[matchStartIndex - 1]) || _startWordSeparators.Contains(stringToCheck[matchStartIndex - 1]))
                    && (matchEndIndex + 1 > substringEndIndex || char.IsWhiteSpace(stringToCheck[matchEndIndex + 1]) || _endWordSeparators.Contains(stringToCheck[matchEndIndex + 1]));
            }
            return true;
        }

        private abstract class Segment
        {
            public int StartIndex { get; protected set; } = -1;
            public int Length { get; protected set; } = -1;
            public virtual int IndexOf(string value, int startIndex, ref string search) => -1;
            public abstract string GetText(ref string text);
        }

        private class SubstringSegment : Segment
        {
            public SubstringSegment(int startIndex, int length)
            {
                StartIndex = startIndex;
                Length = length;
            }

            public override int IndexOf(string value, int startIndex, ref string search) =>
                search.IndexOf(value, StartIndex + startIndex, Length - startIndex);

            public override string GetText(ref string text) =>
                WebUtility.HtmlEncode(text.Substring(StartIndex, Length));
        }

        private class ReplacedSegment : Segment
        {
            private readonly string _text;

            public ReplacedSegment(string text)
            {
                _text = text;
            }

            public override string GetText(ref string text) => _text;
        }
    }
}
