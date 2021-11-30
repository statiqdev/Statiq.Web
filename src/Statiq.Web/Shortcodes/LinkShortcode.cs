using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.Web.Shortcodes
{
    /// <summary>
    /// Renders a link from the given path, using default settings or specifying overrides as appropriate.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;?# Link "/foo/bar" /?&gt;
    /// </code>
    /// </example>
    /// <parameter name="Path">The path to get a link for.</parameter>
    /// <parameter name="IncludeHost">
    /// If set to <c>true</c> the host configured in the output settings will
    /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).
    /// </parameter>
    /// <parameter name="Host">The host to use for the link.</parameter>
    /// <parameter name="Root">The root of the link. The value of this parameter is prepended to the path.</parameter>
    /// <parameter name="Scheme">The scheme to use for the link (will override the <c>UseHttps</c> parameter).</parameter>
    /// <parameter name="UseHttps">If set to <c>true</c>, HTTPS will be used as the scheme for the link.</parameter>
    /// <parameter name="HideIndexPages">If set to <c>true</c>, index files will be hidden.</parameter>
    /// <parameter name="HideExtensions">If set to <c>true</c>, extensions will be hidden.</parameter>
    /// <parameter name="Lowercase">If set to <c>true</c>, links will be rendered in all lowercase.</parameter>
    public class LinkShortcode : SyncShortcode
    {
        private const string Path = nameof(Path);
        private const string IncludeHost = nameof(IncludeHost);
        private const string Host = nameof(Host);
        private const string Root = nameof(Root);
        private const string Scheme = nameof(Scheme);
        private const string UseHttps = nameof(UseHttps);
        private const string HideIndexPages = nameof(HideIndexPages);
        private const string HideExtensions = nameof(HideExtensions);
        private const string Lowercase = nameof(Lowercase);

        /// <inheritdoc />
        public override ShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            IMetadataDictionary arguments = args.ToDictionary(
                Path,
                IncludeHost,
                Host,
                Root,
                Scheme,
                UseHttps,
                HideIndexPages,
                HideExtensions,
                Lowercase);
            arguments.RequireKeys(Path);

            string path = arguments.GetString(Path);
            if (context.LinkGenerator.TryGetAbsoluteHttpUri(path, out string absoluteUri))
            {
                return absoluteUri;
            }

            // Use "Host" if it's provided, otherwise use Host setting if "IncludeHost" is true
            string host = arguments.GetString(Host, arguments.GetBool(IncludeHost) ? context.Settings.GetString(Keys.Host) : null);

            // Use "Root" if it's provided, otherwise LinkRoot setting
            string root = arguments.GetString(Root, context.Settings.GetString(Keys.LinkRoot));

            // Use "Scheme" if it's provided, otherwise if "UseHttps" is true use "https" or use LinksUseHttps setting
            string scheme = arguments.GetString(Scheme, arguments.ContainsKey(UseHttps)
                ? (arguments.GetBool(UseHttps) ? "https" : null)
                : (context.Settings.GetBool(Keys.LinksUseHttps) ? "https" : null));

            // If "HideIndexPages" is provided and true use default hide pages, otherwise use default hide pages if LinkHideIndexPages is true
            string indexFileName = context.Settings.GetIndexFileName();
            string[] hidePages = arguments.ContainsKey(HideIndexPages)
                ? (arguments.GetBool(HideIndexPages) ? new[] { indexFileName } : null)
                : (context.Settings.GetBool(Keys.LinkHideIndexPages) ? new[] { indexFileName } : null);

            // If "HideExtensions" is provided and true use default hide extensions, otherwise use default hide extensions if LinkHideExtensions is true
            string[] pageFileExtensions = context.Settings.GetPageFileExtensions();
            string[] hideExtensions = arguments.ContainsKey(HideExtensions)
                ? (arguments.GetBool(HideExtensions) ? pageFileExtensions : null)
                : (context.Settings.GetBool(Keys.LinkHideExtensions) ? pageFileExtensions : null);

            // If "Lowercase" is provided use that, otherwise use LinkLowercase setting
            bool lowercase = arguments.ContainsKey(Lowercase)
                ? arguments.GetBool(Lowercase)
                : context.Settings.GetBool(Keys.LinkLowercase);

            return context.LinkGenerator.GetLink(path, host, root, scheme, hidePages, hideExtensions, lowercase);
        }
    }
}