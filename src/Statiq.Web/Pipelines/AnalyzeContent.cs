using System.Linq;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;

namespace Statiq.Web.Pipelines
{
    /// <summary>
    /// Allows content-based analyzers to run after content pipelines once content has been output.
    /// To use, set the <see cref="IAnalyzer.PipelinePhases"/> to this pipeline's <see cref="Phase.Process"/> phase.
    /// </summary>
    public class AnalyzeContent : Pipeline
    {
        public AnalyzeContent()
        {
            // Ensures output from non-deployment pipelines is complete since analyzers may look at output files
            Deployment = true;

            ExecutionPolicy = ExecutionPolicy.Normal;

            InputModules = new ModuleList
            {
                new ReplaceDocuments(nameof(Content), nameof(Archives), nameof(Assets))
            };
        }
    }
}
