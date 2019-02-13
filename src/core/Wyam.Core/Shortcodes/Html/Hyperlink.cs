using System;
using System.Collections.Generic;
using System.Text;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Shortcodes;
using Wyam.Common.Util;

namespace Wyam.Core.Shortcodes.Html
{
    public class Hyperlink : IShortcode
    {
        public IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
            context.GetShortcodeResult($"<a href=\"{Link.GetLink(context, args)}\">{content}</a>");
    }
}
