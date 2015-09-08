using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Wyam.Core.Configuration
{
    // Adds qualifiers to module constructor methods
    // Also replaces @docXYZ and @ctxXYZ with lambda expressions
    internal class ConfigRewriter : CSharpSyntaxRewriter
    {
        private readonly HashSet<string> _moduleTypeNames;

        public ConfigRewriter(HashSet<Type> moduleTypes)
        {
            _moduleTypeNames = new HashSet<string>(moduleTypes.Select(x => x.Name));
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            // Replace the module method name with a qualified one if this is a module ctor
            bool nodeChanged = IsInvocationAModuleCtor(node);
            ExpressionSyntax nodeExpression = nodeChanged
                ? ((IdentifierNameSyntax)node.Expression).WithIdentifier(
                    SyntaxFactory.Identifier("ConfigScript." + ((IdentifierNameSyntax)node.Expression).Identifier.Text)
                        .WithTriviaFrom(((IdentifierNameSyntax)node.Expression).Identifier))
                : node.Expression;

            // Only do the replacement if this is a module ctor or a module fluent method
            bool argumentReplacement = nodeChanged
                || IsInvocationAModuleCtor(node
                    .DescendantNodes()
                    .TakeWhile(x => x is InvocationExpressionSyntax || x is MemberAccessExpressionSyntax)
                    .OfType<InvocationExpressionSyntax>()
                    .LastOrDefault());

            // Get a hash of all the previous @doc and @ctx lambda parameters and don't replace the same
            HashSet<string> currentScopeLambdaParameters = new HashSet<string>(
                node.Ancestors().OfType<LambdaExpressionSyntax>().SelectMany(
                    x => x.DescendantNodes().OfType<ParameterSyntax>()).Select(x => x.Identifier.Text));

            // Replace @doc and @ctx argument expressions with the appropriate Func<> expressions, and stop descending if we hit another module ctor
            ArgumentListSyntax argumentList = node.ArgumentList;
            foreach (ArgumentSyntax argument in node.ArgumentList.Arguments)
            {
                if (argumentReplacement && !(argument.Expression is LambdaExpressionSyntax))
                {
                    IdentifierNameSyntax docReplacementName = argument
                        .DescendantNodes(x => !(x is InvocationExpressionSyntax) || !IsInvocationAModuleCtor((InvocationExpressionSyntax)x))
                        .OfType<IdentifierNameSyntax>()
                        .FirstOrDefault(x => x != null && x.Identifier.Text.StartsWith("@doc") && !currentScopeLambdaParameters.Contains(x.Identifier.Text));
                    IdentifierNameSyntax ctxReplacementName = argument
                        .DescendantNodes(x => !(x is InvocationExpressionSyntax) || !IsInvocationAModuleCtor((InvocationExpressionSyntax)x))
                        .OfType<IdentifierNameSyntax>()
                        .FirstOrDefault(x => x != null && x.Identifier.Text.StartsWith("@ctx") && !currentScopeLambdaParameters.Contains(x.Identifier.Text));
                    if (docReplacementName != null)
                    {
                        argumentList = argumentList.ReplaceNode(argument, SyntaxFactory.Argument(
                            SyntaxFactory.ParenthesizedLambdaExpression(
                                SyntaxFactory.ParameterList(
                                    new SeparatedSyntaxList<ParameterSyntax>()
                                        .Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier(docReplacementName.Identifier.Text)))
                                        .Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier(ctxReplacementName == null ? "_" : ctxReplacementName.Identifier.Text)))),
                                argument.Expression))
                            .WithTriviaFrom(argument));
                        nodeChanged = true;
                    }
                    else if (ctxReplacementName != null)
                    {
                        argumentList = argumentList.ReplaceNode(argument, SyntaxFactory.Argument(
                            SyntaxFactory.SimpleLambdaExpression(
                                SyntaxFactory.Parameter(SyntaxFactory.Identifier(ctxReplacementName.Identifier.Text)),
                                argument.Expression))
                            .WithTriviaFrom(argument));
                        nodeChanged = true;
                    }
                }
            }

            // Build and return the result node (or just return the original node)
            return base.VisitInvocationExpression(nodeChanged ? node.WithExpression(nodeExpression).WithArgumentList(argumentList) : node);
        }

        private bool IsInvocationAModuleCtor(InvocationExpressionSyntax invocation)
        {
            if (invocation == null)
            {
                return false;
            }
            IdentifierNameSyntax name = invocation.Expression as IdentifierNameSyntax;
            return name != null && _moduleTypeNames.Contains(name.Identifier.Text);
        }
    }
}
