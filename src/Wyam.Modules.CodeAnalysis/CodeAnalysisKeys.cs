using System.Reflection.Metadata;

namespace Wyam.Modules.CodeAnalysis
{
    // Note that if we ever introduce code analysis for other formats (such as Java or CSS), the metadata should be kept as similar as possible
    public static class CodeAnalysisKeys
    {
        // All
        public const string IsResult = "IsResult"; // bool, true = part of the initial result set (I.e., only those that matched the predicate, if any)
        public const string SymbolId = "SymbolId"; // string, a unique ID that identifies this symbol
        public const string Symbol = "Symbol"; // ISymbol
        public const string Name = "Name"; // string, empty string if the symbol has no name (like the top-level namespace)
        public const string FullName = "FullName"; // string, namespaces = name of the namespace, types = with generic type parameters
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
        public const string DerivedTypes = "DerivedTypes"; // IReadOnlyList<IDocument>
        public const string ImplementingTypes = "ImplementingTypes"; // IReadOnlyList<IDocument>
        public const string Constructors = "Constructors"; // IReadOnlyList<IDocument>
        public const string TypeParameters = "TypeParameters"; // IReadOnlyList<IDocument>
        public const string Accessibility = "Accessibility"; // string
        //                  MemberTypes

        // Method
        public const string Parameters = "Parameters"; // IReadOnlyList<IDocument>
        public const string ReturnType = "ReturnType"; // IDocument, null if returns void
        public const string OverriddenMethod = "OverriddenMethod"; // IDocument
        //                  TypeParameters
        //                  ContainingType
        //                  Accessibility

        // Field
        public const string Type = "Type"; // IDocument
        //                  ContainingType
        //                  Accessibility

        // Event
        //                  Type
        //                  ContainingType
        //                  Accessibility

        // Property
        //                  ContainingType
        //                  Type
        //                  Parameters (I.e., for indexers)
        //                  Accessibility

        // Type Parameter
        public const string DeclaringType = "DeclaringType"; // IDocument

        // Parameter
        //                  Type
        //                  ContainingType

        // Documentation (not present for external symbols)
        public const string CommentXml = "CommentXml"; // string, the XML documentation comments (if any) or an empty string
        public const string Example = "Example"; // string, multiple entries are concatenated
        public const string Remarks = "Remarks"; // string, multiple entries are concatenated
        public const string Summary = "Summary"; // string, multiple entries are concatenated
        public const string Returns = "Returns"; // string, multiple entries are concatenated
        public const string Value = "Value"; // string, multiple entries are concatenated
        public const string Exceptions = "Exceptions"; // IReadOnlyList<ReferenceComment>
        public const string Permissions = "Permissions"; // IReadOnlyList<ReferenceComment>
        public const string Params = "Params"; // IReadOnlyList<ReferenceComment>
        public const string TypeParams = "TypeParams"; // IReadOnlyList<ReferenceComment>
        public const string SeeAlso = "SeeAlso"; // IReadOnlyList<string>, list of <seealso> links (including those in child entities)
        public const string Syntax = "Syntax"; // string
        //                  [ElementName]Comments // IReadOnlyList<OtherComment>, list of extra documentation elements
    }
}