using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.Images
{
    public class Xmp : IModule
    {
        private readonly List<XmpEntryToMetadata> _entrysToSearch = new List<XmpEntryToMetadata>();

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

                foreach (var item in extractedMetadata.XmpMeta.Properties)
                {
                    string pathWithoutNamespacePrefix = Regex.Replace(item.Path, "^[^:]:(?<name>.*)$", "${name}");
                    XmpEntryToMetadata entry = _entrysToSearch.FirstOrDefault(y => y.Namespace == item.Namespace 
                                                        && y.Name == pathWithoutNamespacePrefix);
                    if(entry != null)
                    {
                        
                    }
    }

            });
        }
        private class XmpEntryToMetadata
        {
            public string Namespace { get; set; }
            public string Name { get; set; }
            public string MetadataKey { get; set; }
        }
    }
}
