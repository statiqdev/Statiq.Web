using System.Collections.Generic;

namespace Wyam.Common.Shortcodes
{
    public interface IReadOnlyShortcodeCollection : IReadOnlyCollection<string>
    {
        bool Contains(string name);

        IShortcode CreateInstance(string name);
    }
}
