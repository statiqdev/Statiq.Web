using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Web.Shortcodes
{
    /// <summary>
    /// Embeds a GitHub gist.
    /// </summary>
    /// <example>
    /// <para>
    /// Example usage:
    /// </para>
    /// <code>
    /// &lt;?# Gist 10a2f6e0186fa34b8a7b4bd7d436785d /?&gt;
    /// </code>
    /// <para>
    /// Example output:
    /// </para>
    /// <code>
    /// &lt;script src=&quot;//gist.github.com/10a2f6e0186fa34b8a7b4bd7d436785d.js&quot; type=&quot;text/javascript&quot;&gt;&lt;/script&gt;
    /// </code>
    /// </example>
    /// <parameter name="Id">The ID of the gist.</parameter>
    /// <parameter name="Username">The username that the gist is under (optional).</parameter>
    /// <parameter name="File">The file within the gist to embed (optional).</parameter>
    public class GistShortcode : SyncShortcode
    {
        private const string Id = nameof(Id);
        private const string Username = nameof(Username);
        private const string File = nameof(File);

        /// <inheritdoc />
        public override ShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            IMetadataDictionary arguments = args.ToDictionary(Id, Username, File);
            arguments.RequireKeys(Id);
            return $"<script src=\"//gist.github.com/{arguments.GetString(Username, x => x + "/")}{arguments.GetString(Id)}.js"
                + $"{arguments.GetString(File, x => "?file=" + x)}\" type=\"text/javascript\"></script>";
        }
    }
}
