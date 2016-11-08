using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Wyam.CodeAnalysis.Analysis;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;

namespace Wyam.CodeAnalysis
{
    public abstract class CodeAnalysisModule<TModule> : IModule 
        where TModule : CodeAnalysisModule<TModule>
    {
        private Func<ISymbol, bool> _symbolPredicate;
        private Func<IMetadata, FilePath> _writePath;
        private DirectoryPath _writePathPrefix = null;
        private bool _docsForImplicitSymbols = false;

        // Use an intermediate Dictionary to initialize with defaults
        private readonly ConcurrentDictionary<string, string> _cssClasses
            = new ConcurrentDictionary<string, string>(
                new Dictionary<string, string>
                {
                    { "table", "table" }
                });

        public abstract IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context);
        
        protected IEnumerable<IDocument> Execute(Compilation compilation, IEnumerable<ISymbol> symbols, IExecutionContext context)
        {
            // Get and return the document tree
            AnalyzeSymbolVisitor visitor = new AnalyzeSymbolVisitor(context, _symbolPredicate,
                _writePath ?? (x => DefaultWritePath(x, _writePathPrefix)), _cssClasses, _docsForImplicitSymbols);
            foreach (ISymbol symbol in symbols)
            {
                visitor.Visit(symbol);
            }
            return visitor.Finish();
        }

        /// <summary>
        /// By default, XML documentation comments are not parsed and rendered for documents that are not part
        /// of the initial result set. This can control that behavior and be used to generate documentation
        /// metadata for all documents, regardless if they were part of the initial result set.
        /// </summary>
        /// <param name="docsForImplicitSymbols">If set to <c>true</c>, documentation metadata is generated for XML comments on all symbols.</param>
        public TModule WithDocsForImplicitSymbols(bool docsForImplicitSymbols = true)
        {
            _docsForImplicitSymbols = docsForImplicitSymbols;
            return (TModule)this;
        }

        /// <summary>
        /// Controls which symbols are processed as part of the initial result set.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the symbol should be included in the initial result set.</param>
        public TModule WhereSymbol(Func<ISymbol, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            Func<ISymbol, bool> currentPredicate = _symbolPredicate;
            _symbolPredicate = currentPredicate == null ? predicate : x => currentPredicate(x) && predicate(x);
            return (TModule)this;
        }

        /// <summary>
        /// Restricts the initial result set to named type symbols (I.e., classes, interfaces, etc.). Also allows supplying
        /// an additional predicate on the named type.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the symbol should be included in the initial result set.</param>
        public TModule WithNamedTypes(Func<INamedTypeSymbol, bool> predicate = null)
        {
            Func<ISymbol, bool> newPredicate = x =>
            {
                INamedTypeSymbol namedTypeSymbol = x as INamedTypeSymbol;
                return namedTypeSymbol != null &&
                       (predicate?.Invoke(namedTypeSymbol) ?? true);
            };
            Func<ISymbol, bool> currentPredicate = _symbolPredicate;
            _symbolPredicate = currentPredicate == null ? newPredicate : x => currentPredicate(x) && newPredicate(x);
            return (TModule)this;
        }

        /// <summary>
        /// Limits symbols in the initial result set to those in the specified namespaces.
        /// </summary>
        /// <param name="includeGlobal">If set to <c>true</c>, symbols in the unnamed global namespace are included.</param>
        /// <param name="namespaces">The namespaces to include symbols from (if <c>namespaces</c> is <c>null</c>, symbols from all
        /// namespaces are included).</param>
        public TModule WhereNamespaces(bool includeGlobal, params string[] namespaces)
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
        public TModule WhereNamespaces(Func<string, bool> predicate)
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
        public TModule WherePublic(bool includeProtected = true)
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
        public TModule WithCssClasses(string tagName, string cssClasses)
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
            return (TModule)this;
        }
        
        /// <summary>
        /// This changes the default behavior for the generated <c>WritePath</c> metadata value, which is to place files in a path 
        /// with the same name as their containing namespace. Namespace documents will be named "index.html" while other type documents 
        /// will get a name equal to their SymbolId. Member documents will get the same name as their containing type plus an 
        /// anchor to their SymbolId. Note that the default scheme makes the assumption that members will not have their own files, 
        /// if that's not the case a new WritePath function will have to be supplied using this method.
        /// </summary>
        /// <param name="writePath">A function that takes the metadata for a given symbol and returns a <c>FilePath</c> to 
        /// use for the <c>WritePath</c> metadata value.</param>
        public TModule WithWritePath(Func<IMetadata, FilePath> writePath)
        {
            _writePath = writePath;
            return (TModule)this;
        }
        
        /// <summary>
        /// This lets you add a prefix to the default <c>WritePath</c> behavior (such as nesting symbol documents inside 
        /// a folder like "api/"). Whatever you supply will be combined with the <c>WritePath</c>. This method has no 
        /// effect if you've supplied a custom <c>WritePath</c> behavior.
        /// </summary>
        /// <param name="prefix">The prefix to use for each generated <c>WritePath</c>.</param>
        public TModule WithWritePathPrefix(DirectoryPath prefix)
        {
            _writePathPrefix = prefix;
            return (TModule)this;
        }

        private FilePath DefaultWritePath(IMetadata metadata, DirectoryPath prefix)
        {
            IDocument namespaceDocument = metadata.Get<IDocument>(CodeAnalysisKeys.ContainingNamespace);
            FilePath writePath = null;

            // Namespaces output to the index page in a folder of their full name
            if (metadata.String(CodeAnalysisKeys.Kind) == SymbolKind.Namespace.ToString())
            {
                // If this namespace does not have a containing namespace, it's the global namespace
                writePath = new FilePath(namespaceDocument == null ? "global/index.html" : $"{metadata[CodeAnalysisKeys.DisplayName]}/index.html");
            }
            // Types output to the index page in a folder of their SymbolId under the folder for their namespace
            else if (metadata.String(CodeAnalysisKeys.Kind) == SymbolKind.NamedType.ToString())
            {
                // If containing namespace is null (shouldn't happen) or our namespace is global, output to root folder
                writePath = new FilePath(namespaceDocument?[CodeAnalysisKeys.ContainingNamespace] == null
                    ? $"global/{metadata[CodeAnalysisKeys.SymbolId]}/index.html"
                    : $"{namespaceDocument[CodeAnalysisKeys.DisplayName]}/{metadata[CodeAnalysisKeys.SymbolId]}/index.html");
            }
            else
            {
                // Members output to a page equal to their SymbolId under the folder for their type
                IDocument containingTypeDocument = metadata.Get<IDocument>(CodeAnalysisKeys.ContainingType, null);
                string containingPath = containingTypeDocument.FilePath(Keys.WritePath).FullPath;
                if (prefix != null && containingPath.StartsWith(prefix.FullPath + "/"))
                {
                    containingPath = containingPath.Substring(prefix.FullPath.Length + 1);
                }
                writePath = new FilePath(containingPath.Replace("index.html", metadata.String(CodeAnalysisKeys.SymbolId) + ".html"));
            }

            // Add the prefix
            if (prefix != null)
            {
                writePath = prefix.CombineFile(writePath);
            }

            return writePath;
        }
    }
}