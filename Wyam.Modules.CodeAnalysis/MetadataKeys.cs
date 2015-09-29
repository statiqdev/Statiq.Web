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

        public const string DocumentationCommentXml = "DocumentationCommentXml"; // string, the XML documentation comments (if any) or an empty string
        public const string ExampleHtml = "ExampleHtml"; // IReadOnlyList<KeyValuePair<string, IReadOnlyList<string>>>, list of <example> HTML, key = html, value = list of <seealso> links
        public const string RemarksHtml = "RemarksHtml"; // IReadOnlyList<KeyValuePair<string, IReadOnlyList<string>>>, list of <remarks> HTML, key = html, value = list of <seealso> links
        public const string SummaryHtml = "SummaryHtml"; // IReadOnlyList<KeyValuePair<string, IReadOnlyList<string>>>, list of <summary> HTML, key = html, value = list of <seealso> links
        public const string ExceptionHtml = "ExceptionHtml"; // IReadOnlyList<KeyValuePair<string, string>>, key = link to exception (or name if not found), value = exception html
        public const string ParamHtml = "ParamHtml"; // IReadOnlyList<KeyValuePair<string, string>>, key = name of param, value = param description
        public const string PermissionHtml = "PermissionHtml"; // IReadOnlyList<KeyValuePair<string, string>>, key = link to permission (or name if not found), value = permission html
        public const string ReturnsHtml = "ReturnsHtml"; // IReadOnlyList<KeyValuePair<string, IReadOnlyList<string>>>, list of <returns> HTML, key = html, value = list of <seealso> links
        public const string SeeAlsoHtml = "SeeAlsoHtml"; // IReadOnlyList<string>, list of top-level <seealso> links

        // Namespace
        public const string MemberTypes = "MemberTypes"; // IReadOnlyList<IDocument>, only contains direct children, not all descendants
        public const string MemberNamespaces = "MemberNamespaces"; // IReadOnlyList<IDocument>, empty if none

        // Type
        public const string ContainingType = "ContainingType"; // IDocument, null if not nested
        public const string BaseType = "BaseType"; // IDocument
        public const string AllInterfaces = "AllInterfaces"; // IReadOnlyList<IDocument>
        public const string Members = "Members"; // IReadOnlyList<IDocument>
        //                  MemberTypes

        // All Members
        //                  ContainingType

        // Method

        // Field

        // Event

        // Property
    }
}