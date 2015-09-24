namespace Wyam.Modules.CodeAnalysis
{
    // Note that if we ever introduce code analysis for other formats (such as Java or CSS), the metadata should be kept as similar as possible
    internal static class MetadataKeys
    {
        public const string Symbol = "Symbol"; // ISymbol
        public const string Name = "Name"; // string, empty string if the symbol has no name (like the top-level namespace)
        public const string DisplayString = "DisplayString"; // string, a full string representation of the symbol
        public const string Kind = "Kind"; // string, the kind of symbol (Namespace, etc.)
        public const string DocumentationCommentXml = "DocumentationCommentXml"; // string, the XML documentation comments (if any) or an empty string
        public const string Documentation = "Documentation"; // string, the documentation specific to this symbol (as gathered from documentation comment XML on this or a parent) or an empty string

        // Namespace
        public const string ContainingNamespace = "ContainingNamespace"; // IDocument, null if not nested
        public const string MemberTypes = "MemberTypes"; // IReadOnlyList<IDocument>, only contains direct children, not all descendants
        public const string MemberNamespaces = "MemberNamespaces"; // IReadOnlyList<IDocument>, empty if none

        // Type
        public const string ContainingType = "ContainingType"; // IDocument, null if not nested
        //                  ContainingNamespace
        //                  MemberTypes

        // Method
        //                  ContainingType
    }
}