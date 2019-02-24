using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Shortcodes;
using Wyam.Common.Util;

namespace Wyam.Core.Shortcodes.Html
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
    public class Twitter : Embed
    {
        private bool _omitScript;

        /// <inheritdoc />
        public override IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            ConvertingDictionary arguments = args.ToDictionary(
                context,
                "Id",
                "HideMedia",
                "HideThread",
                "Theme",
                "OmitScript");
            arguments.RequireKeys("Id");

            // Create the url
            List<string> query = new List<string>();
            if (arguments.Bool("HideMedia"))
            {
                query.Add("hide_media=true");
            }
            if (arguments.Bool("HideThread"))
            {
                query.Add("hide_thread=true");
            }
            if (arguments.ContainsKey("Theme"))
            {
                query.Add($"theme={arguments.String("theme")}");
            }
            if (_omitScript || arguments.Bool("OmitScript"))
            {
                query.Add("omit_script=true");
            }

            // Omit the script on the next Twitter embed
            _omitScript = true;

            return Execute("https://publish.twitter.com/oembed", $"https://twitter.com/username/status/{arguments.String("Id")}", query, context);
        }
    }
}
