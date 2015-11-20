using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.Tables
{
    /// <summary>
    /// Converts CSV content to HTML tables.
    /// </summary>
    /// <remarks>
    /// This module reads the content of each input document as CSV and outputs an HTML <c>&lt;table&gt;</c> tag 
    /// containing the CSV content. No <c>&lt;html&gt;</c> or <c>&lt;body&gt;</c> tags are output. The input CSV
    /// content must use <c>,</c> as separator and enclose every value in <c>"</c>.
    /// </remarks>
    /// <category>Content</category>
    public class CsvToHtml : IModule
    {
        private bool _firstLineHeader = false;

        /// <summary>
        /// Treats the first line of input content as a header and generates <c>&lt;th&gt;</c> tags in the output table.
        /// </summary>
        public CsvToHtml WithHeader()
        {
            _firstLineHeader = true;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().Select(x =>
            {
                try
                {
                    Tabular.Csv csv = new Tabular.Csv() { Data = x.Content };
                    Tabular.Table table = Tabular.Csv.FromCsv(csv);
                    StringBuilder builder = new StringBuilder();

                    bool firstLine = true;
                    builder.AppendLine("<table>");
                    foreach (var row in table.Rows)
                    {
                        builder.AppendLine("<tr>");
                        foreach (var cell in row)
                        {
                            if (_firstLineHeader && firstLine)
                            {
                                builder.AppendLine($"<th>{cell.Value}</th>");
                            }
                            else
                            {
                                builder.AppendLine($"<td>{cell.Value}</td>");
                            }
                        }
                        builder.AppendLine("</tr>");
                        firstLine = false;
                    }
                    builder.Append("</table>");
                    return x.Clone(builder.ToString());
                }
                catch (Exception e)
                {
                    context.Trace.Error($"An {e.ToString()} occurred ({x.Source}): {e.Message}");
                    return null;
                }
            }).Where(x => x != null);
        }
    }
}
