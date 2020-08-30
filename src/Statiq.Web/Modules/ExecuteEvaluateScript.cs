using System;
using System.Linq;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;
using Statiq.Web.Pipelines;

namespace Statiq.Web.Modules
{
    public class ExecuteEvaluateScript : ForAllDocuments
    {
        public ExecuteEvaluateScript(bool exceptArchives)
            : base(new ExecuteIf(Config.FromDocument(doc => (doc.MediaTypeEquals(MediaTypes.CSharp) || doc.GetBool(WebKeys.Script)) && (!exceptArchives || !Archives.IsArchive(doc))))
            {
                new EvaluateScript(),

                // Remove the script extension and reset the media type and destination path
                new ExecuteIf(Config.FromDocument(doc =>
                    doc.MediaTypeEquals(MediaTypes.CSharp)
                    && doc.Destination.HasExtension
                    && MediaTypes.GetExtensions(MediaTypes.CSharp).Any(x => x.Equals(doc.Destination.Extension, StringComparison.OrdinalIgnoreCase))))
                {
                    new SetDestination(Config.FromDocument(doc => doc.Destination.ChangeExtension(null))),
                    new SetMediaType(Config.FromDocument(doc => MediaTypes.Get(doc.Destination.FullPath)))
                }
            })
        {
        }
    }
}
