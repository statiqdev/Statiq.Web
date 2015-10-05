using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Wyam.Modules.CodeAnalysis
{
    internal static class SyntaxHelper
    {
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
            StringBuilder builder = new StringBuilder();

            // Attributes
            foreach (SyntaxNode attributeListNode in symbol.GetAttributes()
                .Select(x => x.ApplicationSyntaxReference.GetSyntax().Parent.NormalizeWhitespace()))
            {
                builder.AppendLine(attributeListNode.ReplaceTrivia(attributeListNode.DescendantTrivia(), (x, y) => new SyntaxTrivia()).NormalizeWhitespace().ToString());
            }

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
            builder.Append(symbol.ToDisplayString(_symbolDisplayFormat));

            // Insert base types and interfaces if a named type	
            StringBuilder baseBuilder = new StringBuilder();
            if (namedTypeSymbol != null && namedTypeSymbol.TypeKind != TypeKind.Enum)
            {
                // Base type (checking the base's base excludes object)
                int baseLength = 0;
                if (namedTypeSymbol.BaseType != null && namedTypeSymbol.BaseType.BaseType != null)
                {
                    string baseDisplayString = namedTypeSymbol.BaseType.ToDisplayString(_baseTypeDisplayFormat);
                    baseLength = baseDisplayString.Length;
                    baseBuilder.Append(" :" + Environment.NewLine + "    ");
                    baseBuilder.Append(baseDisplayString);
                }

                // Interfaces (wrap around if they get too long)
                if (namedTypeSymbol.AllInterfaces.Length > 0)
                {
                    baseBuilder.Append(baseBuilder.Length == 0 ? " :" + Environment.NewLine + "    " : ",");
                }
                int lineCount = -1;
                foreach (INamedTypeSymbol interfaceSymbol in namedTypeSymbol.AllInterfaces)
                {
                    if (lineCount > -1)
                    {
                        baseBuilder.Append(",");
                    }
                    else
                    {
                        lineCount = baseLength;
                    }
                    string interfaceDisplayString = interfaceSymbol.ToDisplayString(_baseTypeDisplayFormat);
                    lineCount += interfaceDisplayString.Length;
                    if (lineCount > 100)
                    {
                        baseBuilder.Append(Environment.NewLine + "    " + interfaceDisplayString);
                        lineCount = interfaceDisplayString.Length;
                    }
                    else
                    {
                        baseBuilder.Append(" " + interfaceDisplayString);
                    }
                }
            }

            // Insert any base classes and move generic constraints to the next line
            int insert = builder.ToString().IndexOf(" where", StringComparison.Ordinal);
            if (insert == -1)
            {
                builder.Append(baseBuilder);
            }
            else
            {
                builder.Insert(insert, baseBuilder + Environment.NewLine + "   ");
            }

            return builder.ToString();
        }
    }
}
