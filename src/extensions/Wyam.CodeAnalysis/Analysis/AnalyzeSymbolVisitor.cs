using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ConcurrentCollections;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Wyam.Common.Caching;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;

namespace Wyam.CodeAnalysis.Analysis
{
    internal class AnalyzeSymbolVisitor : SymbolVisitor
    {
        private readonly ConcurrentDictionary<string, IDocument> _namespaceDisplayNameToDocument = new ConcurrentDictionary<string, IDocument>();
        private readonly ConcurrentDictionary<string, ConcurrentHashSet<INamespaceSymbol>> _namespaceDisplayNameToSymbols = new ConcurrentDictionary<string, ConcurrentHashSet<INamespaceSymbol>>();
        private readonly ConcurrentDictionary<ISymbol, IDocument> _symbolToDocument = new ConcurrentDictionary<ISymbol, IDocument>();
        private readonly ConcurrentHashSet<IMethodSymbol> _extensionMethods = new ConcurrentHashSet<IMethodSymbol>();
        private ImmutableArray<KeyValuePair<INamedTypeSymbol, IDocument>> _namedTypes;  // This contains all of the NamedType symbols and documents obtained during the initial processing

        private readonly Compilation _compilation;
        private readonly IExecutionContext _context;
        private readonly Func<ISymbol, bool> _symbolPredicate;
        private readonly Func<IMetadata, FilePath> _writePath;
        private readonly ConcurrentDictionary<string, string> _cssClasses;
        private readonly bool _docsForImplicitSymbols;
        private readonly bool _assemblySymbols;
        private readonly MethodInfo _getAccessibleMembersInThisAndBaseTypes;
        private readonly Type _documentationCommentCompiler;
        private readonly MethodInfo _documentationCommentCompilerDefaultVisit;
        private bool _finished; // When this is true, we're visiting external symbols and should omit certain metadata and don't descend

        public AnalyzeSymbolVisitor(
            Compilation compilation,
            IExecutionContext context,
            Func<ISymbol, bool> symbolPredicate, 
            Func<IMetadata, FilePath> writePath, 
            ConcurrentDictionary<string, string> cssClasses,
            bool docsForImplicitSymbols,
            bool assemblySymbols)
        {
            _compilation = compilation;
            _context = context;
            _symbolPredicate = symbolPredicate;
            _writePath = writePath;
            _cssClasses = cssClasses;
            _docsForImplicitSymbols = docsForImplicitSymbols;
            _assemblySymbols = assemblySymbols;

            // Get any reflected methods we need
            Assembly reflectedAssembly = typeof(Microsoft.CodeAnalysis.Workspace).Assembly;
            Type reflectedType = reflectedAssembly.GetType("Microsoft.CodeAnalysis.Shared.Extensions.ITypeSymbolExtensions");
            MethodInfo reflectedMethod = reflectedType.GetMethod("GetAccessibleMembersInThisAndBaseTypes");
            _getAccessibleMembersInThisAndBaseTypes = reflectedMethod.MakeGenericMethod(typeof(ISymbol));

            reflectedAssembly = typeof(CSharpCompilation).Assembly;
            _documentationCommentCompiler = reflectedAssembly.GetType("Microsoft.CodeAnalysis.CSharp.DocumentationCommentCompiler");
            _documentationCommentCompilerDefaultVisit = _documentationCommentCompiler.GetMethod("DefaultVisit");
        }

        public IEnumerable<IDocument> Finish()
        {
            _finished = true;
            _namedTypes = _symbolToDocument
                .Where(x => x.Key.Kind == SymbolKind.NamedType)
                .Select(x => new KeyValuePair<INamedTypeSymbol, IDocument>((INamedTypeSymbol)x.Key, x.Value))
                .ToImmutableArray();
            return _symbolToDocument.Select(x => x.Value);
        }

        public override void DefaultVisit(ISymbol symbol)
        {
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                AddDocument(symbol, false, new MetadataItems());
            }

            base.DefaultVisit(symbol);
        }

        public override void VisitAssembly(IAssemblySymbol symbol)
        {
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                AddDocument(symbol, true, new MetadataItems
                {
                    { CodeAnalysisKeys.SpecificKind, _ => symbol.Kind.ToString() },
                    { CodeAnalysisKeys.MemberNamespaces, DocumentsFor(symbol.GlobalNamespace.GetNamespaceMembers()) }
                });
            }

            // Descend if not finished, regardless if this namespace was included
            if (!_finished)
            {
                symbol.GlobalNamespace.Accept(this);
            }
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            // Add to the namespace symbol cache
            string displayName = GetDisplayName(symbol);
            ConcurrentHashSet<INamespaceSymbol> symbols = _namespaceDisplayNameToSymbols.GetOrAdd(displayName, _ => new ConcurrentHashSet<INamespaceSymbol>());
            symbols.Add(symbol);

            // Create the document
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                _namespaceDisplayNameToDocument.AddOrUpdate(displayName,
                    _ => AddNamespaceDocument(symbol, true),
                    (_, existing) =>
                    {
                        // There's already a document for this symbol display name, add it to the symbol-to-document cache
                        _symbolToDocument.TryAdd(symbol, existing);
                        return existing;
                    });
            }

            // Descend if not finished, regardless if this namespace was included
            if (!_finished)
            {
                Parallel.ForEach(symbol.GetMembers(), s => s.Accept(this));
            }
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                MetadataItems metadata = new MetadataItems
                {
                    { CodeAnalysisKeys.SpecificKind, _ => symbol.TypeKind.ToString() },
                    { CodeAnalysisKeys.ContainingType, DocumentFor(symbol.ContainingType) },
                    { CodeAnalysisKeys.MemberTypes, DocumentsFor(symbol.GetTypeMembers()) },
                    { CodeAnalysisKeys.BaseTypes, DocumentsFor(GetBaseTypes(symbol)) },
                    { CodeAnalysisKeys.AllInterfaces, DocumentsFor(symbol.AllInterfaces) },
                    { CodeAnalysisKeys.Members, DocumentsFor(GetAccessibleMembersInThisAndBaseTypes(symbol, symbol).Where(MemberPredicate)) },
                    { CodeAnalysisKeys.ExtensionMethods, _ => DocumentsFor(_extensionMethods.Where(x => x.ReduceExtensionMethod(symbol) != null)) },
                    { CodeAnalysisKeys.Constructors,
                        DocumentsFor(symbol.Constructors.Where(x => !x.IsImplicitlyDeclared)) },
                    { CodeAnalysisKeys.TypeParameters, DocumentsFor(symbol.TypeParameters) },
                    { CodeAnalysisKeys.Accessibility, _ => symbol.DeclaredAccessibility.ToString() },
                    { CodeAnalysisKeys.Attributes, GetAttributeDocuments(symbol) }
                };
                if (!_finished)
                {
                    metadata.AddRange(new []
                    {
                        new MetadataItem(CodeAnalysisKeys.DerivedTypes, _ => GetDerivedTypes(symbol), true),
                        new MetadataItem(CodeAnalysisKeys.ImplementingTypes, _ => GetImplementingTypes(symbol), true)
                    });
                }
                AddDocument(symbol, true, metadata);

                // Descend if not finished, and only if this type was included
                if (!_finished)
                {
                    Parallel.ForEach(symbol.GetMembers()
                        .Where(MemberPredicate)
                        .Concat(symbol.Constructors.Where(x => !x.IsImplicitlyDeclared)),
                        s => s.Accept(this));
                }
            }
        }

        public override void VisitTypeParameter(ITypeParameterSymbol symbol)
        {
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                AddMemberDocument(symbol, false, new MetadataItems
                {
                    new MetadataItem(CodeAnalysisKeys.SpecificKind, _ => symbol.TypeParameterKind.ToString()),
                    new MetadataItem(CodeAnalysisKeys.DeclaringType, DocumentFor(symbol.DeclaringType)),
                    new MetadataItem(CodeAnalysisKeys.Attributes, GetAttributeDocuments(symbol))
                });
            }
        }

        public override void VisitParameter(IParameterSymbol symbol)
        {
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                AddMemberDocument(symbol, false, new MetadataItems
                {
                    new MetadataItem(CodeAnalysisKeys.SpecificKind, _ => symbol.Kind.ToString()),
                    new MetadataItem(CodeAnalysisKeys.Type, DocumentFor(symbol.Type)),
                    new MetadataItem(CodeAnalysisKeys.Attributes, GetAttributeDocuments(symbol))
                });
            }
        }

        public override void VisitMethod(IMethodSymbol symbol)
        {
            // If this is an extension method, record it
            if (!_finished && symbol.IsExtensionMethod)
            {
                _extensionMethods.Add(symbol);
            }

            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                AddMemberDocument(symbol, true, new MetadataItems
                {
                    new MetadataItem(CodeAnalysisKeys.SpecificKind,
                        _ => symbol.MethodKind == MethodKind.Ordinary ? "Method" : symbol.MethodKind.ToString()),
                    new MetadataItem(CodeAnalysisKeys.TypeParameters, DocumentsFor(symbol.TypeParameters)),
                    new MetadataItem(CodeAnalysisKeys.Parameters, DocumentsFor(symbol.Parameters)),
                    new MetadataItem(CodeAnalysisKeys.ReturnType, DocumentFor(symbol.ReturnType)),
                    new MetadataItem(CodeAnalysisKeys.OverriddenMethod, DocumentFor(symbol.OverriddenMethod)),
                    new MetadataItem(CodeAnalysisKeys.Accessibility, _ => symbol.DeclaredAccessibility.ToString()),
                    new MetadataItem(CodeAnalysisKeys.Attributes, GetAttributeDocuments(symbol))
                });
            }
        }
   
        public override void VisitField(IFieldSymbol symbol)
        {
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                AddMemberDocument(symbol, true, new MetadataItems
                {
                    new MetadataItem(CodeAnalysisKeys.SpecificKind, _ => symbol.Kind.ToString()),
                    new MetadataItem(CodeAnalysisKeys.Type, DocumentFor(symbol.Type)),
                    new MetadataItem(CodeAnalysisKeys.HasConstantValue, _ => symbol.HasConstantValue),
                    new MetadataItem(CodeAnalysisKeys.ConstantValue, _ => symbol.ConstantValue),
                    new MetadataItem(CodeAnalysisKeys.Accessibility, _ => symbol.DeclaredAccessibility.ToString()),
                    new MetadataItem(CodeAnalysisKeys.Attributes, GetAttributeDocuments(symbol))
                });
            }
        }

        public override void VisitEvent(IEventSymbol symbol)
        {
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                AddMemberDocument(symbol, true, new MetadataItems
                {
                    new MetadataItem(CodeAnalysisKeys.SpecificKind, _ => symbol.Kind.ToString()),
                    new MetadataItem(CodeAnalysisKeys.Type, DocumentFor(symbol.Type)),
                    new MetadataItem(CodeAnalysisKeys.OverriddenMethod, DocumentFor(symbol.OverriddenEvent)),
                    new MetadataItem(CodeAnalysisKeys.Accessibility, _ => symbol.DeclaredAccessibility.ToString())
                });
            }
        }

        public override void VisitProperty(IPropertySymbol symbol)
        {
            if (_finished || _symbolPredicate == null || _symbolPredicate(symbol))
            {
                AddMemberDocument(symbol, true, new MetadataItems
                {
                    new MetadataItem(CodeAnalysisKeys.SpecificKind, _ => symbol.Kind.ToString()),
                    new MetadataItem(CodeAnalysisKeys.Parameters, DocumentsFor(symbol.Parameters)),
                    new MetadataItem(CodeAnalysisKeys.Type, DocumentFor(symbol.Type)),
                    new MetadataItem(CodeAnalysisKeys.OverriddenMethod, DocumentFor(symbol.OverriddenProperty)),
                    new MetadataItem(CodeAnalysisKeys.Accessibility, _ => symbol.DeclaredAccessibility.ToString()),
                    new MetadataItem(CodeAnalysisKeys.Attributes, GetAttributeDocuments(symbol))
                });
            }
        }

        // Helpers below...

        // This was helpful: http://stackoverflow.com/a/30445814/807064
        private IEnumerable<ISymbol> GetAccessibleMembersInThisAndBaseTypes(ITypeSymbol containingType, ISymbol within)
        {
            List<ISymbol> members = ((IEnumerable<ISymbol>)_getAccessibleMembersInThisAndBaseTypes.Invoke(null, new object[] { containingType, within })).ToList();
           
            // Remove overridden symbols
            ImmutableHashSet<ISymbol> remove = members
                .Select(x => (ISymbol)(x as IMethodSymbol)?.OverriddenMethod ?? (x as IPropertySymbol)?.OverriddenProperty)
                .Where(x => x != null)
                .ToImmutableHashSet();
            members.RemoveAll(x => remove.Contains(x));
            return members;
        }

        internal static IEnumerable<INamedTypeSymbol> GetBaseTypes(ITypeSymbol type)
        {
            INamedTypeSymbol current = type.BaseType;
            while (current != null)
            {
                yield return current;
                current = current.BaseType;
            }
        }

        private bool MemberPredicate(ISymbol symbol)
        {
            IPropertySymbol propertySymbol = symbol as IPropertySymbol;
            if (propertySymbol != null && propertySymbol.IsIndexer)
            {
                // Special case for indexers
                return true;
            }
            return symbol.CanBeReferencedByName && !symbol.IsImplicitlyDeclared;
        }

        private IDocument AddMemberDocument(ISymbol symbol, bool xmlDocumentation, MetadataItems items)
        {
            items.AddRange(new[]
            {
                new MetadataItem(CodeAnalysisKeys.ContainingType, DocumentFor(symbol.ContainingType))
            });
            return AddDocument(symbol, xmlDocumentation, items);
        }

        private IDocument AddNamespaceDocument(INamespaceSymbol symbol, bool xmlDocumentation)
        {
            string displayName = GetDisplayName(symbol);
            MetadataItems items = new MetadataItems
            {
                {CodeAnalysisKeys.Symbol, _ => _namespaceDisplayNameToSymbols[displayName].ToImmutableList()},
                {CodeAnalysisKeys.SpecificKind, _ => symbol.Kind.ToString()},
                // We need to aggregate the results across all matching namespaces
                {CodeAnalysisKeys.MemberNamespaces, DocumentsFor(_namespaceDisplayNameToSymbols[displayName].SelectMany(x => x.GetNamespaceMembers()))},
                {CodeAnalysisKeys.MemberTypes, DocumentsFor(_namespaceDisplayNameToSymbols[displayName].SelectMany(x => x.GetTypeMembers()))}
            };
            return AddDocumentCommon(symbol, xmlDocumentation, items);
        }

        // Used for everything but namespace documents
        private IDocument AddDocument(ISymbol symbol, bool xmlDocumentation, MetadataItems items)
        {
            items.AddRange(new[]
            {
                new MetadataItem(CodeAnalysisKeys.Symbol, symbol)
            });

            // Add the containing assembly, but only if it's not the code analysis compilation
            if (symbol.ContainingAssembly?.Name != AnalyzeCSharp.CompilationAssemblyName && _assemblySymbols)
            {
                items.Add(new MetadataItem(CodeAnalysisKeys.ContainingAssembly, DocumentFor(symbol.ContainingAssembly)));
            }

            return AddDocumentCommon(symbol, xmlDocumentation, items);
        }

        private IDocument AddDocumentCommon(ISymbol symbol, bool xmlDocumentation, MetadataItems items)
        {
            // Get universal metadata
            string commentId = symbol.GetDocumentationCommentId();
            items.AddRange(new []
            {
                // In general, cache the values that need calculation and don't cache the ones that are just properties of ISymbol
                new MetadataItem(CodeAnalysisKeys.IsResult, !_finished),
                new MetadataItem(CodeAnalysisKeys.SymbolId, _ => GetId(symbol), true),
                new MetadataItem(CodeAnalysisKeys.CommentId, commentId),
                new MetadataItem(CodeAnalysisKeys.Name, metadata => string.IsNullOrEmpty(symbol.Name) ? metadata.String(CodeAnalysisKeys.FullName) : symbol.Name),
                new MetadataItem(CodeAnalysisKeys.FullName, _ => GetFullName(symbol), true),
                new MetadataItem(CodeAnalysisKeys.DisplayName, _ => GetDisplayName(symbol), true),
                new MetadataItem(CodeAnalysisKeys.QualifiedName, _ => GetQualifiedName(symbol), true),
                new MetadataItem(CodeAnalysisKeys.Kind, _ => symbol.Kind.ToString()),
                new MetadataItem(CodeAnalysisKeys.ContainingNamespace, DocumentFor(symbol.ContainingNamespace)),
                new MetadataItem(CodeAnalysisKeys.Syntax, _ => GetSyntax(symbol), true),
                new MetadataItem(CodeAnalysisKeys.IsStatic, _ => symbol.IsStatic ),
                new MetadataItem(CodeAnalysisKeys.IsAbstract, _ => symbol.IsAbstract ),
                new MetadataItem(CodeAnalysisKeys.IsVirtual, _ => symbol.IsVirtual ),
                new MetadataItem(CodeAnalysisKeys.IsOverride, _ => symbol.IsOverride )
            });
            
            // Add metadata that's specific to initially-processed symbols
            if (!_finished)
            {
                items.AddRange(new[]
                {
                    new MetadataItem(Keys.WritePath, x => _writePath(x), true),
                    new MetadataItem(Keys.RelativeFilePath, x => x.FilePath(Keys.WritePath)),
                    new MetadataItem(Keys.RelativeFilePathBase, x =>
                    {
                        FilePath writePath = x.FilePath(Keys.WritePath);
                        return writePath.Directory.CombineFile(writePath.FileNameWithoutExtension);
                    }),
                    new MetadataItem(Keys.RelativeFileDir, x => x.FilePath(Keys.WritePath).Directory)
                });
            }

            // XML Documentation
            if (xmlDocumentation && (!_finished || _docsForImplicitSymbols))
            {
                AddXmlDocumentation(symbol, items);
            }

            // Create the document and add it to caches
            IDocument document = _symbolToDocument.GetOrAdd(symbol, 
                _ => _context.GetDocument(new FilePath((Uri)null, symbol.ToDisplayString(), PathKind.Absolute), null, items));

            return document;
        }

        private void AddXmlDocumentation(ISymbol symbol, MetadataItems metadata)
        {
            INamespaceSymbol namespaceSymbol = symbol as INamespaceSymbol;
            string documentationCommentXml = namespaceSymbol == null
                ? symbol.GetDocumentationCommentXml(expandIncludes: true)
                : GetNamespaceDocumentationCommentXml(namespaceSymbol);
            XmlDocumentationParser xmlDocumentationParser
                = new XmlDocumentationParser(_context, symbol, _compilation, _symbolToDocument, _cssClasses);
            IEnumerable<string> otherHtmlElementNames = xmlDocumentationParser.Parse(documentationCommentXml);

            // Add standard HTML elements
            metadata.AddRange(new []
            {
                new MetadataItem(CodeAnalysisKeys.CommentXml, documentationCommentXml),
                new MetadataItem(CodeAnalysisKeys.Example, _ => xmlDocumentationParser.Process().Example),
                new MetadataItem(CodeAnalysisKeys.Remarks, _ => xmlDocumentationParser.Process().Remarks),
                new MetadataItem(CodeAnalysisKeys.Summary, _ => xmlDocumentationParser.Process().Summary),
                new MetadataItem(CodeAnalysisKeys.Returns, _ => xmlDocumentationParser.Process().Returns),
                new MetadataItem(CodeAnalysisKeys.Value, _ => xmlDocumentationParser.Process().Value),
                new MetadataItem(CodeAnalysisKeys.Exceptions, _ => xmlDocumentationParser.Process().Exceptions),
                new MetadataItem(CodeAnalysisKeys.Permissions, _ => xmlDocumentationParser.Process().Permissions),
                new MetadataItem(CodeAnalysisKeys.Params, _ => xmlDocumentationParser.Process().Params),
                new MetadataItem(CodeAnalysisKeys.TypeParams, _ => xmlDocumentationParser.Process().TypeParams),
                new MetadataItem(CodeAnalysisKeys.SeeAlso, _ => xmlDocumentationParser.Process().SeeAlso)
            });

            // Add other HTML elements with keys of [ElementName]Html
            metadata.AddRange(otherHtmlElementNames.Select(x => 
                new MetadataItem(FirstLetterToUpper(x) + "Comments",
                    _ => xmlDocumentationParser.Process().OtherComments[x])));
        }

        // This can be removed once changes in https://github.com/dotnet/roslyn/pull/15494 are merged and deployed
        private string GetNamespaceDocumentationCommentXml(INamespaceSymbol symbol)
        {
            // Try and get comments applied to the namespace
            TextWriter writer = new StringWriter();
            CancellationToken ct = new CancellationToken();
            object documentationCompiler = Activator.CreateInstance(_documentationCommentCompiler, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[]
            {
                (string)null,
                _compilation,
                writer,
                (SyntaxTree)null,
                (TextSpan?)null,
                true,
                true,
                null,
                ct
            }, null);
            _documentationCommentCompilerDefaultVisit.Invoke(documentationCompiler, new object[] { symbol });
            string docs = writer.ToString();

            // Fall back to looking for a NamespaceDoc class
            if (string.IsNullOrEmpty(docs))
            {
                INamespaceOrTypeSymbol namespaceDoc = symbol.GetMembers("NamespaceDoc").FirstOrDefault();
                if (namespaceDoc != null)
                {
                    return namespaceDoc.GetDocumentationCommentXml(expandIncludes: true);
                }
            }

            return docs ?? string.Empty;
        }

        public static string FirstLetterToUpper(string str)
        {
            if (str == null)
            {
                return null;
            }

            if (str.Length > 1)
            {
                return char.ToUpper(str[0]) + str.Substring(1);
            }

            return str.ToUpper();
        }

        // Note that the symbol ID is not fully-qualified and is therefore only unique within a namespace
        private static string GetId(ISymbol symbol)
        {
            if (symbol is IAssemblySymbol)
            {
                return symbol.Name + ".dll";
            }
            if (symbol is INamespaceOrTypeSymbol)
            {
                char[] id = symbol.MetadataName.ToCharArray();
                for (int c = 0; c < id.Length; c++)
                {
                    if (!char.IsLetterOrDigit(id[c]) && id[c] != '-' && id[c] != '.')
                    {
                        id[c] = '_';
                    }
                }
                return new string(id);
            }

            // Get a hash for anything other than namespaces or types
            return BitConverter.ToString(BitConverter.GetBytes(Crc32.Calculate(symbol.GetDocumentationCommentId() ?? GetFullName(symbol)))).Replace("-", string.Empty);
        }

        internal static string GetFullName(ISymbol symbol)
        {
            return symbol.ToDisplayString(new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                parameterOptions: SymbolDisplayParameterOptions.IncludeType, 
                memberOptions: SymbolDisplayMemberOptions.IncludeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes));
        }

        private static string GetQualifiedName(ISymbol symbol)
        {
            return symbol.ToDisplayString(new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters));
        }

        private static string GetDisplayName(ISymbol symbol)
        {
            if (symbol is IAssemblySymbol)
            {
                // Add .dll to assembly names
                return symbol.Name + ".dll";
            }
            if (symbol.Kind == SymbolKind.Namespace)
            {
                // Use "global" for the global namespace display name since it's a reserved keyword and it's used to refer to the global namespace in code
                return symbol.ContainingNamespace == null ? "global" : GetQualifiedName(symbol);
            }
            return GetFullName(symbol);
        }

        private IReadOnlyList<IDocument> GetDerivedTypes(INamedTypeSymbol symbol)
        {
            return _namedTypes
                .Where(x => x.Key.BaseType != null && x.Key.BaseType.Equals(symbol))
                .Select(x => x.Value)
                .ToImmutableArray();
        }

        private IReadOnlyList<IDocument> GetImplementingTypes(INamedTypeSymbol symbol)
        {
            return _namedTypes
                .Where(x => x.Key.AllInterfaces.Contains(symbol))
                .Select(x => x.Value)
                .ToImmutableArray();
        }

        private string GetSyntax(ISymbol symbol)
        {
            return SyntaxHelper.GetSyntax(symbol);
        }

        private IReadOnlyList<IDocument> GetAttributeDocuments(ISymbol symbol)
        {
            return symbol.GetAttributes().Select(attributeData => _context.GetDocument(new MetadataItems
            {
                { CodeAnalysisKeys.AttributeData, attributeData },
                { CodeAnalysisKeys.Type, DocumentFor(attributeData.AttributeClass) },
                { CodeAnalysisKeys.Name, attributeData.AttributeClass.Name }
            })).ToList();
        }

        private SymbolDocumentValue DocumentFor(ISymbol symbol)
        {
            return new SymbolDocumentValue(symbol, this);
        }

        private SymbolDocumentValues DocumentsFor(IEnumerable<ISymbol> symbols)
        {
            return new SymbolDocumentValues(symbols, this);
        }

        public bool TryGetDocument(ISymbol symbol, out IDocument document)
        {
            return _symbolToDocument.TryGetValue(symbol, out document);
        }
    }
}