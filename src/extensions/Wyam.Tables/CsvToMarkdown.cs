using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;

namespace Wyam.Tables
{
    /// <summary>
    /// Converts CSV content to Markdown tables.
    /// </summary>
    /// <remarks>
    /// This module reads the content of each input document as CSV and outputs an Markdown table 
    /// containing the CSV content. The input CSV content must use <c>,</c> as separator and enclose 
    /// every value in <c>"</c>. The output table has the format
    /// 
    /// +--------------+-------------+
    /// | Test value 1 | TestValue 2 |
    /// +--------------+-------------+
    /// | Test value 2 | TestValue 3 |
    /// +--------------+-------------+
    /// </remarks>
    /// <category>Content</category>
    public class CsvToMarkdown : IModule
    {
        private bool _firstLineHeader = false;

        /// <summary>
        /// Treats the first line of input content as a header and generates <c>&lt;th&gt;</c> tags in the output table.
        /// </summary>
        public CsvToMarkdown WithHeader()
        {
            _firstLineHeader = true;
            return this;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().Select(input =>
            {
                try
                {
                    Tabular.Csv csv = new Tabular.Csv() { Data = input.Content };
                    Tabular.Table table = Tabular.Csv.FromCsv(csv);
                    StringBuilder builder = new StringBuilder();

                    int columnCount = table.First().Count;

                    int[] columnSize = new int[columnCount];

                    foreach (var row in table.Rows)
                    {
                        for (int i = 0; i < row.Count; i++)
                        {
                            var cell = row[i];
                            columnSize[i] = Math.Max(columnSize[i], cell.Value.Length);
                        }
                    }

                    bool firstLine = true;
                    WriteLine(builder, columnSize);
                    foreach (var row in table.Rows)
                    {
                        builder.Append("|");
                        for (int i = 0; i < columnSize.Length; i++)
                        {
                            builder.Append(" ");
                            builder.Append(row[i].Value);
                            builder.Append(' ', columnSize[i] - row[i].Value.Length + 1);
                            builder.Append("|");
                        }
                        builder.AppendLine();
                        WriteLine(builder, columnSize, this._firstLineHeader && firstLine);
                        firstLine = false;
                    }

                    return context.GetDocument(input, builder.ToString());
                }
                catch (Exception e)
                {
                    Trace.Error($"An {e.ToString()} occurred ({input.SourceString()}): {e.Message}");
                    return null;
                }
            }).Where(x => x != null);
        }

        private static void WriteLine(StringBuilder builder, int[] columnSize, bool isHeader = false)
        {
            foreach (var column in columnSize)
            {
                builder.Append("+");
                builder.Append(isHeader ? '=' : '-', column + 2);
            }
            builder.AppendLine("+");
        }
    }
}
