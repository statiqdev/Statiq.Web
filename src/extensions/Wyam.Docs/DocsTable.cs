using System.Collections.Generic;
using Wyam.Common.Documents;

namespace Wyam.Docs
{
    /// <summary>
    /// This model is used for the DocsTable partial that renders documents, titles, headers, and summaries as a table.
    /// </summary>
    public class DocsTable
    {
        public IList<IDocument> Docs { get; set; }
        public string Title { get; set; }
        public string Header { get; set; }
        public bool HasSummary { get; set; }
    }
}