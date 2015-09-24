using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Wyam.Common;

namespace Wyam.Modules.CodeAnalysis
{
    internal class DocumentTreeBuilder
    {
        private readonly ConcurrentBag<IDocument> _allDocuments = new ConcurrentBag<IDocument>();
        private readonly IExecutionContext _context;

        public DocumentTreeBuilder(IExecutionContext context)
        {
            _context = context;
        }

        public IEnumerable<IDocument> BuildDocumentTree(INamespaceSymbol globalNamespaceSymbol)
        {
            GetNamespaceDocuments(globalNamespaceSymbol, null, null);
            return _allDocuments.ToImmutableArray();
        } 

        private void GetNamespaceDocuments(INamespaceSymbol namespaceSymbol, IDocument parentNamespaceDocument, MutableReadOnlyList<IDocument> parentNestedNamespaceDocuments)
        {
            // Create a document for this namespace
            MutableReadOnlyList<IDocument> nestedNamespaceDocuments = new MutableReadOnlyList<IDocument>();
            MutableReadOnlyList<IDocument> typeDocuments = new MutableReadOnlyList<IDocument>();
            List<KeyValuePair<string, object>> metadata = new List<KeyValuePair<string, object>>
            {
                MetadataHelper.New(MetadataKeys.Symbol, namespaceSymbol),
                MetadataHelper.New(MetadataKeys.Name, namespaceSymbol.Name),
                MetadataHelper.New(MetadataKeys.DisplayString, namespaceSymbol.ToDisplayString()),
                MetadataHelper.New(MetadataKeys.Kind, namespaceSymbol.Kind.ToString()),
                MetadataHelper.New(MetadataKeys.NestedNamespaces, nestedNamespaceDocuments),
                MetadataHelper.New(MetadataKeys.Types, typeDocuments)
            };
            if (parentNamespaceDocument != null)
            {
                metadata.Add(MetadataHelper.New(MetadataKeys.ParentNamespace, parentNamespaceDocument));
            }
            IDocument namespaceDocument = _context.GetNewDocument(metadata);
            _allDocuments.Add(namespaceDocument);
            parentNestedNamespaceDocuments?.Add(namespaceDocument);

            // Iterate types in this namespace
            Parallel.ForEach(namespaceSymbol.GetTypeMembers(), x => GetTypeDocuments(x, namespaceDocument, null, typeDocuments, null));
            typeDocuments.MakeImmutable();

            // Iterate nested namespaces
            Parallel.ForEach(namespaceSymbol.GetNamespaceMembers(), x => GetNamespaceDocuments(x, namespaceDocument, nestedNamespaceDocuments));
            nestedNamespaceDocuments.MakeImmutable();
        }

        private void GetTypeDocuments(INamedTypeSymbol typeSymbol, IDocument namespaceDocument, IDocument parentTypeDocument, 
            MutableReadOnlyList<IDocument> namespaceTypeDocuments, MutableReadOnlyList<IDocument> parentTypeDocuments)
        {
            // Create a document for this type
            MutableReadOnlyList<IDocument> nestedTypeDocuments = new MutableReadOnlyList<IDocument>();
            MutableReadOnlyList<IDocument> memberDocuments = new MutableReadOnlyList<IDocument>();
            List<KeyValuePair<string, object>> metadata = new List<KeyValuePair<string, object>>
            {
                MetadataHelper.New(MetadataKeys.Symbol, typeSymbol),
                MetadataHelper.New(MetadataKeys.Name, typeSymbol.Name),
                MetadataHelper.New(MetadataKeys.DisplayString, typeSymbol.ToDisplayString()),
                MetadataHelper.New(MetadataKeys.Kind, typeSymbol.Kind.ToString()),
                MetadataHelper.New(MetadataKeys.Namespace, namespaceDocument),
                MetadataHelper.New(MetadataKeys.NestedTypes, nestedTypeDocuments),
                MetadataHelper.New(MetadataKeys.Members, memberDocuments)
            };
            if (parentTypeDocument != null)
            {
                metadata.Add(MetadataHelper.New(MetadataKeys.ParentType, parentTypeDocument));
            }
            IDocument typeDocument = _context.GetNewDocument(metadata);
            _allDocuments.Add(typeDocument);
            namespaceTypeDocuments.Add(typeDocument);
            parentTypeDocuments?.Add(typeDocument);

            // Iterate members in this type
            Parallel.ForEach(typeSymbol.GetMembers().Where(x => x.CanBeReferencedByName), x => GetMemberDocument(x, typeDocument, memberDocuments));
            memberDocuments.MakeImmutable();

            // Iterate nested types
            Parallel.ForEach(typeSymbol.GetTypeMembers(), x => GetTypeDocuments(x, namespaceDocument, typeDocument, namespaceTypeDocuments, nestedTypeDocuments));
            nestedTypeDocuments.MakeImmutable();
        }

        private void GetMemberDocument(ISymbol memberSymbol, IDocument typeDocument, MutableReadOnlyList<IDocument> parentMemberDocuments)
        {
            // Create a document for this member
            IDocument memberDocument = _context.GetNewDocument(new[]
            {
                MetadataHelper.New(MetadataKeys.Symbol, memberSymbol),
                MetadataHelper.New(MetadataKeys.Name, memberSymbol.Name),
                MetadataHelper.New(MetadataKeys.DisplayString, memberSymbol.ToDisplayString()),
                MetadataHelper.New(MetadataKeys.Kind, memberSymbol.Kind.ToString()),
                MetadataHelper.New(MetadataKeys.Type, typeDocument)
            });
            _allDocuments.Add(memberDocument);
            parentMemberDocuments.Add(memberDocument);
        }
    }

    // Note that if we ever introduce code analysis for other formats (such as Java/Javadoc or CSS), the metadata should be kept as similar as possible
    internal static class MetadataKeys
    {
        public const string Symbol = "Symbol"; // ISymbol
        public const string Name = "Name"; // string, empty string if the symbol has no name (like the top-level namespace)
        public const string DisplayString = "DisplayString"; // string, a full string representation of the symbol
        public const string Kind = "Kind"; // string, the kind of symbol (Namespace, etc.)
        public const string DocumentationCommentXml = "DocumentationCommentXml"; // string, the XML documentation comments (if any) or an empty string
        public const string Documentation = "Documentation"; // string, the documentation specific to this symbol (as gathered from documentation comment XML on this or a parent) or an empty string

        // Namespace
        public const string ParentNamespace = "ParentNamespace"; // IDocument, absent if not nested
        public const string NestedNamespaces = "NestedNamespaces"; // IReadOnlyList<IDocument>, empty if none
        public const string Types = "Types"; // IReadOnlyList<IDocument>, includes all types including nested

        // Type
        public const string Namespace = "Namespace"; // IDocument
        public const string ParentType = "ParentType"; // IDocument, absent if not nested
        public const string NestedTypes = "NestedTypes"; // IReadOnlyList<IDocument>
        public const string Members = "Members"; // IReadOnlyList<IDocument>

        // Member
        public const string Type = "Type"; // IDocument
    }
}