using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;

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
                        records = ExcelFile.GetAllRecords(input.GetStream());
                    }

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        CsvFile.WriteAllRecords(records, memoryStream);
                        return context.GetDocument(input, memoryStream);
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
