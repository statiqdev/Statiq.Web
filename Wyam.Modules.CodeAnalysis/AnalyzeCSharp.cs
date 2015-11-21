using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Pipelines;

namespace Wyam.Modules.CodeAnalysis
{
    /// <summary>
    /// Performs static code analysis on the input documents, outputting a new document for each symbol.
    /// </summary>
    /// <remarks>
    /// This module acts as the basis for code analysis scenarios such as generating source code documentation.
    /// All input documents are assumed to contain C# source in their content and are use to create a Roslyn
    /// compilation. All symbols (namespaces, types, members, etc.) in the compilation are then recursively 
    /// processed and output from this module as documents, one per symbol. The output documents have empty content
    /// and all information about the symbol is contained in the metadata. This lets you pass the output documents
    /// for each symbol on to a template engine like Razor and generate pages for each symbol by having the
    /// template use the document metadata.
    /// </remarks>
    /// <include file="Documentation.xml" path="/Documentation/AnalyzeCSharp/*" />
    /// <category>Metadata</category>
    public class AnalyzeCSharp : IModule
    {
        private Func<ISymbol, bool> _symbolPredicate;
        private Func<IMetadata, string> _writePath;
        private string _writePathPrefix = string.Empty;
        private bool _docsForImplicitSymbols = false;

        // Use an intermediate Dictionary to initialize with defaults
        private readonly ConcurrentDictionary<string, string> _cssClasses
            = new ConcurrentDictionary<string, string>(
                new Dictionary<string, string>
                {
                    { "table", "table" }
                });
        
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
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
            CSharpCompilation compilation = CSharpCompilation
                .Create("CodeAnalysisModule", syntaxTrees)
                .WithReferences(mscorlib)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                    xmlReferenceResolver: new XmlFileResolver(context.InputFolder)));

            // Get and return the document tree
            AnalyzeSymbolVisitor visitor = new AnalyzeSymbolVisitor(context, _symbolPredicate,
                _writePath ?? (x => DefaultWritePath(x, _writePathPrefix)), _cssClasses, _docsForImplicitSymbols);
            visitor.Visit(compilation.Assembly.GlobalNamespace);
            return visitor.Finish();
        }

        /// <summary>
        /// By default, XML documentation comments are not parsed and rendered for documents that are not part
        /// of the initial result set. This can control that behavior and be used to generate documentation
        /// metadata for all documents, regardless if they were part of the initial result set.
        /// </summary>
        /// <param name="docsForImplicitSymbols">If set to <c>true</c>, documentation metadata is generated for XML comments on all symbols.</param>
        public AnalyzeCSharp WithDocsForImplicitSymbols(bool docsForImplicitSymbols = true)
        {
            _docsForImplicitSymbols = docsForImplicitSymbols;
            return this;
        }

        /// <summary>
        /// Controls which symbols are processed as part of the initial result set.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the symbol should be included in the initial result set.</param>
        public AnalyzeCSharp WhereSymbol(Func<ISymbol, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            Func<ISymbol, bool> currentPredicate = _symbolPredicate;
            _symbolPredicate = currentPredicate == null ? predicate : x => currentPredicate(x) && predicate(x);
            return this;
        }
        
        /// <summary>
        /// Limits symbols in the initial result set to those in the specified namespaces.
        /// </summary>
        /// <param name="includeGlobal">If set to <c>true</c>, symbols in the unnamed global namespace are included.</param>
        /// <param name="namespaces">The namespaces to include symbols from (if <c>namespaces</c> is <c>null</c>, symbols from all
        /// namespaces are included).</param>
        public AnalyzeCSharp WhereNamespaces(bool includeGlobal, params string[] namespaces)
        {
            return WhereSymbol(x =>
            {
                INamespaceSymbol namespaceSymbol = x as INamespaceSymbol;
                if (namespaceSymbol == null)
                {
                    return x.ContainingNamespace != null
                        && (namespaces.Length == 0 || namespaces.Any(y => x.ContainingNamespace.ToString().StartsWith(y)));
                }
                if (namespaces.Length == 0)
                {
                    return includeGlobal || !namespaceSymbol.IsGlobalNamespace;
                }
                return (includeGlobal && ((INamespaceSymbol)x).IsGlobalNamespace)
                    || namespaces.Any(y => x.ToString().StartsWith(y));
            });
        }

        /// <summary>
        /// Limits symbols in the initial result set to those in the namespaces that satisfy the specified predicate.
        /// </summary>
        /// <param name="predicate">A predicate that returns true if symbols in the namespace should be included.</param>
        public AnalyzeCSharp WhereNamespaces(Func<string, bool> predicate)
        {
            return WhereSymbol(x =>
            {
                INamespaceSymbol namespaceSymbol = x as INamespaceSymbol;
                if (namespaceSymbol == null)
                {
                    return x.ContainingNamespace != null && predicate(x.ContainingNamespace.ToString());
                }
                return predicate(namespaceSymbol.ToString());
            });
        }
        
        /// <summary>
        /// Limits symbols in the initial result set to those that are public (and optionally protected)
        /// </summary>
        /// <param name="includeProtected">If set to <c>true</c>, protected symbols are also included.</param>
        public AnalyzeCSharp WherePublic(bool includeProtected = true)
        {
            return WhereSymbol(x => x.DeclaredAccessibility == Accessibility.Public
                || (includeProtected && x.DeclaredAccessibility == Accessibility.Protected)
                || x.DeclaredAccessibility == Accessibility.NotApplicable);
        }
        
        /// <summary>
        /// While converting XML documentation to HTML, any tags with the specified name will get the specified CSS class(s).
        /// This is helpful to style your XML documentation comment rendering to support the stylesheet of your site.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="cssClasses">The CSS classes to set for the specified tag name. Separate multiple CSS classes 
        /// with a space (just like you would in HTML).</param>
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
        
        /// <summary>
        /// This changes the default behavior for the generated <c>WritePath</c> metadata value, which is to place files in a path 
        /// with the same name as their containing namespace. Namespace documents will be named "index.html" while other type documents 
        /// will get a name equal to their SymbolId. Member documents will get the same name as their containing type plus an 
        /// anchor to their SymbolId. Note that the default scheme makes the assumption that members will not have their own files, 
        /// if that's not the case a new WritePath function will have to be supplied using this method.
        /// </summary>
        /// <param name="writePath">A function that takes the metadata for a given symbol and returns a <c>string</c> to 
        /// use for the <c>WritePath</c> metadata value.</param>
        public AnalyzeCSharp WithWritePath(Func<IMetadata, string> writePath)
        {
            _writePath = writePath;
            return this;
        }
        
        /// <summary>
        /// This lets you add a prefix to the default <c>WritePath</c> behavior (such as nesting symbol documents inside 
        /// a folder like "api/"). This method has no effect if you've supplied a custom <c>WritePath</c> behavior.
        /// </summary>
        /// <param name="prefix">The prefix to use for each generated <c>WritePath</c>.</param>
        public AnalyzeCSharp WithWritePathPrefix(string prefix)
        {
            if (prefix == null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }
            _writePathPrefix = prefix;
            return this;
        }

        private string DefaultWritePath(IMetadata metadata, string prefix)
        {
            IDocument namespaceDocument = metadata.Get<IDocument>(CodeAnalysisKeys.ContainingNamespace);

            // Namespaces output to the index page in a folder of their full name
            if (metadata.String(CodeAnalysisKeys.Kind) == SymbolKind.Namespace.ToString())
            {
                // If this namespace does not have a containing namespace, it's the global namespace
                return Path.Combine(prefix, namespaceDocument == null ? "global\\index.html" : $"{metadata[CodeAnalysisKeys.DisplayName]}\\index.html");
            }

            // Types output to the index page in a folder of their SymbolId under the folder for their namespace
            if (metadata.String(CodeAnalysisKeys.Kind) == SymbolKind.NamedType.ToString())
            {
                // If containing namespace is null (shouldn't happen) or our namespace is global, output to root folder
                return Path.Combine(prefix, (namespaceDocument?[CodeAnalysisKeys.ContainingNamespace] == null)
                    ? $"global\\{metadata[CodeAnalysisKeys.SymbolId]}\\index.html"
                    : $"{namespaceDocument[CodeAnalysisKeys.DisplayName]}\\{metadata[CodeAnalysisKeys.SymbolId]}\\index.html");
            }

            // Members output to a page equal to their SymbolId under the folder for their type
            IDocument containingTypeDocument = metadata.Get<IDocument>(CodeAnalysisKeys.ContainingType, null);
            return containingTypeDocument?.String(Keys.WritePath)
                .Replace("index.html", metadata.String(CodeAnalysisKeys.SymbolId) + ".html");
        }
    }
}
