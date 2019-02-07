using System;
using System.Collections.Generic;
using System.Text;
using Wyam.Common.Shortcodes;

namespace Wyam.Testing.Shortcodes
{
    public class TestShortcodeCollection : Dictionary<string, Type>, IShortcodeCollection
    {
        public TestShortcodeCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public IShortcode CreateInstance(string name) => (IShortcode)Activator.CreateInstance(this[name]);

        public void Add<TShortcode>(string name)
            where TShortcode : IShortcode =>
            this[name] = typeof(TShortcode);

        public bool Contains(string name) => ContainsKey(name);

        IEnumerator<string> IEnumerable<string>.GetEnumerator() => Keys.GetEnumerator();
    }
}
