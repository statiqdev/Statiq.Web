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
        public IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("Incorrect number of arguments");
            }
            if (args[0].Key != null || args[0].Value == null)
            {
                throw new ArgumentException("Incorrect arguments");
            }

            return context.GetShortcodeResult(document.String(args[0].Value));
        }
    }
}
