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
    public class ResourceLink : IShortcode
    {
        public IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            IMetadataDictionary arguments = ShortcodeHelper.GetArgsDictionary(context, args, "Path", "Rel", "IncludeHost", "Type");
            string typeAttribute = arguments.ContainsKey("Type") ? $" type=\"{arguments.String("Type")}\"" : null;
            return context.GetShortcodeResult($"<link rel=\"{arguments.String("Rel", "stylesheet")}\" href=\"{Link.GetLink(context, arguments)}\"{typeAttribute}>");
        }
    }
}
