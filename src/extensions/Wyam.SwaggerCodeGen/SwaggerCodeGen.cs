using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.OpenApi.Models;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;

namespace Wyam.SwaggerCodeGen
{
    public class SwaggerCodeGen : IModule
    {
        private readonly string _key;
        private readonly string _openApiInputKey;

        public SwaggerCodeGen(string key, string openApiInputKey = OpenAPI.OpenAPI.OpenAPI_DEFAULT_KEY)
        {
            _key = key;
            _openApiInputKey = openApiInputKey;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs
                .AsParallel()
                .Select(context, input =>
                {
                    if (!input.Metadata.TryGetValue(_openApiInputKey, out var inputMeta)) return null;
                    if (!(inputMeta is OpenApiDocument openApiDocument)) return null;

                    

                    var documentMetadata = new Dictionary<string, object>() { { _openApiInputKey, openApiDocument } };
                    return context.GetDocument(input, documentMetadata);
                })
                .Where(x => x != null);

        }
    }
}
