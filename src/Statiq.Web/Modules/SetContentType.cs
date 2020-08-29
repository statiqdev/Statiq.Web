using System;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.Web.Modules
{
    public class SetContentType : ParallelSyncModule
    {
        private readonly HashSet<string> _dataMediaTypes;
        private readonly HashSet<string> _contentMediaTypes;

        public SetContentType(Templates templates)
        {
            _dataMediaTypes = templates.GetMediaTypes(TemplateType.Data)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            _contentMediaTypes = templates.GetMediaTypes(TemplateType.ContentProcess)
                .Concat(templates.GetMediaTypes(TemplateType.ContentPostProcess))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        protected override IEnumerable<IDocument> ExecuteInput(IDocument input, IExecutionContext context)
        {
            if (input.ContainsKey(WebKeys.ContentType))
            {
                // Don't change it if one was already set
                return input.Yield();
            }
            ContentType contentType = ContentType.Asset;
            if (_dataMediaTypes.Contains(input.ContentProvider?.MediaType))
            {
                contentType = ContentType.Data;
            }
            else if (_contentMediaTypes.Contains(input.ContentProvider?.MediaType))
            {
                contentType = ContentType.Content;
            }
            return input.Clone(new MetadataItems { { WebKeys.ContentType, contentType } }).Yield();
        }
    }
}
