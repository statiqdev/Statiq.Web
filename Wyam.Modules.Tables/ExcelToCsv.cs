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
    /// Transforms Excel files to csv.
    /// </summary>
    /// <remarks>
    /// The seperator of the csv is <code>,</code> and every value is enclosed in <code>"</code>.
    /// </remarks>
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
                    context.Trace.Error($"An {e.ToString()} occured ({x.Source}).\n\t{e.Message}");
                    return null;
                }
            }).Where(x => x != null);
        }
    }
}
