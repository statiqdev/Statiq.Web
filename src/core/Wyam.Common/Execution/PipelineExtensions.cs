using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;

namespace Wyam.Common.Execution
{
    /// <summary>
    /// Extension methods for <see cref="IPipeline"/>.
    /// </summary>
    public static class PipelineExtensions
    {
        /// <summary>
        /// Specifies that a given pipeline doesn't use data from other pipelines and prevents reprocessing of documents after the first pass.
        /// </summary>
        /// <remarks>
        /// when set, the pipeline looks for the first occurrence of a given <see cref="IDocument.Source"/> and then caches all final result
        /// documents that have the same source. On subsequent executions, if a document with a previously seen <see cref="IDocument.Source"/>
        /// is found and it has the same content, that document is removed from the module output and therefore won't get passed to the next
        /// module. At the end of the pipeline, all the documents from the first pass that have the same source as the removed one are added
        /// back to the result set (so later pipelines can still access them in the documents collection if needed).
        /// </remarks>
        /// <param name="pipeline">The pipeline to set.</param>
        /// <param name="processDocumentsOnce"><c>true</c> to process documents once, <c>false</c> for the default behavior.</param>
        /// <returns>The specified pipeline.</returns>
        public static IPipeline WithProcessDocumentsOnce(this IPipeline pipeline, bool processDocumentsOnce = true)
        {
            pipeline.ProcessDocumentsOnce = processDocumentsOnce;
            return pipeline;
        }
    }
}
