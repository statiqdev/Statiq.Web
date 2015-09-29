using System;
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
        private Func<IMetadata, string> _writePath = md =>
        {
            IDocument ns = md.Get<IDocument>("ContainingNamespace");

            // Make namespaces output to the index page
            if (md.String("Kind") == "Namespace")
            {
                return ns == null ? "index.html" : $"{md["DisplayName"]}\\index.html";
            }

            // Account both for types that don't have a containing namespace as well as those contained in the global namespace
            return (ns?["ContainingNamespace"] == null) 
                ? $"{md["SymbolId"]}.html" 
                : $"{ns["DisplayName"]}\\{md["SymbolId"]}.html";
        };

        // Use an intermediate Dictionary to initialize with defaults
        private readonly ConcurrentDictionary<string, string> _cssClasses 
            = new ConcurrentDictionary<string, string>(
                new Dictionary<string, string>
                {
                    { "table", "table" }
                }); 
         
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
            AnalyzeSymbolVisitor visitor = new AnalyzeSymbolVisitor(context, _writePath, _cssClasses);
            visitor.Visit(compilation.Assembly.GlobalNamespace);
            return visitor.GetNamespaceOrTypeDocuments();
        }

        // While converting XML documentation to HTML, any tags with the specified name will get the specified CSS class(s)
        // Separate multiple CSS classes with a space (just like you would in HTML)
        public AnalyzeCSharp WithCssClasses(string tagName, string cssClasses)
        {
            if (tagName == null)
            {
                throw new ArgumentNullException(nameof(tagName));
            }
            if (string.IsNullOrWhiteSpace(cssClasses))
            {
                _cssClasses.TryRemove(tagName, out cssClasses);
            }
            else
            {
                _cssClasses[tagName] = cssClasses;
            }
            return this;
        }

        // This changes the default behavior for the WritePath metadata value added to every document
        // Default behavior is to place files in a path with the same name as their containing namespace
        // Namespace documents will be named "index.html" while other documents will get a name equal to their SymbolId
        public AnalyzeCSharp WithWritePath(Func<IMetadata, string> writePath)
        {
            if (writePath == null)
            {
                throw new ArgumentNullException(nameof(writePath));
            }
            _writePath = writePath;
            return this;
        }
    }
}
