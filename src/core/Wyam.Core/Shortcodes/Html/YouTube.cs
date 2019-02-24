using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Shortcodes;

namespace Wyam.Core.Shortcodes.Html
{
    /// <summary>
    /// Embeds a YouTube video.
    /// </summary>
    /// <remarks>
    /// You only need the ID of the video which can be obtained from it's URL after the <c>?v=</c>:
    /// <code>
    /// https://www.youtube.com/watch?v=u5ayTqlLWQQ
    /// </code>
    /// </remarks>
    /// <example>
    /// <code>
    /// &lt;?# YouTube u5ayTqlLWQQ /?&gt;
    /// </code>
    /// </example>
    /// <parameter>The ID of the video.</parameter>
    public class YouTube : Embed
    {
        public override IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
            Execute("https://www.youtube.com/oembed", $"https://www.youtube.com/watch?v={args.SingleValue()}", context);
    }
}
