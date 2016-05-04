using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Wyam.Core;
using Wyam.Core.Configuration;
using Wyam.Core.Execution;

namespace Wyam.Configuration
{
    internal class ConfigCompilation
    {
        public const string AssemblyName = "WyamConfig";

        public string Code { get; private set; }

        public Assembly Assembly { get; private set; }

        public string AssemblyFullName { get; private set; }

        // Internal for testing
        internal void Generate(string declarations, string config, IEnumerable<Type> moduleTypes, IEnumerable<string> namespaces)
        {
            // Start the script, adding all requested namespaces
            StringBuilder scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine(string.Join(Environment.NewLine, namespaces.Select(x => "using " + x + ";")));
            if (!string.IsNullOrWhiteSpace(declarations))
            {
                scriptBuilder.AppendLine(declarations);
            }
            scriptBuilder.Append(@"
                public class ConfigScript : ConfigScriptBase
                {
                    public ConfigScript(Engine engine) : base(engine) { }

                    public override void Run()
                    {" + Environment.NewLine + config + @"
                    }");

            // Add static methods to construct each module
            Dictionary<string, string> moduleNames = new Dictionary<string, string>();
            HashSet<string> moduleTypeNames = new HashSet<string>();
            foreach (Type moduleType in moduleTypes)
            {
                scriptBuilder.Append(GenerateModuleConstructorMethods(moduleType, moduleNames));
                moduleTypeNames.Add(moduleType.Name);
            }

            scriptBuilder.Append("}");

            // Need to replace all instances of module type method name shortcuts to make them fully-qualified
            SyntaxTree scriptTree = CSharpSyntaxTree.ParseText(scriptBuilder.ToString());
            ConfigRewriter configRewriter = new ConfigRewriter(moduleTypeNames);
            Code = configRewriter.Visit(scriptTree.GetRoot()).ToFullString();
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
                        public static {0} {1}{2}
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
                        public static {0} {1}()
                        {{
                            return new {0}();  
                        }}",
                    moduleType.FullName,
                    moduleType.Name);
            }

            return stringBuilder.ToString();
        }

        public byte[] Compile(IEnumerable<Assembly> referenceAssemblies)
        {
            byte[] rawAssembly = ConfigCompiler.Compile("WyamConfig", referenceAssemblies, Code);
            Assembly = Assembly.Load(rawAssembly);
            AssemblyFullName = Assembly.FullName;
            return rawAssembly;
        }

        public void Invoke(Engine engine)
        {
            Type configScriptType = Assembly.GetExportedTypes().First(t => t.Name == "ConfigScript");
            ConfigScriptBase configScript = (ConfigScriptBase)Activator.CreateInstance(configScriptType, engine);
            configScript.Run();
        }
    }
}