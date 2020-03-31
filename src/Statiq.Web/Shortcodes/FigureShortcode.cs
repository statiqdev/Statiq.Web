using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Statiq.Common;

namespace Statiq.Web.Shortcodes
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
    /// &lt;?# Figure Src="/assets/statiq.jpg" ?&gt;
    /// Statiq Logo
    /// &lt;?#/ Figure ?&gt;
    /// </code>
    /// <para>
    /// Example output:
    /// </para>
    /// <code>
    /// &lt;figure&gt;
    ///   &lt;img src=&quot;/assets/statiq.jpg&quot; /&gt;
    ///   &lt;figcaption&gt;Statiq Logo&lt;/figcaption&gt;
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
    public class FigureShortcode : SyncContentShortcode
    {
        public override string Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            IMetadataDictionary arguments = args.ToDictionary(
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

            return figure.ToString();
        }
    }
}
