using System;
using System.Collections.Generic;
using System.Text;

namespace Wyam.Core.Shortcodes
{
    internal class CurrentTag
    {
        public StringBuilder Content { get; } = new StringBuilder();
        public int FirstIndex { get; }

        public CurrentTag(int firstIndex)
        {
            FirstIndex = firstIndex;
        }
    }
}
