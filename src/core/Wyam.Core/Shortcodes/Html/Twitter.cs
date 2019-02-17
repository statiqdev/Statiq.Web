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
    public class Twitter : IShortcode
    {
        // Cache the HttpClient as per recommended advice. Since this is short lived we don't have to worry about DNS issues.
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };

        private bool _omitScript;

        public IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            ConvertingDictionary arguments = args.ToDictionary(
                context,
                "Id",
                "HideMedia",
                "HideThread",
                "Theme");
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
            if (_omitScript)
            {
                query.Add("omit_script=true");
            }
            string queryString = query.Count > 0 ? "?" + string.Join("&", query) : null;
            string url = $"https://publish.twitter.com/oembed?url=https://twitter.com/username/status/{arguments.String("Id")}{query}";

            // Get the oEmbed response with the HTML to embed
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Embed));
            Embed embed;
            using (Stream stream = _httpClient.GetStreamAsync(url).Result)
            {
                embed = (Embed)serializer.ReadObject(stream);
            }

            // Omit the script on the next Twitter embed
            _omitScript = true;

            return context.GetShortcodeResult(embed.Html);
        }

        [DataContract]
        public class Embed
        {
            [DataMember(Name = "html")]
            public string Html { get; set; }
        }
    }
}
