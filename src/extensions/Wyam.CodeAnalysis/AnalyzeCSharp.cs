using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Execution;

namespace Wyam.CodeAnalysis
{
    /// <summary>
    /// Performs static code analysis on the input documents, outputting a new document for each symbol.
    /// </summary>
    /// <remarks>
    /// This module acts as the basis for code analysis scenarios such as generating source code documentation.
    /// All input documents are assumed to contain C# source in their content and are used to create a Roslyn
    /// compilation. All symbols (namespaces, types, members, etc.) in the compilation are then recursively 
    /// processed and output from this module as documents, one per symbol. The output documents have empty content
    /// and all information about the symbol is contained in the metadata. This lets you pass the output documents
    /// for each symbol on to a template engine like Razor and generate pages for each symbol by having the
    /// template use the document metadata.
    /// </remarks>
    /// <include file="Documentation.xml" path="/Documentation/AnalyzeCSharp/*" />
    /// <category>Metadata</category>
    public class AnalyzeCSharp : CodeAnalysisModule<AnalyzeCSharp>
    {
        public override IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            // Get syntax trees (supply path so that XML doc includes can be resolved)
            ConcurrentBag<SyntaxTree> syntaxTrees = new ConcurrentBag<SyntaxTree>();
            Parallel.ForEach(inputs, input =>
            {
                using (Stream stream = input.GetStream())
                {
                    SourceText sourceText = SourceText.From(stream);
                    syntaxTrees.Add(CSharpSyntaxTree.ParseText(sourceText,
                        path: input.String(Keys.SourceFilePath, string.Empty)));
                }
            });

            // Create the compilation (have to supply an XmlReferenceResolver to handle include XML doc comments)
            MetadataReference mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            Compilation compilation = CSharpCompilation
                .Create("CodeAnalysisModule", syntaxTrees)
                .WithReferences(mscorlib)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                    xmlReferenceResolver: new XmlFileResolver(context.FileSystem.RootPath.FullPath)));

            return Execute(compilation, new [] { compilation.Assembly.GlobalNamespace }, context);
        }
    }
}
