using System;
using System.Collections.Generic;
using System.Text;

namespace Wyam.Common.Shortcodes
{
    public class ShortcodeParserException : Exception
    {
        public ShortcodeParserException(string message)
            : base(message)
        {
        }
    }
}
