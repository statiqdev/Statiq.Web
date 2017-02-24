using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Execution;
using Wyam.Common.Tracing;
using Wyam.Common.Util;

namespace Wyam.Tables
{
    /// <summary>
    /// Transforms Excel content to CSV.
    /// </summary>
    /// <remarks>
    /// This module reads the content of each input document as Excel OOXML and outputs CSV content.
    /// The output CSV content uses <c>,</c> as separator and encloses every value in <c>"</c>.
    /// </remarks>
    /// <category>Content</category>
    public class ExcelToCsv : IModule
    {
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs.AsParallel().Select(context, input =>
            {
                try
                {
                    using (var stream = input.GetStream())
                    {
                        Tabular.Table table = Tabular.Excel.ReadFrom(stream, Tabular.ExcelFormat.Excel2007);
                        Tabular.Csv csv = Tabular.Csv.ToCsv(table);
                        return context.GetDocument(input, csv.Data);
                    }
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
