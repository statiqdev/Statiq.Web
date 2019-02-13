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
    public class Link : IShortcode
    {
        public IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
            context.GetShortcodeResult(GetLink(context, args));

        internal static string GetLink(IExecutionContext context, KeyValuePair<string, string>[] args) =>
            GetLink(context, ShortcodeHelper.GetArgsDictionary(context, args, "Path", "IncludeHost"));

        internal static string GetLink(IExecutionContext context, IMetadataDictionary arguments)
        {
            arguments.RequireKeys("Path");
            return context.GetLink(
                arguments.String("Path"),
                arguments.Bool("IncludeHost"));
        }
    }
}
