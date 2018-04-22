using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;

namespace Wyam.OpenAPI
{
    public class OpenAPI : IModule
    {
        public const string OpenAPI_DEFAULT_KEY = "MyOpenApi";

        private readonly string _key;
        private readonly Action<OpenApiReaderSettings> _readerSetup;

        public OpenAPI(string key = OpenAPI_DEFAULT_KEY)
            : this(s => { }, key)
        {
        }

        public OpenAPI(Action<OpenApiReaderSettings> readerSetup, string key = OpenAPI_DEFAULT_KEY)
        {
            _key = key;
            _readerSetup = readerSetup;
        }

        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs
                .AsParallel()
                .Select(context, input =>
                {
                    var readerSetting = new OpenApiReaderSettings();
                    _readerSetup(readerSetting);

                    OpenApiDiagnostic diagnostic = null;
                    try
                    {
                        var openApiDocument = new OpenApiStringReader(readerSetting).Read(input.Content, out diagnostic);
                        var documentMetadata = new Dictionary<string, object>() { { _key, openApiDocument } };
                        return context.GetDocument(input, documentMetadata);
                    }
                    catch (Exception ex)
                    {
                        var msg = string.Join(Environment.NewLine, diagnostic.Errors.Select(e => e.Message).ToArray());
                        throw new Exception(msg, innerException: ex);
                    }
                })
                .Where(x => x != null);
        }
    }
}
