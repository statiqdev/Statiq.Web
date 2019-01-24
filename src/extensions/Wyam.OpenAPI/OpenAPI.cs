using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        public const string OpenAPIDEFAULTKEY = "openApi";

        private readonly string _key;
        private readonly Action<OpenApiReaderSettings> _readerSetup;

        public OpenAPI(string key = OpenAPIDEFAULTKEY)
            : this(s => { }, key)
        {
        }

        public OpenAPI(Action<OpenApiReaderSettings> readerSetup, string key = OpenAPIDEFAULTKEY)
        {
            _key = key;
            _readerSetup = readerSetup;
        }

#pragma warning disable SA1005 // Single line comments should begin with single space

        //public OpenAPI ComponentsWhere(OpenApiXml =>)
        //{
        //    return this;
        //}
#pragma warning restore SA1005 // Single line comments should begin with single space
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs
                .AsParallel()
                .Select(context, input =>
                {
                    OpenApiReaderSettings readerSetting = new OpenApiReaderSettings();
                    _readerSetup(readerSetting);

                    OpenApiDiagnostic diagnostic = null;
                    try
                    {
                        OpenApiDocument openApiDocument = new OpenApiStringReader(readerSetting).Read(input.Content, out diagnostic);
                        Dictionary<string, object> documentMetadata = new Dictionary<string, object>() { { _key, openApiDocument } };
                        return context.GetDocument(input, documentMetadata);
                    }
                    catch (Exception ex)
                    {
                        string msg = string.Join(Environment.NewLine, diagnostic?.Errors.Select(e => e.Message).ToArray() ?? new string[0]);
                        throw new Exception(msg, innerException: ex); // TODO, find another more precise exception type
                    }
                })
                .Where(x => x != null);
        }
    }
}
