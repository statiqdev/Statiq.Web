using System;
using System.Collections.Generic;
using System.IO;
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
    public class Twitter : Embed
    {
        private bool _omitScript;

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
            string queryString = query.Count > 0 ? "?" + string.Join("&", query) : null;
            string url = $"https://publish.twitter.com/oembed?url=https://twitter.com/username/status/{arguments.String("Id")}{queryString}";

            // Omit the script on the next Twitter embed
            _omitScript = true;

            return base.Execute(
                new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, url)
                },
                content,
                document,
                context);
        }
    }
}
