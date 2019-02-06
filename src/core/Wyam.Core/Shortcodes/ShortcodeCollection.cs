using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Wyam.Common.Shortcodes;

namespace Wyam.Core.Shortcodes
{
    internal class ShortcodeCollection : IShortcodeCollection
    {
        private readonly Dictionary<string, Type> _shortcodes =
            new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        public int Count => _shortcodes.Count;

        public bool Contains(string name) => _shortcodes.ContainsKey(name);

        public IEnumerator<string> GetEnumerator() => _shortcodes.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
