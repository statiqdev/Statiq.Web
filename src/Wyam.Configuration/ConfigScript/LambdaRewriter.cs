using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Wyam.Configuration.ConfigScript
{
    // Also replaces @docXYZ and @ctxXYZ with lambda expressions
    internal class LambdaRewriter : CSharpSyntaxRewriter
    {
        private readonly HashSet<string> _moduleTypeNames;

        public LambdaRewriter(IEnumerable<string> moduleTypeNames)
        {
            _moduleTypeNames = new HashSet<string>(moduleTypeNames);
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            // Get a hash of all the previous @doc and @ctx lambda parameters and don't replace the same
            HashSet<string> currentScopeLambdaParameters = new HashSet<string>(
                node.Ancestors().OfType<LambdaExpressionSyntax>().SelectMany(
                    x => x.DescendantNodes().OfType<ParameterSyntax>()).Select(x => x.Identifier.Text));

            // Only do the replacement if this is a module ctor or a module fluent method
            ArgumentListSyntax argumentList = node.ArgumentList;
            bool nodeChanged = false;
            if(IsInvocationAModuleCtorOrFluentMethod(node))
            {
                // Replace @doc and @ctx argument expressions with the appropriate lambda expressions, and stop descending if we hit another module ctor
                foreach (ArgumentSyntax argument in node.ArgumentList.Arguments)
                {
                    // Don't replace existing lambda expressions
                    if (!(argument.Expression is LambdaExpressionSyntax))
                    {
                        List<IdentifierNameSyntax> identifierNames = argument
                            .DescendantNodes(x => !(x is InvocationExpressionSyntax) || !IsInvocationAModuleCtorOrFluentMethod((InvocationExpressionSyntax)x))
                            .OfType<IdentifierNameSyntax>()
                            .Where(x => x != null && !currentScopeLambdaParameters.Contains(x.Identifier.Text))
                            .ToList();
                        IdentifierNameSyntax docReplacementName = identifierNames.FirstOrDefault(x => x.Identifier.Text.StartsWith("@doc"));
                        IdentifierNameSyntax ctxReplacementName = identifierNames.FirstOrDefault(x => x.Identifier.Text.StartsWith("@ctx"));
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
            }

            // Build and return the result node (or just return the original node)
            return base.VisitInvocationExpression(nodeChanged ? node.WithArgumentList(argumentList) : node);
        }

        private bool IsInvocationAModuleCtorOrFluentMethod(InvocationExpressionSyntax invocation) => 
            IsInvocationAModuleCtor(invocation) || IsInvocationAFluentMethod(invocation);

        private bool IsInvocationAModuleCtor(InvocationExpressionSyntax invocation)
        {
            if (invocation == null)
            {
                return false;
            }
            IdentifierNameSyntax name = invocation.Expression as IdentifierNameSyntax;
            return name != null && _moduleTypeNames.Contains(name.Identifier.Text);
        }

        private bool IsInvocationAFluentMethod(InvocationExpressionSyntax invocation) => 
            IsInvocationAModuleCtor(invocation?.DescendantNodes()
                .TakeWhile(x => x is InvocationExpressionSyntax || x is MemberAccessExpressionSyntax)
                .OfType<InvocationExpressionSyntax>()
                .LastOrDefault());
    }
}
