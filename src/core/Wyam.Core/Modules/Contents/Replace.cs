using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.Core.Modules.Contents
{
    /// <summary>
    /// Replaces a search string in the content of each input document with new content.
    /// </summary>
    /// <category>Content</category>
    public class Replace : ContentModule
    {
        private readonly string _search;
        private readonly Func<Match, object> _contentFinder;
        private bool _isRegex;
        private RegexOptions _regexOptions = RegexOptions.None;

        /// <summary>
        /// Replaces all occurrences of the search string in every input document
        /// with the string value of the specified object.
        /// </summary>
        /// <param name="search">The string to search for.</param>
        /// <param name="content">The content to replace the search string with.</param>
        public Replace(string search, object content)
            : base(content)
        {
            _search = search;
        }

        /// <summary>
        /// Replaces all occurrences of the search string in every input document with the
        /// string value of the returned object. This allows you to specify different content
        /// depending on the execution context.
        /// </summary>
        /// <param name="search">The string to search for.</param>
        /// <param name="content">A delegate that returns the content to replace the search string with.</param>
        public Replace(string search, ContextConfig content)
            : base(content)
        {
            _search = search;
        }

        /// <summary>
        /// Replaces all occurrences of the search string in every input document with the
        /// string value of the returned object. This allows you to specify different content
        /// for each document depending on the input document.
        /// </summary>
        /// <param name="search">The string to search for.</param>
        /// <param name="content">A delegate that returns the content to replace the search string with.</param>
        public Replace(string search, DocumentConfig content)
            : base(content)
        {
            _search = search;
        }

        /// <summary>
        /// The specified modules are executed against an empty initial document and the resulting
        /// document content replaces all occurrences of the search string in every input document
        /// (possibly creating more than one output document for each input document).
        /// </summary>
        /// <param name="search">The string to search for.</param>
        /// <param name="modules">Modules that output the content to replace the search string with.</param>
        public Replace(string search, params IModule[] modules)
            : base(modules)
        {
            _search = search;
        }

        /// <summary>
        /// Replaces all occurrences of the search string in every input document
        /// with the string value of the objects returned by the delegate. The delegate will be called
        /// for each Match in the supplied regular expression.
        /// </summary>
        /// <param name="search">The string to search for (interpreted as a regular expression).</param>
        /// <param name="contentFinder">A delegate that returns the content to replace the match.</param>
        public Replace(string search, Func<Match, object> contentFinder)
            : base(null as object)
        {
            _search = search;
            _contentFinder = contentFinder;
            _isRegex = true;
        }

        /// <summary>
        /// Indicates that the search string(s) should be treated as a regular expression(s)
        /// with the specified options.
        /// </summary>
        /// <param name="regexOptions">The options to use (if any).</param>
        public Replace IsRegex(RegexOptions regexOptions = RegexOptions.None)
        {
            _isRegex = true;
            _regexOptions = regexOptions;
            return this;
        }

        protected override IEnumerable<IDocument> Execute(object content, IDocument input, IExecutionContext context)
        {
            if (content == null)
            {
                content = string.Empty;
            }
            if (string.IsNullOrEmpty(_search))
            {
                return new[] { input };
            }
            if (_contentFinder != null)
            {
                Match match = Regex.Match(input.Content, _search, _regexOptions);
                if (!match.Success)
                {
                    return new[] {input};
                }
                string currentDocumentContent = input.Content;
                while (match.Success)
                {
                    object result = _contentFinder(match);
                    currentDocumentContent = currentDocumentContent.Remove(match.Index, match.Length);
                    currentDocumentContent = currentDocumentContent.Insert(match.Index, result.ToString());
                    match = Regex.Match(currentDocumentContent, _search, _regexOptions);
                }
                return new[] { context.GetDocument(input, context.GetContentStream(currentDocumentContent)) };
            }
            return new[]
            {
                context.GetDocument(
                    input,
                    context.GetContentStream(_isRegex ?
                        Regex.Replace(input.Content, _search, content.ToString(), _regexOptions) :
                        input.Content.Replace(_search, content.ToString())
                    ))
            };
        }
    }
}
