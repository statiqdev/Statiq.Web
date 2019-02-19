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
    /// <summary>
    /// Generates HTML5 <c>figure</c> elements.
    /// </summary>
    /// <remarks>
    /// The content of this shortcode specifies a caption to output inside a nested <c>figcaption</c> element.
    /// </remarks>
    /// <example>
    /// <para>
    /// Example usage:
    /// </para>
    /// <code>
    /// &lt;?# Figure Src="/assets/wyam.jpg" ?&gt;
    /// Wyam Logo
    /// &lt;?#/ Figure ?&gt;
    /// </code>
    /// <para>
    /// Example output:
    /// </para>
    /// <code>
    /// &lt;figure&gt;
    ///   &lt;img src=&quot;/assets/wyam.jpg&quot; /&gt;
    ///   &lt;figcaption&gt;Wyam Logo&lt;/figcaption&gt;
    /// &lt;/figure&gt;
    /// </code>
    /// </example>
    /// <parameter name="Src">URL of the image to be displayed.</parameter>
    /// <parameter name="Link">If the image needs to be hyperlinked, URL of the destination.</parameter>
    /// <parameter name="Target">Optional <c>target</c> attribute for the URL if <c>Link</c> parameter is set.</parameter>
    /// <parameter name="Rel">Optional <c>rel</c> attribute for the URL if <c>Link</c> parameter is set.</parameter>
    /// <parameter name="Alt">Alternate text for the image if the image cannot be displayed.</parameter>
    /// <parameter name="Class"><c>class</c> attribute to apply to the <c>figure</c> element.</parameter>
    /// <parameter name="Height"><c>height</c> attribute of the image.</parameter>
    /// <parameter name="Width"><c>width</c> attribute of the image.</parameter>
    public class Figure : IShortcode
    {
        /// <inheritdoc />
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
