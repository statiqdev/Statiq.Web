using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting;
using Wyam.Common.Tracing;
using Wyam.Core.Execution;

namespace Wyam.Configuration.ConfigScript
{
    internal class ScriptManager
    {
        private Script<object> _script;

        public string Code { get; private set; }

        public Assembly Assembly { get; private set; }

        public string AssemblyFullName { get; private set; }

        public MetadataReference MetadataReference { get; private set; }
        
        internal void Create(string code, IEnumerable<Type> moduleTypes, IEnumerable<string> namespaces, IList<Assembly> referenceAssemblies)
        {
            Code = Preprocess(code, moduleTypes, namespaces, referenceAssemblies);
            _script = CSharpScript.Create(Code, ScriptOptions.Default
                .WithReferences(referenceAssemblies.Select(x => MetadataReference.CreateFromFile(x.Location))),
                typeof(Globals));
        }

        // Internal for testing
        internal string Preprocess(string code, IEnumerable<Type> moduleTypes, IEnumerable<string> namespaces, IEnumerable<Assembly> referenceAssemblies)
        {
            StringBuilder codeBuilder = new StringBuilder();

            // Add the using statements
            codeBuilder.AppendLine("// Generated: bring all module namespaces in scope");
            codeBuilder.AppendLine(string.Join(Environment.NewLine, namespaces.Select(x => "using " + x + ";")));
            codeBuilder.AppendLine();

            // Add the actual user code
            codeBuilder.AppendLine("#line 1");
            codeBuilder.AppendLine(code);

            // Add methods to instantiate each module
            codeBuilder.AppendLine();
            codeBuilder.Append("// Generated: methods for module instantiation");  // A new line is prepended before each method
            Dictionary<string, string> moduleNames = new Dictionary<string, string>();
            HashSet<string> moduleTypeNames = new HashSet<string>();
            foreach (Type moduleType in moduleTypes)
            {
                codeBuilder.Append(GenerateModuleConstructorMethods(moduleType, moduleNames));
                moduleTypeNames.Add(moduleType.Name);
            }

            // Rewrite the lambda shorthand expressions
            SyntaxTree scriptTree = CSharpSyntaxTree.ParseText(codeBuilder.ToString(), new CSharpParseOptions(kind: SourceCodeKind.Script));
            LambdaRewriter lambdaRewriter = new LambdaRewriter(moduleTypeNames);
            return lambdaRewriter.Visit(scriptTree.GetRoot()).ToFullString();
        }

        // Internal for testing
        internal static string GenerateModuleConstructorMethods(Type moduleType, Dictionary<string, string> memberNames)
        {
            StringBuilder stringBuilder = new StringBuilder();
            CSharpCompilation compilation = CSharpCompilation
                .Create("ScriptCtorMethodGen")
                .AddReferences(MetadataReference.CreateFromFile(moduleType.Assembly.Location));
            foreach (AssemblyName referencedAssembly in moduleType.Assembly.GetReferencedAssemblies())
            {
                try
                {
                    compilation = compilation.AddReferences(
                        MetadataReference.CreateFromFile(Assembly.Load(referencedAssembly).Location));
                }
                catch (Exception)
                {
                    // We don't care about problems loading referenced assemblies, just ignore them
                }
            }
            INamedTypeSymbol moduleSymbol = compilation.GetTypeByMetadataName(moduleType.FullName);
            bool foundInstanceConstructor = false;
            string moduleFullName = moduleSymbol.ToDisplayString(new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters));
            string moduleName = moduleSymbol.ToDisplayString(new SymbolDisplayFormat(
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters));

            // Check to make sure we haven't already added a module with the same name
            string existingMemberName;
            if (memberNames.TryGetValue(moduleName, out existingMemberName))
            {
                throw new Exception($"Could not add module {moduleFullName} because it was already defined in {existingMemberName}.");
            }
            memberNames.Add(moduleName, moduleFullName);

            foreach (IMethodSymbol ctorSymbol in moduleSymbol.InstanceConstructors
                .Where(x => x.DeclaredAccessibility == Accessibility.Public))
            {
                foundInstanceConstructor = true;
                string ctorDisplayString = ctorSymbol.ToDisplayString(new SymbolDisplayFormat(
                    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                    genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                    parameterOptions: SymbolDisplayParameterOptions.IncludeName
                                      | SymbolDisplayParameterOptions.IncludeDefaultValue
                                      | SymbolDisplayParameterOptions.IncludeParamsRefOut
                                      | SymbolDisplayParameterOptions.IncludeType,
                    memberOptions: SymbolDisplayMemberOptions.IncludeParameters
                                   | SymbolDisplayMemberOptions.IncludeContainingType,
                    miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes));
                string ctorCallDisplayString = ctorSymbol.ToDisplayString(new SymbolDisplayFormat(
                    parameterOptions: SymbolDisplayParameterOptions.IncludeName
                                      | SymbolDisplayParameterOptions.IncludeParamsRefOut,
                    memberOptions: SymbolDisplayMemberOptions.IncludeParameters));
                stringBuilder.AppendFormat(@"
{0} {1}{2}
{{
    return new {0}{3};  
}}",
                    moduleFullName,
                    moduleName,
                    ctorDisplayString.Substring(ctorDisplayString.IndexOf("(", StringComparison.Ordinal)),
                    ctorCallDisplayString.Substring(ctorCallDisplayString.IndexOf("(", StringComparison.Ordinal)));
            }

            // Add a default constructor if we need to
            if (!foundInstanceConstructor)
            {
                stringBuilder.AppendFormat(@"
{0} {1}()
{{
    return new {0}();  
}}",
                    moduleType.FullName,
                    moduleType.Name);
            }

            return stringBuilder.ToString();
        }

        public void Compile()
        {
            // Get the compilation
            CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            Compilation compilation = _script.GetCompilation().WithOptions(compilationOptions);

            // Emit the assembly
            byte[] rawAssembly;
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                // Trace warnings
                List<string> warningMessages = result.Diagnostics
                    .Where(x => x.Severity == DiagnosticSeverity.Warning)
                    .Select(GetCompilationErrorMessage)
                    .ToList();
                if (warningMessages.Count > 0)
                {
                    Trace.Warning("{0} warnings compiling configuration:{1}{2}", warningMessages.Count,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, warningMessages));
                }

                // Trace errors
                List<string> errorMessages = result.Diagnostics
                    .Where(x => x.Severity == DiagnosticSeverity.Error)
                    .Select(GetCompilationErrorMessage)
                    .ToList();
                if (errorMessages.Count > 0)
                {
                    Trace.Error("{0} errors compiling configuration:{1}{2}", errorMessages.Count,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, errorMessages));
                }

                // Throw for errors or not success
                if (!result.Success || errorMessages.Count > 0)
                {
                    throw new ScriptCompilationException(errorMessages);
                }

                ms.Seek(0, SeekOrigin.Begin);
                rawAssembly = ms.ToArray();
            }
            Assembly = Assembly.Load(rawAssembly);
            AssemblyFullName = Assembly.FullName;
            MetadataReference = compilation.ToMetadataReference();
        }

        private static string GetCompilationErrorMessage(Diagnostic diagnostic)
        {
            string line = diagnostic.Location.IsInSource ? "Line " + (diagnostic.Location.GetMappedLineSpan().Span.Start.Line + 1) : "Metadata";
            return $"{line}: {diagnostic.Id}: {diagnostic.GetMessage()}";
        }

        public void Evaluate(Engine engine)
        {
            Globals globals = new Globals(engine);
            _script.RunAsync(globals).Wait();
        }
    }
}