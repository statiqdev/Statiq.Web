using System;
using System.Linq;
using Statiq.Common;
using Statiq.Core;
using Statiq.Html;
using Statiq.Web.Pipelines;

namespace Statiq.Web.Modules
{
    public class ProcessScripts : ForAllDocuments
    {
        public ProcessScripts(bool exceptArchives)
            : base(new ExecuteIf(Config.FromDocument(doc => (doc.MediaTypeEquals(MediaTypes.CSharp) || doc.GetBool(WebKeys.Script)) && (!exceptArchives || !Archives.IsArchive(doc))))
            {
                new EvaluateScript(),

                // Remove the script extension and reset the media type and destination path
                new ExecuteIf(Config.FromDocument(doc =>
                    doc.MediaTypeEquals(MediaTypes.CSharp)
                    && doc.GetBool(WebKeys.RemoveScriptExtension, true)
                    && !doc.Source.IsNullOrEmpty
                    && doc.Source.ChangeExtension(null).HasExtension // Check if it's got multiple extensions
                    && MediaTypes.GetExtensions(MediaTypes.CSharp).Any(x => x.Equals(doc.Source.Extension, StringComparison.OrdinalIgnoreCase))))
                {
                    new SetDestination(Config.FromDocument(doc => doc.Source.GetRelativeInputPath().ChangeExtension(null))),
                    new SetMediaType(Config.FromDocument(doc => doc.Destination.MediaType))
                }
            })
        {
        }
    }
}
