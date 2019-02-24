using System;
using System.Collections.Generic;
using System.Text;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Shortcodes;
using Wyam.Common.Util;

namespace Wyam.Core.Shortcodes.Html
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
    public class Gist : IShortcode
    {
        /// <inheritdoc />
        public IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            ConvertingDictionary arguments = args.ToDictionary(
                context,
                "Id",
                "Username",
                "File");
            arguments.RequireKeys("Id");
            return context.GetShortcodeResult(
                $"<script src=\"//gist.github.com/{arguments.String("Username", x => x + "/")}{arguments.String("Id")}.js"
                + $"{arguments.String("File", x => "?file=" + x)}\" type=\"text/javascript\"></script>");
        }
    }
}
