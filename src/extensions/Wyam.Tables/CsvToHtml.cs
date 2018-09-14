using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;

namespace Wyam.Tables
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
        /// <returns>The current module instance.</returns>
        public CsvToHtml WithHeader()
        {
            _firstLineHeader = true;
            return this;
        }

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().Select(context, input =>
            {
                try
                {
                    IEnumerable<IEnumerable<string>> records;
                    using (Stream stream = input.GetStream())
                    {
                        records = CsvFile.GetAllRecords(input.GetStream());
                    }

                    StringBuilder builder = new StringBuilder();
                    bool firstLine = true;
                    builder.AppendLine("<table>");
                    foreach (IEnumerable<string> row in records)
                    {
                        builder.AppendLine("<tr>");
                        foreach (string cell in row)
                        {
                            if (_firstLineHeader && firstLine)
                            {
                                builder.AppendLine($"<th>{cell}</th>");
                            }
                            else
                            {
                                builder.AppendLine($"<td>{cell}</td>");
                            }
                        }
                        builder.AppendLine("</tr>");
                        firstLine = false;
                    }
                    builder.Append("</table>");
                    return context.GetDocument(input, context.GetContentStream(builder.ToString()));
                }
                catch (Exception e)
                {
                    Trace.Error($"An {e.ToString()} occurred ({input.SourceString()}): {e.Message}");
                    return null;
                }
            }).Where(x => x != null);
        }
    }
}
