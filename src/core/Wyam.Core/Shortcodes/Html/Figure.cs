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
                "Class",
                "Height",
                "Width");

            XElement figure = new XElement(
                "figure",
                arguments.XAttribute("class"));

            // Image link
            XElement imageLink = arguments.XElement("a", "link", x => new[]
            {
                new XAttribute("href", context.GetLink(x)),
                arguments.XAttribute("target"),
                arguments.XAttribute("rel")
            });

            // Image
            XElement image = arguments.XElement("img", "src", x => new[]
            {
                new XAttribute("src", context.GetLink(x)),
                arguments.XAttribute("alt"),
                arguments.XAttribute("height"),
                arguments.XAttribute("width")
            });
            if (imageLink != null && image != null)
            {
                imageLink.Add(image);
                figure.Add(imageLink);
            }
            else if (image != null)
            {
                figure.Add(image);
            }

            // Caption
            if (content != null)
            {
                figure.Add(new XElement("figcaption", content));
            }

            return context.GetShortcodeResult(figure.ToString());
        }
    }
}
