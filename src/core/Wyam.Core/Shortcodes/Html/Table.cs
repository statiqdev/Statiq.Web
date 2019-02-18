using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Shortcodes;

namespace Wyam.Core.Shortcodes.Html
{
    public class Table : IShortcode
    {
        public IShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            ConvertingDictionary dictionary = args.ToDictionary(
                context,
                "Class",
                "HeaderRows",
                "FooterRows",
                "HeaderCols",
                "HeaderClass",
                "BodyClass",
                "FooterClass");

            string[] lines = content
                .Trim()
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToArray();

            // Table
            XElement table = new XElement(
                "table",
                dictionary.XAttribute("class"));
            int line = 0;

            // Header
            int headerRows = dictionary.Get("HeaderRows", 0);
            XElement header = null;
            for (int c = 0; c < headerRows && line < lines.Length; c++, line++)
            {
                // Create the section
                if (c == 0)
                {
                    header = new XElement(
                        "thead",
                        dictionary.XAttribute("class", "HeaderClass"));
                    table.Add(header);
                }

                // Create the current row
                XElement row = new XElement("tr");
                header.Add(row);

                // Add the columns
                foreach (string col in ShortcodeParser.SplitArguments(lines[line], 0).ToValueArray())
                {
                    row.Add(new XElement("th", col));
                }
            }

            // Body
            int bodyRows = lines.Length - line - dictionary.Get("FooterRows", 0);
            XElement body = null;
            for (int c = 0; c < bodyRows && line < lines.Length; c++, line++)
            {
                // Create the section
                if (c == 0)
                {
                    body = new XElement(
                        "tbody",
                        dictionary.XAttribute("class", "BodyClass"));
                    table.Add(body);
                }

                // Create the current row
                XElement row = new XElement("tr");
                body.Add(row);

                // Add the columns
                int th = dictionary.Get("HeaderCols", 0);
                foreach (string col in ShortcodeParser.SplitArguments(lines[line], 0).ToValueArray())
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
                        dictionary.XAttribute("class", "FooterClass"));
                    table.Add(footer);
                }

                // Create the current row
                XElement row = new XElement("tr");
                footer.Add(row);

                // Add the columns
                int th = dictionary.Get("HeaderCols", 0);
                foreach (string col in ShortcodeParser.SplitArguments(lines[line], 0).ToValueArray())
                {
                    row.Add(new XElement(th-- > 0 ? "th" : "td", col));
                }
            }

            return context.GetShortcodeResult(table.ToString());
        }
    }
}
