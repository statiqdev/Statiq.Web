using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wyam.Modules.CodeAnalysis
{
    /// <summary>
    /// Provides an API similar to <see cref="StringBuilder"/> that can be used for building
    /// strings made of unbreakable segments with specified breakpoints for wrapping.
    /// </summary>
    internal class WrappingStringBuilder
    {
        private readonly StringBuilder _masterBuilder 
            = new StringBuilder();

        // value, wrapBefore, prefixContent
        private readonly List<Tuple<string, bool, bool>> _segments 
            = new List<Tuple<string, bool, bool>>();

        private readonly int _maxLineLength;

        public string NewLinePrefix { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WrappingStringBuilder" /> class.
        /// </summary>
        /// <param name="maxLineLength">Maximum length of each line (though a line may be longer if there is no suitable breakpoint).</param>
        /// <param name="newLinePrefix">The prefix to use for all new lines.</param>
        /// <exception cref="System.ArgumentException"><paramref name="maxLineLength"/> is less than 1.</exception>
        public WrappingStringBuilder(int maxLineLength, string newLinePrefix = null)
        {
            if (maxLineLength < 1)
            {
                throw new ArgumentException(nameof(maxLineLength) 
                    + " must be greater than 0.", nameof(maxLineLength));
            }
            _maxLineLength = maxLineLength;
            NewLinePrefix = newLinePrefix;
        }

        /// <summary>
        /// Gets the length of the string.
        /// </summary>
        /// <value>
        /// The length of the string.
        /// </value>
        // Exclude the prefix content for the next line if there's nothing after it 
        public int Length => _masterBuilder.Length 
            + (_segments.Count == 1 && _segments[0].Item3 ? 0 : _segments.Sum(x => x.Item1.Length));

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            // Exclude the prefix content for the next line if there's nothing after it
            return _masterBuilder + (_segments.Count == 1 && _segments[0].Item3 
                ? string.Empty : string.Join("", _segments.Select(x => x.Item1)));
        }

        public WrappingStringBuilder Append(string value, bool wrapBefore = false)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value == string.Empty)
            {
                return this;
            }

            // Check if we need to wrap
            if (_segments.Count > 0 && _segments.Sum(x => x.Item1.Length) + value.Length > _maxLineLength)
            {
                // If this isn't a breakpoint, we need to wrap at the previous breakpoint
                // (if there is one) otherwise, we can just wrap the entire previous line
                int wrapAt = wrapBefore ? _segments.Count : _segments.FindLastIndex(x => x.Item2);
                if (wrapAt > 0)
                {
                    // Found one, wrap it around
                    _masterBuilder.AppendLine(
                        string.Join("", _segments.Take(wrapAt).Select(x => x.Item1)));
                    _segments.RemoveRange(0, wrapAt);
                    if (!string.IsNullOrEmpty(NewLinePrefix))
                    {
                        _segments.Insert(0, Tuple.Create(NewLinePrefix, false, true));
                    }
                }
            }

            // Append the new segment
            _segments.Add(Tuple.Create(value, wrapBefore, false));

            return this;
        }

        public WrappingStringBuilder AppendLine(string value, bool wrapBefore = false)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            // Append this string (and wrap if needed) then wrap again if anything is left
            Append(value, wrapBefore);
            if (_segments.Count > 0)
            {
                _masterBuilder.AppendLine(
                    string.Join("", _segments.Select(x => x.Item1)));
                _segments.Clear();
                if (!string.IsNullOrEmpty(NewLinePrefix))
                {
                    _segments.Add(Tuple.Create(NewLinePrefix, false, true));
                }
            }

            return this;
        }

        public WrappingStringBuilder AppendLine()
        {
            return AppendLine(string.Empty);
        }
    }
}
