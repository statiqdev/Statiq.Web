using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.Images
{
    public class Xmp : IModule
    {
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            inputs.Select(x =>
            {

                var extractedMetadata = MetadataExtractor.ImageMetadataReader.ReadMetadata(x.GetStream())
                    .OfType<MetadataExtractor.Formats.Xmp.XmpDirectory>()
                    .FirstOrDefault();
                if (extractedMetadata.HasError)
                {
                    using (context.Trace.WithIndent().Warning($"Errors in {x.Source}"))
                    {
                        foreach (var error in extractedMetadata.Errors)
                        {
                            context.Trace.Warning(error);
                        }
                    }
                }

            });
        }
    }
}
