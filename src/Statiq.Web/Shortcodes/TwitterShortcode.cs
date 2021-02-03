using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Web.Shortcodes
{
    /// <summary>
    /// Renders a Tweet.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;?# Twitter 123456789 /?&gt;
    /// </code>
    /// </example>
    /// <parameter name="Id">The ID of the Tweet. This can be found at the end of the URL when you copy a link to a Tweet.</parameter>
    /// <parameter name="HideMedia">When set to <c>true</c>, links in a Tweet are not expanded to photo, video, or link previews.</parameter>
    /// <parameter name="HideThread">When set to <c>true</c>, a collapsed version of the previous Tweet in a conversation thread will not be displayed when the requested Tweet is in reply to another Tweet.</parameter>
    /// <parameter name="Theme"><c>light</c> or <c>dark</c>. When set to <c>dark</c>, the Tweet is displayed with light text over a dark background.</parameter>
    /// <parameter name="OmitScript">When set to <c>true</c>, the <c>script</c> element that contains the Twitter embed JavaScript code will not be rendered.</parameter>
    public class TwitterShortcode : EmbedShortcode
    {
        private const string Id = nameof(Id);
        private const string HideMedia = nameof(HideMedia);
        private const string HideThread = nameof(HideThread);
        private const string Theme = nameof(Theme);
        private const string OmitScript = nameof(OmitScript);

        private bool _omitScript;

        /// <inheritdoc />
        public override async Task<ShortcodeResult> ExecuteAsync(KeyValuePair<string, string>[] args, IDocument document, IExecutionContext context)
        {
            IMetadataDictionary arguments = args.ToDictionary(
                Id,
                HideMedia,
                HideThread,
                Theme,
                OmitScript);
            arguments.RequireKeys(Id);

            // Create the url
            List<string> query = new List<string>();
            if (arguments.GetBool(HideMedia))
            {
                query.Add("hide_media=true");
            }
            if (arguments.GetBool(HideThread))
            {
                query.Add("hide_thread=true");
            }
            if (arguments.ContainsKey(Theme))
            {
                query.Add($"theme={arguments.GetString(Theme)}");
            }
            if (_omitScript || arguments.GetBool(OmitScript))
            {
                query.Add("omit_script=true");
            }

            // Omit the script on the next Twitter embed
            _omitScript = true;

            return await GetEmbedResultAsync(arguments, "https://publish.twitter.com/oembed", $"https://twitter.com/username/status/{arguments.GetString(Id)}", query, context);
        }
    }
}
