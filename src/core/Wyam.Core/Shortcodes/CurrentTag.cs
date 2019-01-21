using System;
using System.Collections.Generic;
using System.Text;

namespace Wyam.Core.Shortcodes
{
    internal class CurrentTag
    {
        public StringBuilder Content { get; } = new StringBuilder();
        public int FirstIndex { get; }
        public int LastWhiteSpace { get; }
        public int LastNonWhiteSpace { get; }

        public CurrentTag(int firstIndex, int lastWhiteSpace, int lastNonWhiteSpace)
        {
            FirstIndex = firstIndex;
            LastWhiteSpace = lastWhiteSpace;
            LastNonWhiteSpace = lastNonWhiteSpace;
        }
    }
}
