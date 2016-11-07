using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Wyam.CodeAnalysis.Analysis
{
    internal static class SyntaxHelper
    {
        private const int MaximumLineLength = 100;
        private const string NewLinePrefix = "    ";

        private readonly static SymbolDisplayFormat _symbolDisplayFormat = new SymbolDisplayFormat(
            typeQualificationStyle:
                SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
            genericsOptions:
                SymbolDisplayGenericsOptions.IncludeTypeConstraints
                | SymbolDisplayGenericsOptions.IncludeVariance
                | SymbolDisplayGenericsOptions.IncludeTypeParameters,
            memberOptions:
                SymbolDisplayMemberOptions.IncludeAccessibility
                | SymbolDisplayMemberOptions.IncludeExplicitInterface
                | SymbolDisplayMemberOptions.IncludeModifiers
                | SymbolDisplayMemberOptions.IncludeParameters
                | SymbolDisplayMemberOptions.IncludeType,
            delegateStyle:
                SymbolDisplayDelegateStyle.NameAndSignature,
            extensionMethodStyle:
                SymbolDisplayExtensionMethodStyle.StaticMethod,
            parameterOptions:
                SymbolDisplayParameterOptions.IncludeDefaultValue
                | SymbolDisplayParameterOptions.IncludeExtensionThis
                | SymbolDisplayParameterOptions.IncludeName
                | SymbolDisplayParameterOptions.IncludeParamsRefOut
                | SymbolDisplayParameterOptions.IncludeType,
            propertyStyle:
                SymbolDisplayPropertyStyle.ShowReadWriteDescriptor,
            kindOptions:
                SymbolDisplayKindOptions.IncludeTypeKeyword
                | SymbolDisplayKindOptions.IncludeMemberKeyword
                | SymbolDisplayKindOptions.IncludeNamespaceKeyword,
            miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        private readonly static SymbolDisplayFormat _baseTypeDisplayFormat = new SymbolDisplayFormat(
        typeQualificationStyle:
            SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
        genericsOptions:
            SymbolDisplayGenericsOptions.IncludeVariance
            | SymbolDisplayGenericsOptions.IncludeTypeParameters,
        kindOptions:
            SymbolDisplayKindOptions.None,
        miscellaneousOptions:
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        public static string GetSyntax(ISymbol symbol)
        {
            if (symbol.Language == "C#")
            {
                return GetCSharpSyntax(symbol);
            }
            return string.Empty;
        }

        // C#

        private static readonly Dictionary<Accessibility, string> _cSharpAccessibilityStrings
            = new Dictionary<Accessibility, string>
            {
                {Accessibility.Internal, "internal "},
                {Accessibility.NotApplicable, string.Empty},
                {Accessibility.Private, "private "},
                {Accessibility.Protected, "protected "},
                {Accessibility.ProtectedAndInternal, string.Empty},
                {Accessibility.ProtectedOrInternal, "protected internal "},
                {Accessibility.Public, "public "}
            };

        private static string GetCSharpSyntax(ISymbol symbol)
        {
            WrappingStringBuilder builder = new WrappingStringBuilder(MaximumLineLength);

            // Attributes
            foreach (SyntaxNode attributeListNode in symbol.GetAttributes()
                .Select(x => x.ApplicationSyntaxReference.GetSyntax().Parent.NormalizeWhitespace()))
            {
                builder.AppendLine(attributeListNode.ReplaceTrivia(attributeListNode.DescendantTrivia(), (x, y) => new SyntaxTrivia()).NormalizeWhitespace().ToString());
            }
            builder.NewLinePrefix = NewLinePrefix;

            // Accessors, etc.
            INamedTypeSymbol namedTypeSymbol = symbol as INamedTypeSymbol;
            if (namedTypeSymbol != null)
            {
                builder.Append(_cSharpAccessibilityStrings[namedTypeSymbol.DeclaredAccessibility]);
                if (namedTypeSymbol.TypeKind == TypeKind.Class)
                {
                    if (namedTypeSymbol.IsStatic)
                    {
                        builder.Append("static ");
                    }
                    else
                    {
                        if (namedTypeSymbol.IsAbstract)
                        {
                            builder.Append("abstract ");
                        }
                        else if (namedTypeSymbol.IsSealed)
                        {
                            builder.Append("sealed ");
                        }
                    }
                }
            }

            // Symbol
            string symbolDisplayString = symbol.ToDisplayString(_symbolDisplayFormat);
            int constraintsLocation = symbolDisplayString.IndexOf(" where", StringComparison.Ordinal);
            string genericConstraints = constraintsLocation == -1 ? string.Empty : symbolDisplayString.Substring(constraintsLocation + 1);
            builder.Append(constraintsLocation == -1 ? symbolDisplayString : symbolDisplayString.Substring(0, constraintsLocation));

            // Insert base types and interfaces if a named type	
            if (namedTypeSymbol != null && namedTypeSymbol.TypeKind != TypeKind.Enum)
            {
                // Base type (exclude object base)
                bool baseType = false;
                if (namedTypeSymbol.BaseType != null && namedTypeSymbol.BaseType.Name != "Object")
                {
                    builder.Append(" : ");
                    builder.Append(namedTypeSymbol.BaseType.ToDisplayString(_baseTypeDisplayFormat), true);
                    baseType = true;
                }

                // Interfaces
                if (namedTypeSymbol.AllInterfaces.Length > 0)
                {
                    builder.Append(baseType ? ", " : " : ");
                }
                bool first = true;
                foreach (INamedTypeSymbol interfaceSymbol in namedTypeSymbol.AllInterfaces)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        builder.Append(", ");
                    }
                    builder.Append(interfaceSymbol.ToDisplayString(_baseTypeDisplayFormat), true);
                }
            }

            // Add generic constraints (wrap to new line if already wrapped another part of the signature)
            if (constraintsLocation != -1)
            {
                if (builder.ToString().Contains(Environment.NewLine + NewLinePrefix))
                {
                    builder.AppendLine();
                }
                else
                {
                    builder.Append(" ");
                }
                builder.Append(genericConstraints, true);
            }

            return builder.ToString();
        }
    }
}
