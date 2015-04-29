using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using NuGet;
using Wyam.Core.NuGet;
using Wyam.Extensibility;

namespace Wyam.Core.Configuration
{
    // This just encapsulates configuration logic
    internal class Configurator
    {
        private readonly Engine _engine;
        private readonly PackagesCollection _packages = new PackagesCollection();
        private readonly AssemblyCollection _assemblies = new AssemblyCollection();
        private readonly NamespacesCollection _namespaces = new NamespacesCollection();

        public Configurator(Engine engine)
        {
            _engine = engine;
        }

        // Preconfig is separated from config by a line with only '-' characters
        // If no such line exists, then the entire script is treated as config
        public void Configure(string script = null)
        {
            // Default metadata is configured regardless of if a config script has been provided
            // The script can overwrite or clear the default metadata if needed
            ConfigureDefaultMetadata();

            // If no script, nothing else to do
            if (string.IsNullOrWhiteSpace(script))
            {
                return;
            }

            Tuple<string, string> configParts = GetConfigParts(script);
            if (!string.IsNullOrWhiteSpace(configParts.Item1))
            {
                // Preconfigure (install packages, specify additional assemblies and namespaces, etc.)
                Preconfig(configParts.Item1);
                _packages.InstallPackages();
            }
            Config(configParts.Item2);
        }

        // Item1 = preconfig (possibly null), Item2 = config
        public Tuple<string, string> GetConfigParts(string script)
        {
            string preconfig = null;
            List<string> configLines = script.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            int preconfigLine = configLines.FindIndex(x =>
            {
                string trimmed = x.Trim();
                return trimmed.Length > 0 && x.Trim().All(y => y == '-');
            });
            if (preconfigLine != -1)
            {
                preconfig = string.Join(Environment.NewLine, configLines.Take(preconfigLine));
                configLines.RemoveRange(0, preconfigLine + 1);
            }
            return new Tuple<string, string>(preconfig, string.Join(Environment.NewLine, configLines));
        }

        private void Preconfig(string script)
        {
            try
            {
                // Create the script options
                ScriptOptions scriptOptions = new ScriptOptions()
                    .AddNamespaces(
                        "System",
                        "Wyam.Core",
                        "Wyam.Core.Configuration",
                        "Wyam.Core.NuGet")
                    .AddReferences(
                        Assembly.GetAssembly(typeof(object)),  // System
                        Assembly.GetAssembly(typeof(Engine)));  // Wyam.Core

                // Evaluate the script
                CSharpScript.Eval(script, scriptOptions, new PreConfigGlobals(_packages, _assemblies, _namespaces));
            }
            catch (CompilationErrorException compilationError)
            {
                _engine.Trace.Error("Error compiling pre-configuration: {0}", compilationError.ToString());
                throw;
            }
            catch (Exception ex)
            {
                _engine.Trace.Error("Unexpected error during pre-configuration: {0}", ex.ToString());
                throw;
            }
        }

        private class AssemblyEqualityComparer : IEqualityComparer<Assembly>
        {
            public bool Equals(Assembly x, Assembly y)
            {
                return String.CompareOrdinal(x.FullName, y.FullName) == 0;
            }

            public int GetHashCode(Assembly obj)
            {
                return obj.FullName.GetHashCode();
            }
        }

        private void Config(string script)
        {
            try
            { 
                HashSet<Assembly> assemblies = new HashSet<Assembly>(new AssemblyEqualityComparer())
                {
                    Assembly.GetAssembly(typeof(object)),  // System
                    Assembly.GetAssembly(typeof(List<>)),  // System.Collections.Generic 
                    Assembly.GetAssembly(typeof(ImmutableArrayExtensions)),  // System.Linq
                    Assembly.GetAssembly(typeof(System.Dynamic.DynamicObject)),  // System.Core (needed for dynamic)
                    Assembly.GetAssembly(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo)),  // Microsoft.CSharp (needed for dynamic)
                    Assembly.GetAssembly(typeof(Path)), // System.IO
                    Assembly.GetAssembly(typeof(Engine)), // Wyam.Core
                    Assembly.GetAssembly(typeof(IModule)) // Wyam.Extensibility
                };

                HashSet<string> namespaces = new HashSet<string>()
                {
                    "System",
                    "System.Collections.Generic",
                    "System.Linq",
                    "System.IO",
                    "Wyam.Core",
                    "Wyam.Core.Configuration",
                    "Wyam.Core.Modules",
                    "Wyam.Core.Helpers",
                    "Wyam.Extensibility"
                };
                namespaces.AddRange(_namespaces.Ns);

                // Add specified assemblies from packages, etc.
                GetAssemblies(assemblies);

                // Get modules
                HashSet<Type> moduleTypes = GetModules(assemblies, namespaces);

                // Generate the script
                script = GenerateScript(script, moduleTypes, namespaces);

                // Evaluate the script
                ScriptOptions options = new ScriptOptions()
                    .WithReferences(assemblies)
                    .WithNamespaces(namespaces);
                CSharpScript.Eval(script, options, new ConfigGlobals(_engine.Metadata, _engine.Pipelines));
            }
            catch (CompilationErrorException compilationError)
            {
                _engine.Trace.Error("Error compiling configuration: {0}", compilationError.ToString());
                throw;
            }
            catch (Exception ex)
            {
                _engine.Trace.Error("Unexpected error during configuration: {0}", ex.ToString());
                throw;
            }
        }
        
        // Adds all specified assemblies and those in packages path, finds all modules, and adds their namespaces and all assembly references to the options
        private void GetAssemblies(HashSet<Assembly> assemblies)
        {
            // Get path to all assemblies (except those specified by name)
            List<string> assemblyPaths = new List<string>();
            if (Directory.Exists(_packages.Path))
            {
                _engine.Trace.Verbose("Scanning for assemblies and modules in path {0}.", _packages.Path);
                assemblyPaths.AddRange(Directory.GetFiles(_packages.Path, "*.dll", SearchOption.AllDirectories));
            }
            assemblyPaths.AddRange(_assemblies.Directories
                .Select(x => new Tuple<string, SearchOption>(Path.Combine(Environment.CurrentDirectory, x.Item1), x.Item2))
                .Where(x => Directory.Exists(x.Item1))
                .Select(x =>
                {
                    _engine.Trace.Verbose("Scanning for assemblies and modules in path {0}.", x.Item1);
                    return x;
                })
                .SelectMany(x => Directory.GetFiles(x.Item1, "*.dll", x.Item2)));
            assemblyPaths.AddRange(_assemblies.ByPath
                .Select(x => Path.Combine(Environment.CurrentDirectory, x))
                .Where(File.Exists));

            // Iterate assemblies by path (making sure to add them to the current path if relative), add them to the script, and check for modules
            foreach (string assemblyPath in assemblyPaths.Distinct())
            {
                try
                {
                    _engine.Trace.Verbose("Loading assembly from {0}.", assemblyPath);
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    if (!assemblies.Add(assembly))
                    {
                        _engine.Trace.Verbose("Skipping assembly from {0} because it was already added.", assemblyPath);
                    }
                }
                catch (Exception ex)
                {
                    _engine.Trace.Verbose("{0} exception while loading assembly from {1}: {2}.", ex.GetType().Name, assemblyPath, ex.Message);
                }
            }

            // Also iterate assemblies specified by name
            foreach (string assemblyName in _assemblies.ByName)
            {
                try
                {
                    _engine.Trace.Verbose("Loading assembly {0}.", assemblyName);
                    Assembly assembly = Assembly.Load(assemblyName);
                    if (!assemblies.Add(assembly))
                    {
                        _engine.Trace.Verbose("Skipping assembly {0} because it was already added.", assemblyName);
                    }
                }
                catch (Exception ex)
                {
                    _engine.Trace.Verbose("{0} exception while loading assembly {1}: {2}.", ex.GetType().Name, assemblyName, ex.Message);
                }
            }
        }

        private HashSet<Type> GetModules(HashSet<Assembly> assemblies, HashSet<string> namespaces)
        {
            HashSet<Type> moduleTypes = new HashSet<Type>();
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type moduleType in assembly.GetTypes().Where(x => typeof(IModule).IsAssignableFrom(x)
                    && x.IsPublic && !x.IsAbstract && x.IsClass && !x.ContainsGenericParameters))
                {
                    _engine.Trace.Verbose("Found module {0} in assembly {1}.", moduleType.Name, assembly.FullName);
                    moduleTypes.Add(moduleType);
                    namespaces.Add(moduleType.Namespace);
                }
            }
            return moduleTypes;
        }

        private class ModuleMethodQualifier : CSharpSyntaxRewriter
        {
            private readonly HashSet<string> _moduleTypeName;

            public ModuleMethodQualifier(HashSet<Type> moduleTypes)
            {
                _moduleTypeName = new HashSet<string>(moduleTypes.Select(x => x.Name));    
            }

            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                IdentifierNameSyntax identifierName = node.Expression as IdentifierNameSyntax;
                if (identifierName != null && _moduleTypeName.Contains(identifierName.Identifier.Text))
                {
                    return
                        node.WithExpression(
                            identifierName.WithIdentifier(
                                SyntaxFactory.Identifier("ConfigScript." + identifierName.Identifier.Text)));
                }
                return base.VisitInvocationExpression(node);
            }
        }

        // This creates a wrapper class for the config script that contains static methods for constructing modules
        private string GenerateScript(string script, HashSet<Type> moduleTypes, HashSet<string> namespaces)
        {
            // Need to replace all instances of module type method name shortcuts to make them fully-qualified
            SyntaxTree scriptTree = CSharpSyntaxTree.ParseText(script, new CSharpParseOptions(kind: SourceCodeKind.Script));
            ModuleMethodQualifier moduleMethodQualifier = new ModuleMethodQualifier(moduleTypes);
            script = moduleMethodQualifier.Visit(scriptTree.GetRoot()).ToFullString();

            // Start the script, adding all requested namespaces
            StringBuilder scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine(string.Join(Environment.NewLine, namespaces.Select(x => "using " + x + ";")));
            scriptBuilder.Append(@"
                public static class ConfigScript
                {
                    public static void Run(IDictionary<string, object> Metadata, IPipelineCollection Pipelines)
                    {
                        " + script + @"
                    }");

            // Add static methods to construct each module
            // Use Roslyn to get a display string for each constructor
            foreach (Type moduleType in moduleTypes)
            {
                CSharpCompilation compilation = CSharpCompilation
                    .Create("ScriptCtorMethodGen")
                    .AddReferences(MetadataReference.CreateFromAssembly(moduleType.Assembly));
                foreach (AssemblyName referencedAssembly in moduleType.Assembly.GetReferencedAssemblies())
                {
                    try
                    {
                        compilation = compilation.AddReferences(
                            MetadataReference.CreateFromAssembly(Assembly.Load(referencedAssembly)));
                    }
                    catch (Exception)
                    {
                        // We don't care about problems loading referenced assemblies, just ignore them
                    }
                }
                INamedTypeSymbol moduleSymbol = compilation.GetTypeByMetadataName(moduleType.FullName);
                bool foundInstanceConstructor = false;
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
                    scriptBuilder.AppendFormat(@"
                        public static {0} {1}{2}
                        {{
                            return new {0}{3};  
                        }}",
                        moduleType.FullName,
                        moduleType.Name,
                        ctorDisplayString.Substring(ctorDisplayString.IndexOf("(", StringComparison.Ordinal)),
                        ctorCallDisplayString.Substring(ctorCallDisplayString.IndexOf("(", StringComparison.Ordinal)));
                }

                // Add a default constructor if we need to
                if (!foundInstanceConstructor)
                {
                    scriptBuilder.AppendFormat(@"
                        public static {0} {1}()
                        {{
                            return new {0}();  
                        }}",
                        moduleType.FullName,
                        moduleType.Name);
                }
            }

            scriptBuilder.Append(@"
                }

                ConfigScript.Run(Metadata, Pipelines);");

            // TODO: Debug the tracing logic - it fails with this kind of string (maybe because of the {?)
            //_engine.Trace.Verbose(scriptBuilder.ToString().Replace("{", "*"));

            return scriptBuilder.ToString();
        }

        private void ConfigureDefaultMetadata()
        {
            _engine.Metadata["InputPath"] = @".\input";
            _engine.Metadata["OutputPath"] = @".\output";
        }

        public void ConfigureDefaultPipelines()
        {
            // TODO: Call this from the console project if no script is specified - if using the engine directly, default pipelines must be explicitly configured
        }
    }
}
