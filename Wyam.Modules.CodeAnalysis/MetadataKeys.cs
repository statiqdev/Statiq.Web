namespace Wyam.Modules.CodeAnalysis
{
    // Note that if we ever introduce code analysis for other formats (such as Java or CSS), the metadata should be kept as similar as possible
    internal static class MetadataKeys
    {
        // All
        public const string WritePath = "WritePath"; // string, feeds WriteFiles and tells it where to place the output file (or missing for external symbols)
        public const string SymbolId = "SymbolId"; // string, a unique ID that identifies this symbol
        public const string Symbol = "Symbol"; // ISymbol
        public const string Name = "Name"; // string, empty string if the symbol has no name (like the top-level namespace)
        public const string FullName = "FullName"; // string, namespaces = name of the namespace, types = with generic type parameters
        public const string QualifiedName = "QualifiedName"; // string, the qualified name including containing namespaces
        public const string DisplayName = "DisplayName"; // string, namespace = QualifiedName, type = FullName
        public const string Kind = "Kind"; // string, the general kind of symbol (Namespace, NamedType, etc.)
        public const string SpecificKind = "SpecificKind"; // string, the more specific kind of the symbol (Class, Struct, etc. - same as Kind if no more specific kind)
        public const string ContainingNamespace = "ContainingNamespace"; // IDocument, null if not nested

        // Documentation (not present for external symbols)
        public const string DocumentationCommentXml = "DocumentationCommentXml"; // string, the XML documentation comments (if any) or an empty string
        public const string ExampleHtml = "ExampleHtml"; // string, multiple entries are concatenated
        public const string RemarksHtml = "RemarksHtml"; // string, multiple entries are concatenated
        public const string SummaryHtml = "SummaryHtml"; // string, multiple entries are concatenated
        public const string ReturnsHtml = "ReturnsHtml"; // string, multiple entries are concatenated
        public const string ValueHtml = "ValueHtml"; // string, multiple entries are concatenated
        public const string ExceptionHtml = "ExceptionHtml"; // IReadOnlyList<KeyValuePair<string, string>>, key = link to exception (or name if not found), value = exception HTML
        public const string PermissionHtml = "PermissionHtml"; // IReadOnlyList<KeyValuePair<string, string>>, key = link to permission (or name if not found), value = permission HTML
        public const string ParamHtml = "ParamHtml"; // IReadOnlyList<KeyValuePair<string, string>>, key = name of param, value = param HTML
        public const string TypeParamHtml = "TypeParamHtml"; // IReadOnlyList<KeyValuePair<string, string>>, key = name of param, value = param HTML
        public const string SeeAlsoHtml = "SeeAlsoHtml"; // IReadOnlyList<string>, list of <seealso> links (including those in child entities)
        public const string Syntax = "Syntax"; // string
        //                  [ElementName]Html // IReadOnlyList<KeyValuePair<IReadOnlyDictionary<string, string>, string>>, list of extra documentation elements, key = dictionary of attributes (or empty), value = HTML

        // Namespace
        public const string MemberTypes = "MemberTypes"; // IReadOnlyList<IDocument>, only contains direct children, not all descendants
        public const string MemberNamespaces = "MemberNamespaces"; // IReadOnlyList<IDocument>, empty if none

        // Type
        public const string ContainingType = "ContainingType"; // IDocument, null if not nested
        public const string BaseType = "BaseType"; // IDocument
        public const string AllInterfaces = "AllInterfaces"; // IReadOnlyList<IDocument>
        public const string Members = "Members"; // IReadOnlyList<IDocument>
        public const string DerivedTypes = "DerivedTypes"; // IReadOnlyList<IDocument>
        public const string ImplementingTypes = "ImplementingTypes"; // IReadOnlyList<IDocument>
        public const string Constructors = "Constructors"; // IReadOnlyList<IDocument>
        public const string TypeParams = "TypeParams"; // IReadOnlyList<IDocument>
        //                  MemberTypes
        
        // Method
        public const string Parameters = "Parameters"; // IDocument
        public const string ReturnType = "ReturnType"; // IDocument, null if returns void
        public const string Overridden = "Overridden"; // IDocument
        //                  TypeParams
        //                  ContainingType

        // Field
        public const string Type = "Type"; // IDocument
        //                  ContainingType

        // Event
        //                  Type
        //                  ContainingType

        // Property
        //                  ContainingType
        //                  Type
        //                  Parameters (I.e., for indexers)

        // Type Parameter
        public const string DeclaringType = "DeclaringType"; // IDocument

        // Parameter
        //                  Type
        //                  ContainingType
    }
}