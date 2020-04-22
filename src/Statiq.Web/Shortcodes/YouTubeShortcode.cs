using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Web.Shortcodes
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
    public class YouTubeShortcode : EmbedShortcode
    {
        public override async Task<ShortcodeResult> ExecuteAsync(KeyValuePair<string, string>[] args, IDocument document, IExecutionContext context) =>
            await GetEmbedResultAsync("https://www.youtube.com/oembed", $"https://www.youtube.com/watch?v={args.SingleValue()}", context);
    }
}
