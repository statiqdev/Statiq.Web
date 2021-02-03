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
    /// <parameter name="Id">The ID of the video.</parameter>
    public class YouTubeShortcode : EmbedShortcode
    {
        private const string Id = nameof(Id);

        public override async Task<ShortcodeResult> ExecuteAsync(KeyValuePair<string, string>[] args, IDocument document, IExecutionContext context)
        {
            IMetadataDictionary arguments = args.ToDictionary(Id);
            arguments.RequireKeys(Id);
            return await GetEmbedResultAsync(arguments, "https://www.youtube.com/oembed", $"https://www.youtube.com/watch?v={arguments.GetString(Id)}", context);
        }
    }
}
