using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wyam.Core.Shortcodes
{
    internal class Delimiter
    {
        private static readonly char[] Styles = new[] { '%', '!', '*', '#' };

        private readonly bool _start;

        public string Text { get; }

        private int _index;

        public Delimiter(string text, bool start)
        {
            Text = text;
            _start = start;
        }

        public bool Locate(char c, bool closing, ref char? style)
        {
            if (closing && _index == Text.Length + 1)
            {
                // Closing delimiter, check closing slash
                if (c == '/')
                {
                    _index++;
                }
                else
                {
                    _index = 0;
                }
            }
            else if ((_start && _index == Text.Length)
                && ((style == null && Styles.Contains(c)) || (style != null && c == style)))
            {
                // Start delimiter, check style char
                style = c;
                _index++;
            }
            else if ((!_start && _index == 0)
                && ((style == null && Styles.Contains(c)) || (style != null && c == style)))
            {
                // End delimiter, check style char
                style = c;
                _index++;
            }
            else if (_start && _index < Text.Length && c == Text[_index])
            {
                // Start delimiter, check text
                _index++;
            }
            else if (!_start && _index > 0 && _index - 1 < Text.Length && c == Text[_index - 1])
            {
                // Start delimiter, check text
                _index++;
            }
            else
            {
                // Didn't complete the delimiter, reset
                _index = 0;
            }

            // Did we complete the end delimiter?
            if ((closing && _index == Text.Length + 2)
                || (!closing && _index == Text.Length + 1))
            {
                _index = 0;
                return true;
            }

            return false;
        }
    }
}
