using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Wyam.Razor
{

    internal class WyamDocumentPhase : RazorEnginePhaseBase
    {
        private readonly string _baseType;
        private readonly NamespaceCollection _namespaces;

        public WyamDocumentPhase(string baseType, NamespaceCollection namespaces)
        {
            _baseType = baseType;
            _namespaces = namespaces;
        }

        protected override void ExecuteCore(RazorCodeDocument codeDocument)
        {
            DocumentIntermediateNode documentNode = codeDocument.GetDocumentIntermediateNode();

            NamespaceDeclarationIntermediateNode namespaceDeclaration =
                documentNode.Children.OfType<NamespaceDeclarationIntermediateNode>().Single();

            // Get the current model type, replacing default of dynamic with IDocument
            string modelType = ModelDirective.GetModelType(documentNode);
            modelType = modelType == "dynamic" ? "IDocument" : modelType;

            // Set the base page type and perform default model type substitution here
            ClassDeclarationIntermediateNode classDeclaration =
                namespaceDeclaration.Children.OfType<ClassDeclarationIntermediateNode>().Single();
            classDeclaration.BaseType = _baseType.Replace("<TModel>", "<" + modelType + ">");

            // Add namespaces
            int insertIndex = namespaceDeclaration.Children.IndexOf(
                namespaceDeclaration.Children.OfType<UsingDirectiveIntermediateNode>().First());
            foreach (string ns in _namespaces)
            {
                namespaceDeclaration.Children.Insert(
                    insertIndex,
                    new UsingDirectiveIntermediateNode()
                    {
                        Content = ns
                    });
            }

            codeDocument.SetDocumentIntermediateNode(documentNode);
        }
    }
}