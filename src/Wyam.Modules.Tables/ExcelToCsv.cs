using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.Tables
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
            return inputs.AsParallel().Select(x =>
            {
                try
                {
                    using (var stream = x.GetStream())
                    {
                        Tabular.Table table = Tabular.Excel.ReadFrom(stream, Tabular.ExcelFormat.Excel2007);
                        Tabular.Csv csv = Tabular.Csv.ToCsv(table);
                        return x.Clone(csv.Data);
                    }
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
