using System.Collections.Generic;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;
using Wyam.Common.Documents;

namespace Wyam.CodeAnalysis
{
    /// <summary>
    /// Common metadata keys for code analysis modules.
    /// </summary>
    public static class CodeAnalysisKeys
    {
        // Note that if we ever introduce code analysis for other formats (such as Java or CSS), the metadata should be kept as similar as possible

        /// <summary>
        /// The name of the assembly for each input document. Used to group input documents by assembly (if provided). If this is
        /// not provided for each input source document, then analysis that depends on both source files and assemblies may not
        /// correctly bind symbols across multiple inputs.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string AssemblyName = nameof(AssemblyName);

        /// <summary>
        /// By default only certain symbols are processed as part of the initial
        /// result set(such as those that match the specified predicate). If this value is <c>true</c>, then this
        /// symbol was part of the initial result set. If it is <c>false</c>, the symbol was lazily processed later
        /// while fetching related symbols and may not contain the full set of metadata.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string IsResult = nameof(IsResult);

        /// <summary>
        /// A unique ID that identifies the symbol (can be used for generating folder paths, for example).
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string SymbolId = nameof(SymbolId);

        /// <summary>
        /// The Roslyn <c>ISymbol</c> from which this document is derived. If the document represents a namespace, this metadata might contain more than one
        /// symbol since the namespaces documents consolidate same-named namespaces across input code and assemblies.
        /// </summary>
        /// <type><see cref="ISymbol"/> (or <see cref="IEnumerable{ISymbol}"/> if a namespace)</type>
        public const string Symbol = nameof(Symbol);

        /// <summary>
        /// The name of the symbol, or an empty string if the symbol has no name (like the global namespace).
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Name = nameof(Name);

        /// <summary>
        /// The full name of the symbol. For namespaces, this is the name of the namespace. For types, this includes all generic type parameters.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string FullName = nameof(FullName);

        /// <summary>
        /// The qualified name of the symbol which includes all containing namespaces.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string QualifiedName = nameof(QualifiedName);

        /// <summary>
        /// A display name for the symbol. For namespaces, this is the same as <see cref="QualifiedName"/>.
        /// For types, this is the same as <see cref="FullName"/>.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string DisplayName = nameof(DisplayName);

        /// <summary>
        /// This is the general kind of symbol. For example, the for a namespace this is "Namespace" and for a type this is "NamedType".
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Kind = nameof(Kind);

        /// <summary>
        /// The more specific kind of the symbol ("Class", "Struct", etc.) This is the same as <c>Kind</c> if there is no more specific kind.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string SpecificKind = nameof(SpecificKind);

        /// <summary>
        /// The document that represents the containing namespace (or null if this symbol is not nested).
        /// </summary>
        /// <type><see cref="IDocument"/></type>
        public const string ContainingNamespace = nameof(ContainingNamespace);

        /// <summary>
        /// The document that represents the containing assembly (or null if this symbol is not from an assembly).
        /// </summary>
        /// <type><see cref="IDocument"/></type>
        public const string ContainingAssembly = nameof(ContainingAssembly);

        /// <summary>
        /// Indicates if the symbol is static.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string IsStatic = nameof(IsStatic);

        /// <summary>
        /// Indicates if the symbol is abstract.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string IsAbstract = nameof(IsAbstract);

        /// <summary>
        /// Indicates if the symbol is virtual.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string IsVirtual = nameof(IsVirtual);

        /// <summary>
        /// Indicates if the symbol is an override.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string IsOverride = nameof(IsOverride);

        /// <summary>
        /// A unique ID that identifies the symbol for documentation purposes.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string CommentId = nameof(CommentId);

        /// <summary>
        /// This is available for namespace and type symbols and contains a collection of the documents that represent all member types.
        /// It only contains direct children (as opposed to all nested types).
        /// </summary>
        /// <type><c>IReadOnlyList&lt;IDocument&gt;</c></type>
        public const string MemberTypes = nameof(MemberTypes);

        /// <summary>
        /// This is available for namespace symbols and contains a collection of the documents that represent all member namespaces.
        /// The collection is empty if there are no member namespaces.
        /// </summary>
        /// <type><c>IReadOnlyList&lt;IDocument&gt;</c></type>
        public const string MemberNamespaces = nameof(MemberNamespaces);

        /// <summary>
        /// This is available for type, method, field, event, property, and parameter symbols and contains a document
        /// representing the containing type(or<c>null</c> if no containing type).
        /// </summary>
        /// <type><see cref="IDocument"/></type>
        public const string ContainingType = nameof(ContainingType);

        /// <summary>
        /// This is available for type symbols and contains a collection of the documents that represent all base types (inner-most first).
        /// </summary>
        /// <type><c>IReadOnlyList&lt;IDocument&gt;</c></type>
        public const string BaseTypes = nameof(BaseTypes);

        /// <summary>
        /// This is available for type symbols and contains a collection of the documents that represent all implemented interfaces. The collection
        /// is empty if the type doesn't implement any interfaces.
        /// </summary>
        /// <type><c>IReadOnlyList&lt;IDocument&gt;</c></type>
        public const string AllInterfaces = nameof(AllInterfaces);

        /// <summary>
        /// This is available for type symbols and contains a collection of the documents that represent all members of the type, including inherited ones. The collection
        /// is empty if the type doesn't have any members.
        /// </summary>
        /// <type><c>IReadOnlyList&lt;IDocument&gt;</c></type>
        public const string Members = nameof(Members);

        /// <summary>
        /// This is available for type symbols and contains a collection of the documents that represent all operators of the type, including inherited ones. The collection
        /// is empty if the type doesn't have any operators.
        /// </summary>
        /// <type><c>IReadOnlyList&lt;IDocument&gt;</c></type>
        public const string Operators = nameof(Operators);

        /// <summary>
        /// This is available for type symbols and contains a collection of the documents that represent all extension members applicable to the type.
        /// </summary>
        /// <type><c>IReadOnlyList&lt;IDocument&gt;</c></type>
        public const string ExtensionMethods = nameof(ExtensionMethods);

        /// <summary>
        /// This is available for type symbols and contains a collection of the documents that represent all types derived from the type. The collection
        /// is empty if the type doesn't have any derived types.
        /// </summary>
        /// <type><c>IReadOnlyList&lt;IDocument&gt;</c></type>
        public const string DerivedTypes = nameof(DerivedTypes);

        /// <summary>
        /// This is available for interface symbols and contains a collection of the documents that represent all types that implement the interface. The collection
        /// is empty if no other types implement the interface.
        /// </summary>
        /// <type><c>IReadOnlyList&lt;IDocument&gt;</c></type>
        public const string ImplementingTypes = nameof(ImplementingTypes);

        /// <summary>
        /// This is available for type symbols and contains a collection of the documents that represent all constructors of the type. The collection
        /// is empty if the type doesn't have any explicit constructors.
        /// </summary>
        /// <type><c>IReadOnlyList&lt;IDocument&gt;</c></type>
        public const string Constructors = nameof(Constructors);

        /// <summary>
        /// This is available for type and method symbols and contains a collection of the documents that represent all generic type parameters of the type or method. The collection
        /// is empty if the type or method doesn't have any generic type parameters.
        /// </summary>
        /// <type><c>IReadOnlyList&lt;IDocument&gt;</c></type>
        public const string TypeParameters = nameof(TypeParameters);

        /// <summary>
        /// This is available for type, method, field, event, and property symbols and contains the declared accessibility of the symbol.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Accessibility = nameof(Accessibility);

        /// <summary>
        /// This is available for type, method, field, event, property, parameter, and type parameter symbols and contains the type symbol documents for attributes applied to the symbol.
        /// </summary>
        /// <type><c>IReadOnlyList&lt;IDocument&gt;</c></type>
        public const string Attributes = nameof(Attributes);

        /// <summary>
        /// This is available for method and property (I.e., indexer) symbols and contains a collection of the documents that represent the parameters of the method or property.
        /// </summary>
        /// <type><c>IReadOnlyList&lt;IDocument&gt;</c></type>
        public const string Parameters = nameof(Parameters);

        /// <summary>
        /// This is available for method symbols and contains a document that represents the return type of the method (or <c>null</c> if the method returns <c>void</c>).
        /// </summary>
        /// <type><see cref="IDocument"/></type>
        public const string ReturnType = nameof(ReturnType);

        /// <summary>
        /// This is available for method symbols and contains a document that represents the method being overridden (or <c>null</c> if no method is overriden by this one).
        /// </summary>
        /// <type><see cref="IDocument"/></type>
        public const string OverriddenMethod = nameof(OverriddenMethod);

        /// <summary>
        /// This is available for field, event, property, and parameter symbols and contains the document that represents the type of the symbol.
        /// </summary>
        /// <type><see cref="IDocument"/></type>
        public const string Type = nameof(Type);

        /// <summary>
        /// This is available for field symbols and indicates whether a constant value is available for the field.
        /// </summary>
        /// <type><see cref="bool"/></type>
        public const string HasConstantValue = nameof(HasConstantValue);

        /// <summary>
        /// This is available for field symbols and contains the constant value (if one exists).
        /// </summary>
        /// <type><see cref="object"/></type>
        public const string ConstantValue = nameof(ConstantValue);

        /// <summary>
        /// This is available for type parameter symbols and contains a document that represents the declaring type of the type parameter.
        /// </summary>
        /// <type><see cref="IDocument"/></type>
        public const string DeclaringType = nameof(DeclaringType);

        /// <summary>
        /// This is available for attribute symbols and contains the Roslyn <see cref="Microsoft.CodeAnalysis.AttributeData"/> instance for the attribute.
        /// </summary>
        /// <type><see cref="Microsoft.CodeAnalysis.AttributeData"/></type>
        public const string AttributeData = nameof(AttributeData);

        // Documentation keys (not present for external symbols)

        /// <summary>
        /// This is available for documents in the initial result set (<see cref="IsResult"/> is <c>true</c>) and contains
        /// the full unprocessed XML documentation comments content for this symbol. In addition, special metadata keys
        /// may be added for custom comment elements with the name <c>[ElementName]Comments</c>. These special metadata
        /// keys contain a <see cref="OtherComment"/> instance with the rendered HTML content (and any attributes) of the
        /// custom XML documentation comments with the given <c>[ElementName]</c>.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string CommentXml = nameof(CommentXml);

        /// <summary>
        /// This is available for documents in the initial result set (<see cref="IsResult"/> is <c>true</c>) and contains
        /// the rendered HTML content from all<c>example</c> XML documentation comments for this symbol.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Example = nameof(Example);

        /// <summary>
        /// This is available for documents in the initial result set (<see cref="IsResult"/> is <c>true</c>) and contains
        /// the rendered HTML content from all<c>remarks</c> XML documentation comments for this symbol.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Remarks = nameof(Remarks);

        /// <summary>
        /// This is available for documents in the initial result set (<see cref="IsResult"/> is <c>true</c>) and contains
        /// the rendered HTML content from all<c>summary</c> XML documentation comments for this symbol.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Summary = nameof(Summary);

        /// <summary>
        /// This is available for documents in the initial result set (<see cref="IsResult"/> is <c>true</c>) and contains
        /// the rendered HTML content from all<c>returns</c> XML documentation comments for this symbol.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Returns = nameof(Returns);

        /// <summary>
        /// This is available for documents in the initial result set (<see cref="IsResult"/> is <c>true</c>) and contains
        /// the rendered HTML content from all<c>value</c> XML documentation comments for this symbol.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Value = nameof(Value);

        /// <summary>
        /// This is available for documents in the initial result set (<see cref="IsResult"/> is <c>true</c>) and contains
        /// a collection of all<c>exception</c> XML documentation comments for this symbol with their name, link, and/or rendered HTML content.
        /// </summary>
        /// <type><see cref="IReadOnlyList{ReferenceComment}"/></type>
        public const string Exceptions = nameof(Exceptions);

        /// <summary>
        /// This is available for documents in the initial result set (<see cref="IsResult"/> is <c>true</c>) and contains
        /// a collection of all<c>permission</c> XML documentation comments for this symbol with their name, link, and/or rendered HTML content.
        /// </summary>
        /// <type><see cref="IReadOnlyList{ReferenceComment}"/></type>
        public const string Permissions = nameof(Permissions);

        /// <summary>
        /// This is available for documents in the initial result set (<see cref="IsResult"/> is <c>true</c>) and contains
        /// a collection of all<c>param</c> XML documentation comments for this symbol with their name, link, and/or rendered HTML content.
        /// </summary>
        /// <type><see cref="IReadOnlyList{ReferenceComment}"/></type>
        public const string Params = nameof(Params);

        /// <summary>
        /// This is available for documents in the initial result set (<see cref="IsResult"/> is <c>true</c>) and contains
        /// a collection of all<c>typeparam</c> XML documentation comments for this symbol with their name, link, and/or rendered HTML content.
        /// </summary>
        /// <type><see cref="IReadOnlyList{ReferenceComment}"/></type>
        public const string TypeParams = nameof(TypeParams);

        /// <summary>
        /// This is available for documents in the initial result set (<see cref="IsResult"/> is <c>true</c>) and contains
        /// a collection of all<c>seealso</c> XML documentation comments for this symbol with their rendered HTML link (or just name if no link could be generated).
        /// </summary>
        /// <type><c>IReadOnlyList&lt;string&gt;</c></type>
        public const string SeeAlso = nameof(SeeAlso);

        /// <summary>
        /// This is available for documents in the initial result set (<see cref="IsResult"/> is <c>true</c>) and contains
        /// a generated syntax example for the symbol.
        /// </summary>
        /// <type><see cref="string"/></type>
        public const string Syntax = nameof(Syntax);
    }
}