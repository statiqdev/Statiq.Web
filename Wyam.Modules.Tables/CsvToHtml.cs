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
    /// Converts Csv files to html tables.
    /// </summary>
    /// <remarks>
    /// The output document only contains the table tag. No html or body tag.
    /// 
    /// The csv files must use <code>,</code> as seperator and encapsule every value in <c>"</c>.
    /// </remarks>
    public class CsvToHtml : IModule
    {
        private bool _firstLineHeader = false;

        /// <summary>
        /// Enables the first line as headers of your table.
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
                                builder.AppendLine($"<th>{cell.Value}</th>");
                            else
                                builder.AppendLine($"<td>{cell.Value}</td>");
                        }
                        builder.AppendLine("</tr>");
                        firstLine = false;
                    }
                    builder.AppendLine("</table>");
                    return x.Clone(builder.ToString());

                }
                catch (Exception e)
                {
                    context.Trace.Error($"An {e.ToString()} occured ({x.Source}).\n\t{e.Message}");
                    return null;
                }
            }).Where(x => x != null);
        }
    }
}
