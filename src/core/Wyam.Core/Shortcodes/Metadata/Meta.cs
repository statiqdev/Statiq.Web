using System;
using System.Collections.Generic;
using System.Text;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Shortcodes;

namespace Wyam.Core.Shortcodes.Metadata
{
    /// <summary>
    /// Renders the metadata value with the given key.
    /// </summary>
    /// <remarks>
    /// This shortcode accepts a single argument value with the key of the metadata value to render.
    /// </remarks>
    public class Meta : IShortcode
    {
        public IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
            context.GetShortcodeResult(document.String(args.SingleValue()));
    }
}
