using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wyam.Common.Shortcodes;

namespace Wyam.Core.Shortcodes
{
    internal class ShortcodeResult : IShortcodeResult, IDisposable
    {
        private readonly Stream _content;

        public IEnumerable<KeyValuePair<string, object>> Metadata { get; }

        public ShortcodeResult(Stream content, IEnumerable<KeyValuePair<string, object>> metadata)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            Metadata = metadata;
        }

        public void Dispose()
        {
            _content.Dispose();
        }
    }
}
