using System;
using System.Collections.Generic;
using System.Text;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Shortcodes;
using Wyam.Common.Util;

namespace Wyam.Core.Shortcodes.Html
{
    /// <summary>
    /// Renders a link from the given path, using default settings or specifying overrides as appropriate.
    /// </summary>
    public class Link : IShortcode
    {
        public IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            IMetadataDictionary arguments = ShortcodeHelper.GetArgsDictionary(
                context,
                args,
                "Path",
                "IncludeHost",
                "Host",
                "Root",
                "Scheme",
                "UseHttps",
                "HideIndexPages",
                "HideExtensions",
                "Lowercase");
            arguments.RequireKeys("Path");

            string path = arguments.String("Path");
            if (LinkGenerator.TryGetAbsoluteHttpUri(path, out string absoluteUri))
            {
                return context.GetShortcodeResult(absoluteUri);
            }
            FilePath filePath = new FilePath(path);

            // Use "Host" if it's provided, otherwise use Host setting if "IncludeHost" is true
            string host = arguments.String("Host", arguments.Bool("IncludeHost") ? context.String(Keys.Host) : null);

            // Use "Root" if it's provided, otherwise LinkRoot setting
            DirectoryPath root = arguments.DirectoryPath("Root", context.DirectoryPath(Keys.LinkRoot));

            // Use "Scheme" if it's provided, otherwise if "UseHttps" is true use "https" or use LinksUseHttps setting
            string scheme = arguments.String("Scheme", arguments.ContainsKey("UseHttps")
                ? (arguments.Bool("UseHttps") ? "https" : null)
                : (context.Bool(Keys.LinksUseHttps) ? "https" : null));

            // If "HideIndexPages" is provided and true use default hide pages, otherwise use default hide pages if LinkHideIndexPages is true
            string[] hidePages = arguments.ContainsKey("HideIndexPages")
                ? (arguments.Bool("HideIndexPages") ? LinkGenerator.DefaultHidePages : null)
                : (context.Bool(Keys.LinkHideIndexPages) ? LinkGenerator.DefaultHidePages : null);

            // If "HideExtensions" is provided and true use default hide extensions, otherwise use default hide extensions if LinkHideExtensions is true
            string[] hideExtensions = arguments.ContainsKey("HideExtensions")
                ? (arguments.Bool("HideExtensions") ? LinkGenerator.DefaultHideExtensions : null)
                : (context.Bool(Keys.LinkHideExtensions) ? LinkGenerator.DefaultHideExtensions : null);

            // If "Lowercase" is provided use that, otherwise use LinkLowercase setting
            bool lowercase = arguments.ContainsKey("Lowercase")
                ? arguments.Bool("Lowercase")
                : context.Bool(Keys.LinkLowercase);

            return context.GetShortcodeResult(LinkGenerator.GetLink(filePath, host, root, scheme, hidePages, hideExtensions, lowercase));
        }
    }
}
