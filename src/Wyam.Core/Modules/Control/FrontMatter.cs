using System;
using System.Collections.Generic;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Core.Modules.Control
{
    /// <summary>
    /// Extracts the first part of content for each document and sends it to a child module for processing.
    /// </summary>
    /// <remarks>
    /// This module is typically used in conjunction with the Yaml module to enable putting YAML front
    /// matter in a file. First, the content of each input document is scanned for a line that consists 
    /// entirely of the delimiter character or (- by default) or the delimiter string. Once found, the 
    /// content before the delimiter is passed to the specified child modules. Any metadata from the child
    /// module output document(s) is added to the input document. Note that if the child modules result 
    /// in more than one output document, multiple clones of the input document will be made for each one. 
    /// The output document content is set to the original content without the front matter.
    /// </remarks>
    /// <category>Control</category>
    public class FrontMatter : IModule
    {
        private readonly string _delimiter;
        private bool _ignoreDelimiterOnFirstLine = true;
        private readonly bool _repeated;
        private readonly IModule[] _modules;

        /// <summary>
        /// Uses the default delimiter character and passes any front matter to the specified child modules for processing.
        /// </summary>
        /// <param name="modules">The modules to execute against the front matter.</param>
        public FrontMatter(params IModule[] modules)
        {
            _delimiter = "-";
            _repeated = true;
            _modules = modules;
        }

        /// <summary>
        /// Uses the specified delimiter string and passes any front matter to the specified child modules for processing.
        /// </summary>
        /// <param name="delimiter">The delimiter to use.</param>
        /// <param name="modules">The modules to execute against the front matter.</param>
        public FrontMatter(string delimiter, params IModule[] modules)
        {
            _delimiter = delimiter;
            _repeated = false;
            _modules = modules;
        }

        /// <summary>
        /// Uses the specified delimiter character and passes any front matter to the specified child modules for processing.
        /// </summary>
        /// <param name="delimiter">The delimiter to use.</param>
        /// <param name="modules">The modules to execute against the front matter.</param>
        public FrontMatter(char delimiter, params IModule[] modules)
        {
            _delimiter = new string(delimiter, 1);
            _repeated = true;
            _modules = modules;
        }

        /// <summary>
        /// Ignores the delimiter if it appears on the first line. This is useful when processing Jekyll style front matter that
        /// has the delimiter both above and below the front matter content. The default behavior is <c>true</c>.
        /// </summary>
        /// <param name="ignore">If set to <c>true</c>, ignore the delimiter if it appears on the first line.</param>
        public FrontMatter IgnoreDelimiterOnFirstLine(bool ignore = true)
        {
            _ignoreDelimiterOnFirstLine = ignore;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            foreach (IDocument input in inputs)
            {
                List<string> inputLines = input.Content.Split(new[] { '\n' }, StringSplitOptions.None).ToList();
                int delimiterLine = inputLines.FindIndex(x =>
                {
                    string trimmed = x.TrimEnd();
                    return trimmed.Length > 0 && (_repeated ? trimmed.All(y => y == _delimiter[0]) : trimmed == _delimiter);
                });
                int startLine = 0;
                if (delimiterLine == 0 && _ignoreDelimiterOnFirstLine)
                {
                    startLine = 1;
                    delimiterLine = inputLines.FindIndex(1, x =>
                    {
                        string trimmed = x.TrimEnd();
                        return trimmed.Length > 0 && (_repeated ? trimmed.All(y => y == _delimiter[0]) : trimmed == _delimiter);
                    });
                }
                if (delimiterLine != -1)
                {
                    string frontMatter = string.Join("\n", inputLines.Skip(startLine).Take(delimiterLine - startLine)) + "\n";
                    inputLines.RemoveRange(0, delimiterLine + 1);
                    string content = string.Join("\n", inputLines);
                    foreach (IDocument result in context.Execute(_modules, new[] { context.GetDocument(input, frontMatter) }))
                    {
                        yield return context.GetDocument(result, content);
                    }
                }
                else
                {
                    yield return input;
                }
            }
        }
    }
}
