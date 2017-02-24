using System.Collections.Generic;
using System.Text.RegularExpressions;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;

namespace Wyam.Core.Modules.Contents
{
    /// <summary>
    /// Replaces a search string in the specified content with the content of input documents.
    /// </summary>
    /// <remarks>
    /// This is sort of like the inverse of the Replace module and is very useful for simple
    /// template substitution.
    /// </remarks>
    /// <category>Content</category>
    public class ReplaceIn : ContentModule
    {
        private readonly string _search;
        private bool _isRegex;
        private RegexOptions _regexOptions = RegexOptions.None;

        /// <summary>
        /// Replaces all occurrences of the search string in the string value of the
        /// specified object with the content of each input document.
        /// </summary>
        /// <param name="search">The string to search for.</param>
        /// <param name="content">The content within which to search for the search string.</param>
        public ReplaceIn(string search, object content)
            : base(content)
        {
            _search = search;
        }

        /// <summary>
        /// Replaces all occurrences of the search string in the string value of the
        /// returned object with the content of each input document. This allows you to
        /// specify different content depending on the execution context.
        /// </summary>
        /// <param name="search">The string to search for.</param>
        /// <param name="content">A delegate that returns the content within which
        /// to search for the search string.</param>
        public ReplaceIn(string search, ContextConfig content)
            : base(content)
        {
            _search = search;
        }

        /// <summary>
        /// Replaces all occurrences of the search string in the string value of the returned
        /// object with the content of each input document. This allows you to specify different
        /// content for each document depending on the input document.
        /// </summary>
        /// <param name="search">The string to search for.</param>
        /// <param name="content">A delegate that returns the content within which
        /// to search for the search string.</param>
        public ReplaceIn(string search, DocumentConfig content)
            : base(content)
        {
            _search = search;
        }

        /// <summary>
        /// The specified modules are executed against an empty initial document and all
        /// occurrences of the search string in the resulting document content are replaced by the content of
        /// each input document (possibly creating more than one output document for each input document).
        /// </summary>
        /// <param name="search">The string to search for.</param>
        /// <param name="modules">Modules that output the content within which
        /// to search for the search string.</param>
        public ReplaceIn(string search, params IModule[] modules)
            : base(modules)
        {
            _search = search;
        }

        /// <summary>
        /// Indicates that the search string(s) should be treated as a regular expression(s)
        /// with the specified options.
        /// </summary>
        /// <param name="regexOptions">The options to use (if any).</param>
        public ReplaceIn IsRegex(RegexOptions regexOptions = RegexOptions.None)
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
                return new[] { context.GetDocument(input, content.ToString()) };
            }
            return new[] { context.GetDocument(input,
                _isRegex ?
                    Regex.Replace(input.Content, _search, content.ToString(), _regexOptions) :
                    content.ToString().Replace(_search, input.Content)
                )
            };
        }
    }
}
