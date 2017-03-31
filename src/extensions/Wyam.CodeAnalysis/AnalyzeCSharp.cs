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
using Wyam.CodeAnalysis.Analysis;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Tracing;
using Wyam.Common.Util;

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
    /// <metadata cref="Keys.RelativeFilePath" usage="Output">
    /// A generated relative file path specific to the symbol.
    /// </metadata>
    /// <metadata cref="Keys.RelativeFilePathBase" usage="Output">
    /// A generated relative file path without extension specific to the symbol.
    /// </metadata>
    /// <metadata cref="Keys.RelativeFileDir" usage="Output">
    /// A generated relative file directory specific to the symbol.
    /// </metadata>
    /// <metadata cref="Keys.WritePath" usage="Output">
    /// A generated write file path specific to the symbol.
    /// </metadata>
    /// <include file="Documentation.xml" path="/Documentation/AnalyzeCSharp/*" />
    /// <category>Metadata</category>
    public class AnalyzeCSharp : IModule
    {
        internal const string CompilationAssemblyName = nameof(CompilationAssemblyName);

        // Use an intermediate Dictionary to initialize with defaults
        private readonly ConcurrentDictionary<string, string> _cssClasses
            = new ConcurrentDictionary<string, string>(
                new Dictionary<string, string>
                {
                    { "table", "table" }
                });

        private readonly List<string> _assemblyGlobs = new List<string>();

        private Func<ISymbol, bool> _symbolPredicate;
        private Func<IMetadata, FilePath> _writePath;
        private DirectoryPath _writePathPrefix = null;
        private bool _docsForImplicitSymbols = false;
        private bool _inputDocuments = true;
        private bool _assemblySymbols = false;

        /// <summary>
        /// By default, XML documentation comments are not parsed and rendered for documents that are not part
        /// of the initial result set. This can control that behavior and be used to generate documentation
        /// metadata for all documents, regardless if they were part of the initial result set.
        /// </summary>
        /// <param name="docsForImplicitSymbols">If set to <c>true</c>, documentation metadata is generated for XML comments on all symbols.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithDocsForImplicitSymbols(bool docsForImplicitSymbols = true)
        {
            _docsForImplicitSymbols = docsForImplicitSymbols;
            return this;
        }

        /// <summary>
        /// Controls whether the content of input documents is treated as code and used in the analysis (the default is <c>true</c>).
        /// </summary>
        /// <param name="inputDocuments"><c>true</c> to analyze the content of input documents.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithInputDocuments(bool inputDocuments = true)
        {
            _inputDocuments = inputDocuments;
            return this;
        }

        /// <summary>
        /// Analyzes the specified assemblies.
        /// </summary>
        /// <param name="assemblies">A globbing pattern indicating the assemblies to analyze.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithAssemblies(string assemblies)
        {
            if (!string.IsNullOrEmpty(assemblies))
            {
                _assemblyGlobs.Add(assemblies);
            }
            return this;
        }

        /// <summary>
        /// Analyzes the specified assemblies.
        /// </summary>
        /// <param name="assemblies">Globbing patterns indicating the assemblies to analyze.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithAssemblies(IEnumerable<string> assemblies)
        {
            if (assemblies != null)
            {
                _assemblyGlobs.AddRange(assemblies.Where(x => !string.IsNullOrEmpty(x)));
            }
            return this;
        }

        /// <summary>
        /// Controls which symbols are processed as part of the initial result set.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the symbol should be included in the initial result set.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WhereSymbol(Func<ISymbol, bool> predicate)
        {
            if (predicate != null)
            {
                Func<ISymbol, bool> currentPredicate = _symbolPredicate;
                _symbolPredicate = currentPredicate == null ? predicate : x => currentPredicate(x) && predicate(x);
            }
            return this;
        }

        /// <summary>
        /// Restricts the initial result set to named type symbols (I.e., classes, interfaces, etc.). Also allows supplying
        /// an additional predicate on the named type.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the symbol should be included in the initial result set.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithNamedTypes(Func<INamedTypeSymbol, bool> predicate = null)
        {
            return WhereSymbol(x =>
            {
                INamedTypeSymbol namedTypeSymbol = x as INamedTypeSymbol;
                return namedTypeSymbol != null && (predicate?.Invoke(namedTypeSymbol) ?? true);
            });
        }

        /// <summary>
        /// Limits symbols in the initial result set to those in the specified namespaces.
        /// </summary>
        /// <param name="includeGlobal">If set to <c>true</c>, symbols in the unnamed global namespace are included.</param>
        /// <param name="namespaces">The namespaces to include symbols from (if <c>namespaces</c> is <c>null</c>, symbols from all
        /// namespaces are included).</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WhereNamespaces(bool includeGlobal, params string[] namespaces)
        {
            return WhereSymbol(x =>
            {
                if (x is IAssemblySymbol)
                {
                    return true;
                }
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
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WhereNamespaces(Func<string, bool> predicate)
        {
            return WhereSymbol(x =>
            {
                if (x is IAssemblySymbol)
                {
                    return true;
                }
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
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WherePublic(bool includeProtected = true)
        {
            return WhereSymbol(x =>
            {
                if (x is IAssemblySymbol)
                {
                    return true;
                }
                return x.DeclaredAccessibility == Accessibility.Public
                    || (includeProtected && x.DeclaredAccessibility == Accessibility.Protected)
                    || x.DeclaredAccessibility == Accessibility.NotApplicable;
            });
        }

        /// <summary>
        /// While converting XML documentation to HTML, any tags with the specified name will get the specified CSS class(s).
        /// This is helpful to style your XML documentation comment rendering to support the stylesheet of your site.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="cssClasses">The CSS classes to set for the specified tag name. Separate multiple CSS classes
        /// with a space (just like you would in HTML).</param>
        /// <returns>The current module instance.</returns>
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
        /// Controls whether assembly symbol documents are output.
        /// </summary>
        /// <param name="assemblySymbols"><c>true</c> to output assembly symbol documents.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithAssemblySymbols(bool assemblySymbols = true)
        {
            _assemblySymbols = assemblySymbols;
            return WhereSymbol(x => !(x is IAssemblySymbol) || (_assemblySymbols && x.Name != CompilationAssemblyName));
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
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithWritePath(Func<IMetadata, FilePath> writePath)
        {
            _writePath = writePath;
            return this;
        }

        /// <summary>
        /// This lets you add a prefix to the default <c>WritePath</c> behavior (such as nesting symbol documents inside
        /// a folder like "api/"). Whatever you supply will be combined with the <c>WritePath</c>. This method has no
        /// effect if you've supplied a custom <c>WritePath</c> behavior.
        /// </summary>
        /// <param name="prefix">The prefix to use for each generated <c>WritePath</c>.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithWritePathPrefix(DirectoryPath prefix)
        {
            _writePathPrefix = prefix;
            return this;
        }

        private FilePath DefaultWritePath(IMetadata metadata, DirectoryPath prefix)
        {
            IDocument namespaceDocument = metadata.Document(CodeAnalysisKeys.ContainingNamespace);
            FilePath writePath = null;

            // Assemblies output to the index page in a folder of their name
            if (metadata.String(CodeAnalysisKeys.Kind) == SymbolKind.Assembly.ToString())
            {
                writePath = new FilePath($"{metadata[CodeAnalysisKeys.DisplayName]}/index.html");
            }
            // Namespaces output to the index page in a folder of their full name
            else if (metadata.String(CodeAnalysisKeys.Kind) == SymbolKind.Namespace.ToString())
            {
                // If this namespace does not have a containing namespace, it's the global namespace
                writePath = new FilePath(namespaceDocument == null ? "global/index.html" : $"{metadata[CodeAnalysisKeys.DisplayName]}/index.html");
            }
            // Types output to the index page in a folder of their SymbolId under the folder for their namespace
            else if (metadata.String(CodeAnalysisKeys.Kind) == SymbolKind.NamedType.ToString())
            {
                writePath = new FilePath(namespaceDocument?[CodeAnalysisKeys.ContainingNamespace] == null
                    ? $"global/{metadata[CodeAnalysisKeys.SymbolId]}/index.html"
                    : $"{namespaceDocument[CodeAnalysisKeys.DisplayName]}/{metadata[CodeAnalysisKeys.SymbolId]}/index.html");
            }
            else
            {
                // Members output to a page equal to their SymbolId under the folder for their type
                IDocument containingTypeDocument = metadata.Document(CodeAnalysisKeys.ContainingType);
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

        /// <inheritdoc />
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            List<ISymbol> symbols = new List<ISymbol>();

            // Create the compilation (have to supply an XmlReferenceResolver to handle include XML doc comments)
            MetadataReference mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            Compilation compilation = CSharpCompilation
                .Create(CompilationAssemblyName)
                .WithReferences(mscorlib)
                .WithOptions(new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    xmlReferenceResolver: new XmlFileResolver(context.FileSystem.RootPath.FullPath)));

            // Handle input documents
            if (_inputDocuments)
            {
                // Get syntax trees (supply path so that XML doc includes can be resolved)
                ConcurrentBag<SyntaxTree> syntaxTrees = new ConcurrentBag<SyntaxTree>();
                context.ParallelForEach(inputs, input =>
                {
                    using (Stream stream = input.GetStream())
                    {
                        SourceText sourceText = SourceText.From(stream);
                        syntaxTrees.Add(CSharpSyntaxTree.ParseText(
                            sourceText,
                            path: input.String(Keys.SourceFilePath, string.Empty)));
                    }
                });

                compilation = compilation.AddSyntaxTrees(syntaxTrees);
            }

            // Handle assemblies
            IEnumerable<IFile> assemblyFiles = context.FileSystem.GetInputFiles(_assemblyGlobs)
                .Where(x => (x.Path.Extension == ".dll" || x.Path.Extension == ".exe") && x.Exists);
            MetadataReference[] assemblyReferences = assemblyFiles.Select(assemblyFile =>
            {
                // Create the metadata reference for the compilation
                IFile xmlFile = context.FileSystem.GetFile(assemblyFile.Path.ChangeExtension("xml"));
                if (xmlFile.Exists)
                {
                    Trace.Verbose($"Creating metadata reference for assembly {assemblyFile.Path.FullPath} with XML documentation file");
                    using (Stream xmlStream = xmlFile.OpenRead())
                    {
                        using (MemoryStream xmlBytes = new MemoryStream())
                        {
                            xmlStream.CopyTo(xmlBytes);
                            return MetadataReference.CreateFromStream(
                                assemblyFile.OpenRead(),
                                documentation: XmlDocumentationProvider.CreateFromBytes(xmlBytes.ToArray()));
                        }
                    }
                }
                Trace.Verbose($"Creating metadata reference for assembly {assemblyFile.Path.FullPath} without XML documentation file");
                return (MetadataReference)MetadataReference.CreateFromStream(assemblyFile.OpenRead());
            }).ToArray();
            if (assemblyReferences.Length > 0)
            {
                compilation = compilation.AddReferences(assemblyReferences);
                symbols.AddRange(assemblyReferences
                    .Select(x => (IAssemblySymbol)compilation.GetAssemblyOrModuleSymbol(x))
                    .Select(x => _assemblySymbols ? x : (ISymbol)x.GlobalNamespace));
            }

            // Get and return the document tree
            symbols.Add(compilation.Assembly.GlobalNamespace);
            AnalyzeSymbolVisitor visitor = new AnalyzeSymbolVisitor(
                compilation,
                context,
                _symbolPredicate,
                _writePath ?? (x => DefaultWritePath(x, _writePathPrefix)),
                _cssClasses,
                _docsForImplicitSymbols,
                _assemblySymbols);
            foreach (ISymbol symbol in symbols)
            {
                visitor.Visit(symbol);
            }
            return visitor.Finish();
        }
    }
}