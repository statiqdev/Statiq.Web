using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Wyam.Common;

namespace Wyam.Modules.CodeAnalysis
{
    public class AnalyzeCSharp : IModule
    {
        private readonly List<KeyValuePair<string, ConfigHelper<object>>> _withMetadata
            = new List<KeyValuePair<string, ConfigHelper<object>>>();
         
        public IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            // Get syntax trees
            ConcurrentBag<SyntaxTree> syntaxTrees = new ConcurrentBag<SyntaxTree>();
            Parallel.ForEach(inputs, input =>
            {
                using (Stream stream = input.GetStream())
                {
                    SourceText sourceText = SourceText.From(stream);
                    syntaxTrees.Add(CSharpSyntaxTree.ParseText(sourceText));
                }
            });

            // Create the compilation
            MetadataReference mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            CSharpCompilation compilation = CSharpCompilation.Create("CodeAnalysisModule", syntaxTrees).WithReferences(mscorlib);

            // Get and return the document tree
            AnalyzeSymbolVisitor visitor = new AnalyzeSymbolVisitor(context, _withMetadata);
            visitor.Visit(compilation.Assembly.GlobalNamespace);
            return visitor.GetNamespaceOrTypeDocuments();
        }

        // These methods add metadata to the symbol documents as they're being constructed
        // They're important because if you add the metadata later as part of the pipeline,
        // it'll clone the symbol documents and document collections on the original symbol
        // documents like MemberTypes will still return the original documents without
        // the new metadata.
        public AnalyzeCSharp WithMeta(string key, object metadata)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _withMetadata.Add(new KeyValuePair<string, ConfigHelper<object>>(
                key, new ConfigHelper<object>(metadata)));
            return this;
        }

        public AnalyzeCSharp WithMeta(string key, ContextConfig metadata)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _withMetadata.Add(new KeyValuePair<string, ConfigHelper<object>>(
                key, new ConfigHelper<object>(metadata)));
            return this;
        }

        public AnalyzeCSharp WithMeta(string key, DocumentConfig metadata)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            _withMetadata.Add(new KeyValuePair<string, ConfigHelper<object>>(
                key, new ConfigHelper<object>(metadata)));
            return this;
        }
    }
}
