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
        private Func<IMetadata, string> _writePath = metadata =>
        {
            IDocument namespaceDocument = metadata.Get<IDocument>(MetadataKeys.ContainingNamespace);

            // Namespaces output to the index page in a folder of their full name
            if (metadata.String(MetadataKeys.Kind) == "Namespace")
            {
                // If this namespace does not have a containing namespace, it's the global namespace
                return namespaceDocument == null ? "index.html" : $"{metadata[MetadataKeys.DisplayName]}\\index.html";
            }

            // Types output to the index page in a folder of their SymbolId under the folder for their namespace
            if (metadata.String(MetadataKeys.Kind) == "NamedType")
            {
                // If containing namespace is null (shouldn't happen) or our namespace is global, output to root folder
                return (namespaceDocument?[MetadataKeys.ContainingNamespace] == null)
                    ? $"{metadata[MetadataKeys.SymbolId]}\\index.html"
                    : $"{namespaceDocument[MetadataKeys.DisplayName]}\\{metadata[MetadataKeys.SymbolId]}\\index.html";
            }

            // Members output to a page equal to their SymbolId under the folder for their type
            IDocument containingTypeDocument = metadata.Get<IDocument>(MetadataKeys.ContainingType, null);
            return containingTypeDocument?.String(MetadataKeys.WritePath)
                .Replace("index.html", metadata.String(MetadataKeys.SymbolId) + ".html");
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
            visitor.PostExecution = true;
            return visitor.GetAllDocuments();
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
        // Namespace documents will be named "index.html" while other type documents will get a name equal to their SymbolId
        // Member documents will get the same name as their containing type plus an anchor to their SymbolId
        // Note that this scheme makes the assumption that members will not have their own files, if that's not the case a new WritePath function will have to be supplied
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
