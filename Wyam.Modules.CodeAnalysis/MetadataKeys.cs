namespace Wyam.Modules.CodeAnalysis
{
    // Note that if we ever introduce code analysis for other formats (such as Java or CSS), the metadata should be kept as similar as possible
    internal static class MetadataKeys
    {
        // All
        public const string WritePath = "WritePath"; // string, feeds WriteFiles and tells it where to place the output file
        public const string SymbolId = "SymbolId"; // string, a unique ID that identifies this symbol
        public const string Symbol = "Symbol"; // ISymbol
        public const string Name = "Name"; // string, empty string if the symbol has no name (like the top-level namespace)
        public const string FullName = "FullName"; // string, namespaces = all containing namespaces, types = with generic type parameters
        public const string QualifiedName = "QualifiedName"; // string, the qualified name including containing namespaces
        public const string DisplayName = "DisplayName"; // string, namespace = QualifiedName, type = FullName
        public const string Kind = "Kind"; // string, the general kind of symbol (Namespace, NamedType, etc.)
        public const string SpecificKind = "SpecificKind"; // string, the more specific kind of the symbol (Class, Struct, etc. - same as Kind if no more specific kind)
        public const string ContainingNamespace = "ContainingNamespace"; // IDocument, null if not nested

        // Namespace
        public const string MemberTypes = "MemberTypes"; // IReadOnlyList<IDocument>, only contains direct children, not all descendants
        public const string MemberNamespaces = "MemberNamespaces"; // IReadOnlyList<IDocument>, empty if none

        // Type
        public const string ContainingType = "ContainingType"; // IDocument, null if not nested
        public const string BaseType = "BaseType"; // IDocument
        public const string AllInterfaces = "AllInterfaces"; // IReadOnlyList<IDocument>
        public const string Members = "Members"; // IReadOnlyList<IDocument>
                                                 //                  MemberTypes

        // Method
        //                  ContainingType

        // Field
        //                  ContainingType

        // Event
        //                  ContainingType

        // Documentation Comments
        public const string DocumentationCommentXml = "DocumentationCommentXml"; // string, the XML documentation comments (if any) or an empty string
        public const string ExampleHtml = "ExampleHtml"; // IReadOnlyList<string>
        public const string RemarksHtml = "RemarksHtml"; // IReadOnlyList<string>
        public const string SummaryHtml = "SummaryHtml"; // IReadOnlyList<string>
        public const string ExceptionHtml = "ExceptionHtml"; // IReadOnlyList<KeyValuePair<string, string>>, key = link to exception (or name if not found), value = exception html
    }
}