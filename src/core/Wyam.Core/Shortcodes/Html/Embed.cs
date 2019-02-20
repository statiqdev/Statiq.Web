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
    /// <summary>
    /// Calls an oEmbed endpoint and renders the embedded content.
    /// </summary>
    /// <remarks>
    /// See https://oembed.com/ for details on the oEmbed standard and available endpoints.
    /// </remarks>
    /// <example>
    /// <code>
    /// &lt;?# Embed http://codepen.io/api/oembed?url=https://codepen.io/gingerdude/pen/JXwgdK&quot;format=json /?&gt;
    /// </code>
    /// </example>
    /// <parameter>The URL to fetch the oEmbed response from.</parameter>
    public class Embed : IShortcode
    {
        /// <inheritdoc />
        public virtual IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            string value = args.SingleValue();

            // Get the oEmbed response
            HttpResponseMessage response = context.HttpClient.GetAsync(value).Result;
            response.EnsureSuccessStatusCode();
            EmbedResponse embedResponse;
            if (response.Content.Headers.ContentType.MediaType == "application/json"
                || response.Content.Headers.ContentType.MediaType == "text/html")
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(EmbedResponse));
                using (Stream stream = response.Content.ReadAsStreamAsync().Result)
                {
                    embedResponse = (EmbedResponse)serializer.ReadObject(stream);
                }
            }
            else if (response.Content.Headers.ContentType.MediaType == "text/xml")
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(EmbedResponse));
                using (Stream stream = response.Content.ReadAsStreamAsync().Result)
                {
                    embedResponse = (EmbedResponse)serializer.ReadObject(stream);
                }
            }
            else
            {
                throw new InvalidDataException("Unknown content type for oEmbed response");
            }

            // Switch based on type
            if (!string.IsNullOrEmpty(embedResponse.Html))
            {
                return context.GetShortcodeResult(embedResponse.Html);
            }
            else if (embedResponse.Type == "photo")
            {
                if (string.IsNullOrEmpty(embedResponse.Url)
                    || string.IsNullOrEmpty(embedResponse.Width)
                    || string.IsNullOrEmpty(embedResponse.Height))
                {
                    throw new InvalidDataException("Did not receive required oEmbed values for image type");
                }
                return context.GetShortcodeResult($"<img src=\"{embedResponse.Url}\" width=\"{embedResponse.Width}\" height=\"{embedResponse.Height}\" />");
            }
            else if (embedResponse.Type == "link")
            {
                if (!string.IsNullOrEmpty(embedResponse.Title))
                {
                    return context.GetShortcodeResult($"<a href=\"{value}\">{embedResponse.Title}</a>");
                }
                return context.GetShortcodeResult($"<a href=\"{value}\">{value}</a>");
            }

            throw new InvalidDataException("Could not determine embedded content for oEmbed response");
        }

        [DataContract(Name = "oembed", Namespace = "")]
        public class EmbedResponse
        {
            [DataMember(Name = "type")]
            public string Type { get; set; }

            [DataMember(Name = "url")]
            public string Url { get; set; }

            [DataMember(Name = "title")]
            public string Title { get; set; }

            [DataMember(Name = "width")]
            public string Width { get; set; }

            [DataMember(Name = "height")]
            public string Height { get; set; }

            [DataMember(Name = "html")]
            public string Html { get; set; }
        }
    }
}
