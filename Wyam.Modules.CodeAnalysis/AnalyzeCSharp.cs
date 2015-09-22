using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Wyam.Common;

namespace Wyam.Modules.CodeAnalysis
{
    public class AnalyzeCSharp : IModule
    {
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            // Get syntax trees
            ConcurrentBag<SyntaxTree> syntaxTrees = new ConcurrentBag<SyntaxTree>();
            Parallel.ForEach(inputs, input =>
            {
                using (Stream stream = input.GetStream())
                {
                    SourceText sourceText = SourceText.From(stream);
                    syntaxTrees.Add(CSharpSyntaxTree.ParseText(sourceText));
                }
            });

            // Create the compilation
            MetadataReference mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            CSharpCompilation compilation = CSharpCompilation.Create("CodeAnalysisModule", syntaxTrees).WithReferences(mscorlib);

            // Get and return the document tree
            return new DocumentTreeBuilder(context).BuildDocumentTree(compilation.Assembly.GlobalNamespace);
        }
    }
}
