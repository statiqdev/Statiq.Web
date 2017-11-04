using System;
using System.Collections.Generic;
using System.Linq;

namespace Wyam.Common.Util
{
    /// <summary>
    /// Splits a string based on a delimiter.
    /// </summary>
    /// <remarks>
    /// This class is used to split some content on a deliimiter.
    /// It is extracted from the FrontMatter module.
    /// </remarks>
    public class Splitter
    {
        private readonly string _delimiter;
        private readonly bool _repeated;
        private bool _ignoreDelimiterOnFirstLine = true;

        /// <summary>
        /// Constructs a Splitter instance with the specified delimiter.
        /// </summary>
        /// <param name="delimiter">The delimiter to use.</param>
        /// <param name="repeated">True if the delimiter will be repeated. Defaults to false.</param>
        public Splitter(string delimiter, bool repeated = false)
        {
            _delimiter = delimiter;
            _repeated = repeated;
        }

        /// <summary>
        /// Ignores the delimiter if it appears on the first line. This is useful when processing Jekyll style front matter that
        /// has the delimiter both above and below the front matter content. The default behavior is <c>true</c>.
        /// </summary>
        /// <param name="ignore">If set to <c>true</c>, ignore the delimiter if it appears on the first line.</param>
        /// <returns>The current Splitter instance.</returns>
        public Splitter IgnoreDelimiterOnFirstLine(bool ignore = true)
        {
            _ignoreDelimiterOnFirstLine = ignore;
            return this;
        }

        /// <summary>
        /// Split the input.
        /// </summary>
        /// <param name="input">The input to split.</param>
        /// <returns>A list of the split sections.</returns>
        public List<string> Split(string input)
        {
            List<string> inputLines = input.Split(new[] { '\n' }, StringSplitOptions.None).ToList();
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

                return new List<string>() { frontMatter, content };
            }
            else
            {
                return new List<string>() { input };
            }
        }
    }
}
