using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Statiq.Common;

namespace Statiq.Web.Shortcodes
{
    /// <summary>
    /// Renders an HTML table.
    /// </summary>
    /// <remarks>
    /// The content of the shortcode contains the table with each row on a new line and each
    /// column separated by new lines. Enclose columns in quotes if they contain a space.
    /// Note that since the content of a shortcode may get processed by template engines like
    /// Markdown and the content of this shortcode should not be, you probably want to wrap
    /// the shortcode content in the special XML processing instruction that will get trimmed
    /// like <c>&lt;?* ... ?&gt;</c> so it "passes through" any template engines (see example below).
    /// </remarks>
    /// <example>
    /// <para>
    /// Example usage:
    /// </para>
    /// <code>
    /// &lt;?# Table Class=table HeaderRows=1 ?&gt;
    /// &lt;?*
    /// Vehicle "Number Of Wheels"
    /// Bike 2
    /// Car 4
    /// Truck "A Whole Lot"
    /// ?&gt;
    /// &lt;?#/ Table ?&gt;
    /// </code>
    /// <para>
    /// Example output:
    /// </para>
    /// <code>
    /// &lt;table class=&quot;table&quot;&gt;
    ///   &lt;thead&gt;
    ///     &lt;tr&gt;
    ///       &lt;th&gt;Vehicle&lt;/th&gt;
    ///       &lt;th&gt;Number Of Wheels&lt;/th&gt;
    ///     &lt;/tr&gt;
    ///   &lt;/thead&gt;
    ///   &lt;tbody&gt;
    ///     &lt;tr&gt;
    ///       &lt;td&gt;Bike&lt;/td&gt;
    ///       &lt;td&gt;2&lt;/td&gt;
    ///     &lt;/tr&gt;
    ///     &lt;tr&gt;
    ///       &lt;td&gt;Car&lt;/td&gt;
    ///       &lt;td&gt;4&lt;/td&gt;
    ///     &lt;/tr&gt;
    ///     &lt;tr&gt;
    ///       &lt;td&gt;Truck&lt;/td&gt;
    ///       &lt;td&gt;A Whole Lot&lt;/td&gt;
    ///     &lt;/tr&gt;
    ///   &lt;/tbody&gt;
    /// &lt;/table&gt;
    /// </code>
    /// </example>
    /// <parameter name="Class">The <c>class</c> attribute to apply to the <c>table</c> element.</parameter>
    /// <parameter name="HeaderRows">The number of header rows in the table.</parameter>
    /// <parameter name="FooterRows">The number of footer rows in the table.</parameter>
    /// <parameter name="HeaderCols">The number of header columns to the left of the table.</parameter>
    /// <parameter name="HeaderClass">The <c>class</c> attribute to apply to the <c>thead</c> element.</parameter>
    /// <parameter name="BodyClass">The <c>class</c> attribute to apply to the <c>tbody</c> element.</parameter>
    /// <parameter name="FooterClass">The <c>class</c> attribute to apply to the <c>tfoot</c> element.</parameter>
    public class TableShortcode : SyncShortcode
    {
        private const string Class = nameof(Class);
        private const string HeaderRows = nameof(HeaderRows);
        private const string FooterRows = nameof(FooterRows);
        private const string HeaderCols = nameof(HeaderCols);
        private const string HeaderClass = nameof(HeaderClass);
        private const string BodyClass = nameof(BodyClass);
        private const string FooterClass = nameof(FooterClass);

        /// <inheritdoc />
        public override ShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            IMetadataDictionary dictionary = args.ToDictionary(
                Class,
                HeaderRows,
                FooterRows,
                HeaderCols,
                HeaderClass,
                BodyClass,
                FooterClass);

            string[] lines = content
                .Trim()
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToArray();

            // Table
            XElement table = new XElement(
                "table",
                dictionary.XAttribute(Class));
            int line = 0;

            // Header
            int headerRows = dictionary.Get(HeaderRows, 0);
            XElement header = null;
            for (int c = 0; c < headerRows && line < lines.Length; c++, line++)
            {
                // Create the section
                if (c == 0)
                {
                    header = new XElement(
                        "thead",
                        dictionary.XAttribute("class", HeaderClass));
                    table.Add(header);
                }

                // Create the current row
                XElement row = new XElement("tr");
                header.Add(row);

                // Add the columns
                foreach (string col in ShortcodeHelper.SplitArguments(lines[line], 0).ToValueArray())
                {
                    row.Add(new XElement("th", col));
                }
            }

            // Body
            int bodyRows = lines.Length - line - dictionary.Get(FooterRows, 0);
            XElement body = null;
            for (int c = 0; c < bodyRows && line < lines.Length; c++, line++)
            {
                // Create the section
                if (c == 0)
                {
                    body = new XElement(
                        "tbody",
                        dictionary.XAttribute("class", BodyClass));
                    table.Add(body);
                }

                // Create the current row
                XElement row = new XElement("tr");
                body.Add(row);

                // Add the columns
                int th = dictionary.Get(HeaderCols, 0);
                foreach (string col in ShortcodeHelper.SplitArguments(lines[line], 0).ToValueArray())
                {
                    row.Add(new XElement(th-- > 0 ? "th" : "td", col));
                }
            }

            // Footer
            XElement footer = null;
            for (int c = 0; line < lines.Length; c++, line++)
            {
                // Create the section
                if (c == 0)
                {
                    footer = new XElement(
                        "tfoot",
                        dictionary.XAttribute("class", FooterClass));
                    table.Add(footer);
                }

                // Create the current row
                XElement row = new XElement("tr");
                footer.Add(row);

                // Add the columns
                int th = dictionary.Get(HeaderCols, 0);
                foreach (string col in ShortcodeHelper.SplitArguments(lines[line], 0).ToValueArray())
                {
                    row.Add(new XElement(th-- > 0 ? "th" : "td", col));
                }
            }

            return table.ToString();
        }
    }
}
