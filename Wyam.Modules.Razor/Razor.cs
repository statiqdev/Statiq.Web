using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Razor;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Wyam.Extensibility;

namespace Wyam.Modules.Razor
{
    public class Razor : IModule
    {
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IPipelineContext pipeline)
        {
            return inputs.Select(x =>
            {
                // TODO: Add namespaces from the engine (will need to expose via the pipeline context)

                // Generate code using Razor
                GeneratedClassContext generatedClassContext = new GeneratedClassContext(
                    executeMethodName: "ExecuteAsync",
                    writeMethodName: "Write",
                    writeLiteralMethodName: "WriteLiteral",
                    writeToMethodName: "WriteTo",
                    writeLiteralToMethodName: "WriteLiteralTo",
                    templateTypeName: "Wyam.Modules.Razor.HelperResult",
                    defineSectionMethodName: "DefineSection",
                    generatedTagHelperContext: null)
                {
                    ResolveUrlMethodName = "Href",
                    BeginContextMethodName = "BeginContext",
                    EndContextMethodName = "EndContext"
                };
                RazorEngineHost host = new RazorEngineHost(new CSharpRazorCodeLanguage())
                {
                    DefaultBaseClass = "Wyam.Modules.Razor.RazorPage",
                    GeneratedClassContext = generatedClassContext
                };
                RazorTemplateEngine engine = new RazorTemplateEngine(host);
                GeneratorResults results = engine.GenerateCode(new StringReader(x.Content));
                if (!results.Success)
                {
                    foreach (RazorError error in results.ParserErrors)
                    {
                        pipeline.Trace.Error("Error parsing Razor document at {0}: {1}.", error.Location.ToString(), error.Message);
                    }
                    return x.Clone(string.Empty);
                }

                // TODO: Run the code through the Roslyn scripting engine to get a delegate

                // TODO: Set RazorPage.Path

                // TODO: Cache the generated code for the next time through - what to base it on; a hash of the content?

                return x.Clone(string.Empty);
            });
        }
    }
}
