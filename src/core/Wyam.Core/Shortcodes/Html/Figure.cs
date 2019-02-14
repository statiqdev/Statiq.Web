using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Shortcodes;

namespace Wyam.Core.Shortcodes.Html
{
    public class Figure : IShortcode
    {
        public IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            ConvertingDictionary arguments = args.ToDictionary(
                context,
                "Src",
                "Link",
                "Target",
                "Rel",
                "Alt",
                "Title",
                "Caption",
                "Class",
                "Height",
                "Width",
                "Attr",
                "AttrLink");

            XElement figure = new XElement(
                "figure",
                arguments.XElement("img", "Src", src => new[]
                {
                    new XAttribute("src", context.GetLink(src))
                }));

            return context.GetShortcodeResult(figure.ToString());
        }
    }
}
