using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wyam.Common.Shortcodes;
using Wyam.Core.Documents;
using Wyam.Core.Util;

namespace Wyam.Core.Shortcodes
{
    internal class ShortcodeResult : IShortcodeResult
    {
        public Stream Stream { get; }

        public IEnumerable<KeyValuePair<string, object>> Metadata { get; }

        public ShortcodeResult(Stream stream, IEnumerable<KeyValuePair<string, object>> metadata)
        {
            if (stream?.CanRead == false)
            {
                throw new ArgumentException("Shortcode stream must support reading.", nameof(stream));
            }

            Stream = stream == null
                ? null
                : (stream.CanSeek ? stream : new SeekableStream(stream, true));
            Metadata = metadata;
        }
    }
}
