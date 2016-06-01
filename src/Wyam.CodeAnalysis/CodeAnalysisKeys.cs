using System.Reflection.Metadata;

namespace Wyam.CodeAnalysis
{
    // Note that if we ever introduce code analysis for other formats (such as Java or CSS), the metadata should be kept as similar as possible
    public static class CodeAnalysisKeys
    {
        // All
        public const string IsResult = nameof(IsResult); // bool, true = part of the initial result set (I.e., only those that matched the predicate, if any)
        public const string SymbolId = nameof(SymbolId); // string, a unique ID that identifies this symbol
        public const string Symbol = nameof(Symbol); // ISymbol
        public const string Name = nameof(Name); // string, empty string if the symbol has no name (like the top-level namespace)
        public const string FullName = nameof(FullName); // string, namespaces = name of the namespace, types = with generic type parameters
        public const string QualifiedName = nameof(QualifiedName); // string, the qualified name including containing namespaces
        public const string DisplayName = nameof(DisplayName); // string, namespace = QualifiedName, type = FullName
        public const string Kind = nameof(Kind); // string, the general kind of symbol (Namespace, NamedType, etc.)
        public const string SpecificKind = nameof(SpecificKind); // string, the more specific kind of the symbol (Class, Struct, etc. - same as Kind if no more specific kind)
        public const string ContainingNamespace = nameof(ContainingNamespace); // IDocument, null if not nested

        // Namespace
        public const string MemberTypes = nameof(MemberTypes); // IReadOnlyList<IDocument>, only contains direct children, not all descendants
        public const string MemberNamespaces = nameof(MemberNamespaces); // IReadOnlyList<IDocument>, empty if none

        // Type
        public const string ContainingType = nameof(ContainingType); // IDocument, null if not nested
        public const string BaseType = nameof(BaseType); // IDocument
        public const string AllInterfaces = nameof(AllInterfaces); // IReadOnlyList<IDocument>
        public const string Members = nameof(Members); // IReadOnlyList<IDocument>
        public const string DerivedTypes = nameof(DerivedTypes); // IReadOnlyList<IDocument>
        public const string ImplementingTypes = nameof(ImplementingTypes); // IReadOnlyList<IDocument>
        public const string Constructors = nameof(Constructors); // IReadOnlyList<IDocument>
        public const string TypeParameters = nameof(TypeParameters); // IReadOnlyList<IDocument>
        public const string Accessibility = nameof(Accessibility); // string
        //                  MemberTypes

        // Method
        public const string Parameters = nameof(Parameters); // IReadOnlyList<IDocument>
        public const string ReturnType = nameof(ReturnType); // IDocument, null if returns void
        public const string OverriddenMethod = nameof(OverriddenMethod); // IDocument
        //                  TypeParameters
        //                  ContainingType
        //                  Accessibility

        // Field
        public const string Type = nameof(Type); // IDocument
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
        public const string DeclaringType = nameof(DeclaringType); // IDocument

        // Parameter
        //                  Type
        //                  ContainingType

        // Documentation (not present for external symbols)
        public const string CommentXml = nameof(CommentXml); // string, the XML documentation comments (if any) or an empty string
        public const string Example = nameof(Example); // string, multiple entries are concatenated
        public const string Remarks = nameof(Remarks); // string, multiple entries are concatenated
        public const string Summary = nameof(Summary); // string, multiple entries are concatenated
        public const string Returns = nameof(Returns); // string, multiple entries are concatenated
        public const string Value = nameof(Value); // string, multiple entries are concatenated
        public const string Exceptions = nameof(Exceptions); // IReadOnlyList<ReferenceComment>
        public const string Permissions = nameof(Permissions); // IReadOnlyList<ReferenceComment>
        public const string Params = nameof(Params); // IReadOnlyList<ReferenceComment>
        public const string TypeParams = nameof(TypeParams); // IReadOnlyList<ReferenceComment>
        public const string SeeAlso = nameof(SeeAlso); // IReadOnlyList<string>, list of <seealso> links (including those in child entities)
        public const string Syntax = nameof(Syntax); // string
        //                  [ElementName]Comments // IReadOnlyList<OtherComment>, list of extra documentation elements
    }
}