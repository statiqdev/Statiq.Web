using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

using Wyam.Common.Execution;
using Wyam.Common.Tracing;

namespace Wyam.Configuration.ConfigScript
{
    internal class ScriptManager : IScriptManager
    {
        public const string AssemblyName = "WyamConfig";
        public const string ScriptClassName = "Script";

        public string Code { get; private set; }

        public Assembly Assembly { get; private set; }

        public string AssemblyFullName { get; private set; }

        public byte[] RawAssembly { get; private set; }

        public void Create(string code, IReadOnlyCollection<Type> moduleTypes, IEnumerable<string> namespaces)
        {
            Code = Parse(code, moduleTypes, namespaces);
        }

        // Internal for testing
        internal static string Parse(string code, IReadOnlyCollection<Type> moduleTypes, IEnumerable<string> namespaces)
        {
            // Rewrite the lambda shorthand expressions
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(kind: SourceCodeKind.Script));
            LambdaRewriter lambdaRewriter = new LambdaRewriter(moduleTypes.Select(x => x.Name));
            SyntaxNode syntaxNode = lambdaRewriter.Visit(syntaxTree.GetRoot());

            // "Lift" class and method declarations
            LiftingWalker liftingWalker = new LiftingWalker();
            liftingWalker.Visit(syntaxNode);

            // Get the using statements
            string usingStatements = string.Join(Environment.NewLine, namespaces.Select(x => "using " + x + ";"));

            // Get the methods to instantiate each module
            Dictionary<string, string> moduleNames = new Dictionary<string, string>();
            string moduleMethods = string.Join(
                Environment.NewLine,
                moduleTypes.Select(x => GenerateModuleConstructorMethods(x, moduleNames)));

            // Return the fully parsed script
            return
                $@"// Generated: bring all module namespaces in scope
                {usingStatements}

                // Input: using directives
                {liftingWalker.UsingDirectives}

                public class {ScriptClassName} : ScriptBase
                {{
                    public {ScriptClassName}(Engine engine) : base(engine) {{ }}

                    public override void Run()
                    {{
                        // Input: script code
{liftingWalker.ScriptCode}
                    }}

                    // Input: lifted methods
{liftingWalker.MethodDeclarations}

                    // Generated: methods for module instantiation
                    {moduleMethods} 
                }}

                // Input: lifted object declarations
{liftingWalker.TypeDeclarations}

                public static class ScriptExtensionMethods
                {{
                    // Input: lifted extension methods
{liftingWalker.ExtensionMethodDeclarations}
                }}";
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
                stringBuilder.AppendFormat(
                    @"
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
                stringBuilder.AppendFormat(
                    @"
                    {0} {1}()
                    {{
                        return new {0}();  
                    }}",
                    moduleType.FullName,
                    moduleType.Name);
            }

            return stringBuilder.ToString();
        }

        public void Compile(IReadOnlyCollection<Assembly> referenceAssemblies)
        {
            // Get the compilation
            CSharpParseOptions parseOptions = new CSharpParseOptions();
            SourceText sourceText = SourceText.From(Code, Encoding.UTF8);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceText, parseOptions, AssemblyName);
            CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).
                WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic>
                {
                    // ensure that specific warnings about assembly references are always suppressed
                    // https://github.com/dotnet/roslyn/issues/5501
                    { "CS1701", ReportDiagnostic.Suppress },
                    { "CS1702", ReportDiagnostic.Suppress },
                    { "CS1705", ReportDiagnostic.Suppress }
                });

            // For some reason, Roslyn really wants these added by filename
            // See http://stackoverflow.com/questions/23907305/roslyn-has-no-reference-to-system-runtime
            string assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            CSharpCompilation compilation = CSharpCompilation.Create(
                AssemblyName,
                new[] { syntaxTree },
                referenceAssemblies
                    .Where(x => !x.IsDynamic && !string.IsNullOrEmpty(x.Location))
                    .Select(x => MetadataReference.CreateFromFile(x.Location)), compilationOptions)
                    .AddReferences(
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));

            // Emit the assembly
            using (MemoryStream ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                // Trace warnings
                List<string> warningMessages = result.Diagnostics
                    .Where(x => x.Severity == DiagnosticSeverity.Warning)
                    .Select(GetCompilationErrorMessage)
                    .ToList();
                if (warningMessages.Count > 0)
                {
                    Trace.Warning(
                        "{0} warnings compiling configuration:{1}{2}",
                        warningMessages.Count,
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
                    Trace.Error(
                        "{0} errors compiling configuration:{1}{2}",
                        errorMessages.Count,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, errorMessages));
                }

                // Throw for errors or not success
                if (!result.Success || errorMessages.Count > 0)
                {
                    throw new ScriptCompilationException(errorMessages);
                }

                ms.Seek(0, SeekOrigin.Begin);
                byte[] rawAssembly = ms.ToArray();
                LoadCompiledConfig(rawAssembly);
            }
        }

        private static string GetCompilationErrorMessage(Diagnostic diagnostic)
        {
            string line = diagnostic.Location.IsInSource ? "Line " + (diagnostic.Location.GetMappedLineSpan().Span.Start.Line + 1) : "Metadata";
            return $"{line}: {diagnostic.Id}: {diagnostic.GetMessage()}";
        }

        public void LoadCompiledConfig(byte[] rawAssembly)
        {
            RawAssembly = rawAssembly;
            Assembly = Assembly.Load(rawAssembly);
            AssemblyFullName = Assembly.FullName;
        }

        public void Evaluate(IEngine engine)
        {
            Type scriptType = Assembly.GetExportedTypes().First(t => t.Name == ScriptClassName);
            ScriptBase script = (ScriptBase)Activator.CreateInstance(scriptType, engine);
            script.Run();
        }
    }
}