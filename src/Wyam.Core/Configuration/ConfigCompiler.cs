using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Wyam.Common.Tracing;

namespace Wyam.Core.Configuration
{
    internal static class ConfigCompiler
    {
        public static byte[] Compile(string assemblyName, IEnumerable<Assembly> referenceAssemblies, string code)
        {
            // Create the compilation
            var parseOptions = new CSharpParseOptions();
            var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(code, Encoding.UTF8), parseOptions, assemblyName);
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            var assemblyPath = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create(assemblyName, new[] { syntaxTree },
                referenceAssemblies.Select(x => MetadataReference.CreateFromFile(x.Location)), compilationOptions)
                .AddReferences(
                    // For some reason, Roslyn really wants these added by filename
                    // See http://stackoverflow.com/questions/23907305/roslyn-has-no-reference-to-system-runtime
                    MetadataReference.CreateFromFile(System.IO.Path.Combine(assemblyPath, "mscorlib.dll")),
                    MetadataReference.CreateFromFile(System.IO.Path.Combine(assemblyPath, "System.dll")),
                    MetadataReference.CreateFromFile(System.IO.Path.Combine(assemblyPath, "System.Core.dll")),
                    MetadataReference.CreateFromFile(System.IO.Path.Combine(assemblyPath, "System.Runtime.dll"))
            );

            // Emit the assembly
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);
                if (!result.Success)
                {
                    List<string> diagnosticMessages = result.Diagnostics
                        .Where(x => x.Severity == DiagnosticSeverity.Error)
                        .Select(GetCompilationErrorMessage)
                        .ToList();
                    Trace.Error("{0} errors compiling configuration:{1}{2}", result.Diagnostics.Length, Environment.NewLine,
                        string.Join(Environment.NewLine, diagnosticMessages));
                    throw new AggregateException(diagnosticMessages.Select(x => new Exception(x)));
                }
                ms.Seek(0, SeekOrigin.Begin);
                return ms.ToArray();
            }

        }

        private static string GetCompilationErrorMessage(Diagnostic diagnostic)
        {
            string line = diagnostic.Location.IsInSource ? "Line " + (diagnostic.Location.GetMappedLineSpan().Span.Start.Line + 1) : "Metadata";
            return $"{line}: {diagnostic.Id}: {diagnostic.GetMessage()}";
        }
    }
}
