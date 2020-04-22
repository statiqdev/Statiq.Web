using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Web.Shortcodes
{
    /// <summary>
    /// Calls an oEmbed endpoint and renders the embedded content.
    /// </summary>
    /// <remarks>
    /// See https://oembed.com/ for details on the oEmbed standard and available endpoints.
    /// </remarks>
    /// <example>
    /// <code>
    /// &lt;?# Embed https://codepen.io/api/oembed https://codepen.io/gingerdude/pen/JXwgdK /?&gt;
    /// </code>
    /// </example>
    /// <parameter name="Endpoint">The oEmbed endpoint.</parameter>
    /// <parameter name="Url">The embeded URL to fetch an embed for.</parameter>
    /// <parameter name="Format">An optional format to use ("xml" or "json").</parameter>
    public class EmbedShortcode : Shortcode
    {
        private const string Endpoint = nameof(Endpoint);
        private const string Url = nameof(Url);
        private const string Format = nameof(Format);

        public sealed override async Task<ShortcodeResult> ExecuteAsync(
            KeyValuePair<string, string>[] args,
            string content,
            IDocument document,
            IExecutionContext context) =>
            await ExecuteAsync(args, document, context);

        /// <summary>
        /// Shortcodes for specific oEmbed services should override this method and call one of the other execute helper methods.
        /// </summary>
        public virtual async Task<ShortcodeResult> ExecuteAsync(KeyValuePair<string, string>[] args, IDocument document, IExecutionContext context)
        {
            IMetadataDictionary arguments = args.ToDictionary(Endpoint, Url, Format);
            arguments.RequireKeys(Endpoint, Url);
            return await GetEmbedResultAsync(
                arguments.GetString(Endpoint),
                arguments.GetString(Url),
                arguments.ContainsKey(Format)
                    ? new string[] { $"format={arguments.GetString(Format)}" }
                    : null,
                context);
        }

        protected async Task<ShortcodeResult> GetEmbedResultAsync(string endpoint, string url, IExecutionContext context) =>
            await GetEmbedResultAsync(endpoint, url, null, context);

        protected async Task<ShortcodeResult> GetEmbedResultAsync(string endpoint, string url, IEnumerable<string> query, IExecutionContext context)
        {
            // Get the oEmbed response
            EmbedResponse embedResponse;
            using (HttpClient httpClient = context.CreateHttpClient())
            {
                string request = $"{endpoint}?url={WebUtility.UrlEncode(url)}";
                if (query != null)
                {
                    request += "&" + string.Join("&", query);
                }
                HttpResponseMessage response = await httpClient.GetAsync(request);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    context.LogError($"Received 404 not found for oEmbed at {request}");
                }
                response.EnsureSuccessStatusCode();
                if (response.Content.Headers.ContentType.MediaType == MediaTypes.Json
                    || response.Content.Headers.ContentType.MediaType == MediaTypes.Html)
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(EmbedResponse));
                    using (Stream stream = await response.Content.ReadAsStreamAsync())
                    {
                        embedResponse = (EmbedResponse)serializer.ReadObject(stream);
                    }
                }
                else if (response.Content.Headers.ContentType.MediaType == MediaTypes.Xml)
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(EmbedResponse));
                    using (Stream stream = await response.Content.ReadAsStreamAsync())
                    {
                        embedResponse = (EmbedResponse)serializer.ReadObject(stream);
                    }
                }
                else
                {
                    throw new InvalidDataException("Unknown content type for oEmbed response");
                }
            }

            // Switch based on type
            if (!string.IsNullOrEmpty(embedResponse.Html))
            {
                return embedResponse.Html;
            }
            else if (embedResponse.Type == "photo")
            {
                if (string.IsNullOrEmpty(embedResponse.Url)
                    || string.IsNullOrEmpty(embedResponse.Width)
                    || string.IsNullOrEmpty(embedResponse.Height))
                {
                    throw new InvalidDataException("Did not receive required oEmbed values for image type");
                }
                return $"<img src=\"{embedResponse.Url}\" width=\"{embedResponse.Width}\" height=\"{embedResponse.Height}\" />";
            }
            else if (embedResponse.Type == "link")
            {
                if (!string.IsNullOrEmpty(embedResponse.Title))
                {
                    return $"<a href=\"{url}\">{embedResponse.Title}</a>";
                }
                return $"<a href=\"{url}\">{url}</a>";
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
