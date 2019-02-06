using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wyam.Core.Shortcodes
{
    internal class Delimiter
    {
        private readonly bool _start;

        public string Text { get; }

        private int _index;

        public Delimiter(string text, bool start)
        {
            Text = text;
            _start = start;
        }

        public bool Locate(char c, bool isclosing)
        {
            if (isclosing && _index == Text.Length)
            {
                // Closing tag, check closing slash
                if (c == '/')
                {
                    _index++;
                }
                else
                {
                    _index = 0;
                }
            }
            else if (_index < Text.Length && c == Text[_index])
            {
                // Check delimiter char
                _index++;
            }
            else
            {
                // Didn't complete the delimiter, reset
                _index = 0;
            }

            // Did we complete the end delimiter?
            if ((isclosing && _index == Text.Length + 1)
                || (!isclosing && _index == Text.Length))
            {
                _index = 0;
                return true;
            }

            return false;
        }
    }
}
