using System.Collections.Generic;
using System.IO;

namespace Wyam.Common.Shortcodes
{
    public interface IShortcodeResult
    {
        Stream Stream { get; }
        IEnumerable<KeyValuePair<string, object>> Metadata { get; }
    }
}
