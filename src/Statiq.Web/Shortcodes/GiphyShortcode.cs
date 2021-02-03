using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Web.Shortcodes
{
    /// <summary>
    /// Embeds a Giphy gif.
    /// </summary>
    /// <remarks>
    /// You only need the ID of the gif which can be obtained from it's URL:
    /// <code>
    /// https://giphy.com/gifs/excited-birthday-yeah-yoJC2GnSClbPOkV0eA
    /// </code>
    /// </remarks>
    /// <example>
    /// <code>
    /// &lt;?# Giphy excited-birthday-yeah-yoJC2GnSClbPOkV0eA /?&gt;
    /// </code>
    /// </example>
    /// <parameter name="Id">The ID of the gif.</parameter>
    public class GiphyShortcode : EmbedShortcode
    {
        private const string Id = nameof(Id);

        public override async Task<ShortcodeResult> ExecuteAsync(KeyValuePair<string, string>[] args, IDocument document, IExecutionContext context)
        {
            IMetadataDictionary arguments = args.ToDictionary(Id);
            arguments.RequireKeys(Id);
            return await GetEmbedResultAsync(arguments, "https://giphy.com/services/oembed", $"https://giphy.com/gifs/{arguments.GetString(Id)}", context);
        }
    }
}
