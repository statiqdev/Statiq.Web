using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using HDN = HandlebarsDotNet;

namespace Wyam.Handlebars
{
    public class Handlebars : IModule
    {
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            return inputs
                .AsParallel()
                .Select(context, input =>
                {
                    var template = HDN.Handlebars.Compile(input.Content);
                    var templateValues = new { metadata = input.Metadata.ToDictionary(kv => kv.Key, kv => kv.Value) };
                    var output = template(templateValues);
                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(output)))
                    {
                        var document = context.GetDocument(input, stream, disposeStream: false);
                        return document;
                    }
                })
                .Where(x => x != null);
        }
    }
}
