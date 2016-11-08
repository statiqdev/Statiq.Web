using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Wyam.CodeAnalysis.Analysis;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using System.IO;
using Wyam.Common.Tracing;

namespace Wyam.CodeAnalysis
{
    /// <summary>
    /// Performs static code analysis on assemblies, outputting a new document for each symbol.
    /// </summary>
    /// <remarks>
    /// This module acts as the basis for code analysis scenarios such as generating source code documentation.
    /// All specified assemblies are used to create a Roslyn compilation. The input documents are ignored.
    /// All symbols (namespaces, types, members, etc.) in the compilation are then recursively 
    /// processed and output from this module as documents, one per symbol. If an XML documentation output file
    /// can be located alongside the assembly, it is used to get documentation for the symbols. 
    /// The output documents have empty content
    /// and all information about the symbol is contained in the metadata. This lets you pass the output documents
    /// for each symbol on to a template engine like Razor and generate pages for each symbol by having the
    /// template use the document metadata.
    /// </remarks>
    /// <include file="Documentation.xml" path="/Documentation/AnalyzeCSharp/*" />
    /// <category>Metadata</category>
    public class AnalyzeAssemblies : CodeAnalysisModule<AnalyzeAssemblies>
    {
        private readonly List<string> _assemblyGlobs = new List<string>();

        /// <summary>
        /// Analyzes the specified assemblies.
        /// </summary>
        /// <param name="assemblies">A globbing pattern indicating the assemblies to analyze.</param>
        public AnalyzeAssemblies(string assemblies)
        {
            WithAssemblies(assemblies);
        }

        /// <summary>
        /// Analyzes the specified assemblies.
        /// </summary>
        /// <param name="assemblies">Globbing patterns indicating the assemblies to analyze.</param>
        public AnalyzeAssemblies(IEnumerable<string> assemblies)
        {
            WithAssemblies(assemblies);
        }

        /// <summary>
        /// Analyzes the specified assemblies.
        /// </summary>
        /// <param name="assemblies">A globbing pattern indicating the assemblies to analyze.</param>
        public AnalyzeAssemblies WithAssemblies(string assemblies)
        {
            if (!string.IsNullOrEmpty(assemblies))
            {
                _assemblyGlobs.Add(assemblies);
            }
            return this;
        }

        /// <summary>
        /// Analyzes the specified assemblies.
        /// </summary>
        /// <param name="assemblies">Globbing patterns indicating the assemblies to analyze.</param>
        public AnalyzeAssemblies WithAssemblies(IEnumerable<string> assemblies)
        {
            if (assemblies != null)
            {
                _assemblyGlobs.AddRange(assemblies.Where(x => !string.IsNullOrEmpty(x)));
            }
            return this;
        }

        public override IEnumerable<IDocument> Execute(IReadOnlyList<IDocument> inputs, IExecutionContext context)
        {
            // Get and iterate all the candidate assemblies
            IEnumerable<IFile> assemblyFiles = context.FileSystem.GetInputFiles(_assemblyGlobs)
                .Where(x => (x.Path.Extension == ".dll" || x.Path.Extension == ".exe") && x.Exists);
            MetadataReference[] assemblyReferences = assemblyFiles.Select(assemblyFile =>
            {
                // Create the metadata reference for the compilation
                IFile xmlFile = context.FileSystem.GetFile(assemblyFile.Path.ChangeExtension("xml"));
                if (xmlFile.Exists)
                {
                    Trace.Verbose($"Creating metadata reference for assembly {assemblyFile.Path.FullPath} with XML documentation file");
                    using (Stream xmlStream = xmlFile.OpenRead())
                    {
                        using (MemoryStream xmlBytes = new MemoryStream())
                        {
                            xmlStream.CopyTo(xmlBytes);
                            return MetadataReference.CreateFromStream(assemblyFile.OpenRead(),
                                documentation: XmlDocumentationProvider.CreateFromBytes(xmlBytes.ToArray()));
                        }
                    }
                }
                Trace.Verbose($"Creating metadata reference for assembly {assemblyFile.Path.FullPath} without XML documentation file");
                return (MetadataReference)MetadataReference.CreateFromStream(assemblyFile.OpenRead());
            }).ToArray();

            // Create the compilation
            MetadataReference mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            Compilation compilation = CSharpCompilation
                .Create("CodeAnalysisModule")
                .WithReferences(mscorlib)
                .WithReferences(assemblyReferences)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            return Execute(compilation, assemblyReferences.Select(x => ((IAssemblySymbol)compilation.GetAssemblyOrModuleSymbol(x)).GlobalNamespace), context);
        }
    }
}