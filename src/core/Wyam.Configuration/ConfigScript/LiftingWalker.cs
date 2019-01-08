using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Wyam.Configuration.ConfigScript
{
    internal class LiftingWalker : CSharpSyntaxWalker
    {
        private readonly StringBuilder _scriptCode = new StringBuilder();
        private readonly StringBuilder _usingDirectives = new StringBuilder();
        private readonly StringBuilder _typeDeclarations = new StringBuilder();
        private readonly StringBuilder _methodDeclarations = new StringBuilder();
        private readonly StringBuilder _extensionMethodDeclarations = new StringBuilder();

        private int _lineNumber = 1;
        private StringBuilder _lastBuilder = null;

        public string ScriptCode => _scriptCode.ToString();
        public string UsingDirectives => _usingDirectives.ToString();
        public string TypeDeclarations => _typeDeclarations.ToString();
        public string MethodDeclarations => _methodDeclarations.ToString();
        public string ExtensionMethodDeclarations => _extensionMethodDeclarations.ToString();

        // Standard script code
        public override void Visit(SyntaxNode node)
        {
            if (node is CompilationUnitSyntax)
            {
                // Top-level compilation unit
                base.Visit(node);
            }
            else if (node is UsingDirectiveSyntax)
            {
                // Using directive
                Add(node.ToFullString(), _usingDirectives, false);
            }
            else if (node is BaseTypeDeclarationSyntax)
            {
                // Type declaration
                Add(node.ToFullString(), _typeDeclarations);
            }
            else if (node is MethodDeclarationSyntax)
            {
                // Method (standard or extension)
                ParameterSyntax firstParameter = ((MethodDeclarationSyntax)node).ParameterList.Parameters.FirstOrDefault();
                if (firstParameter?.GetFirstToken().Kind() == SyntaxKind.ThisKeyword)
                {
                    Add(node.ToFullString(), _extensionMethodDeclarations);
                }
                else
                {
                    Add(node.ToFullString(), _methodDeclarations);
                }
            }
            else
            {
                // Everything else is standard script code
                Add(node?.ToFullString(), _scriptCode);
            }
        }

        public override void VisitTrivia(SyntaxTrivia trivia) => Add(trivia.ToFullString(), _scriptCode);

        public override void VisitToken(SyntaxToken token) => Add(token.ToFullString(), _scriptCode);

        private void Add(string code, StringBuilder builder, bool insertLineDirective = true)
        {
            if (code != null)
            {
                if (builder != _lastBuilder && insertLineDirective)
                {
                    builder.AppendLine("#line " + _lineNumber);
                }
                _lastBuilder = builder;
                _lineNumber += code.Count(x => x == '\n');
                builder.Append(code);
            }
        }
    }
}