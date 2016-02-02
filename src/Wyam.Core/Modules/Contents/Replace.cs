using System.Collections.Generic;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;
using System.Text.RegularExpressions;

namespace Wyam.Core.Modules.Contents
{
    /// <summary>
    /// Replaces a search string in the content of each input document with new content.
    /// </summary>
    /// <category>Content</category>
    public class Replace : ContentModule
    {
        private readonly string _search;
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


        public Replace IsRegex(RegexOptions regexOptions = RegexOptions.None)
        {
            _isRegex = true;
            _regexOptions = regexOptions;
            return this;
        }

        /// <summary>
        /// The specified modules are executed against an empty initial document and the results 
        /// replace all occurrences of the search string in every input document (possibly 
        /// creating more than one output document for each input document).
        /// </summary>
        /// <param name="search">The string to search for.</param>
        /// <param name="modules">Modules that output the content to replace the search string with.</param>
        public Replace(string search, params IModule[] modules)
            : base(modules)
        {
            _search = search;
        }

        protected override IEnumerable<IDocument> Execute(object content, IDocument input, IExecutionContext context)
        {
            if (string.IsNullOrEmpty(_search))
            {
                return new[] { input };
            }
            if (content == null)
            {
                content = string.Empty;
            }
            return new[] {
                input.Clone(
                    _isRegex ?
                        Regex.Replace(input.Content, _search, content.ToString(), _regexOptions) :
                        input.Content.Replace(_search, content.ToString())
                    )
            };
        }
    }
}
