using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
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
using Wyam.Abstractions;

namespace Wyam.Core.Configuration
{
    // This just encapsulates configuration logic
    internal class Configurator
    {
        private readonly Engine _engine;
        private readonly PackagesCollection _packages;
        private readonly AssemblyCollection _assemblies = new AssemblyCollection();
        private readonly NamespacesCollection _namespaces = new NamespacesCollection();

        public Configurator(Engine engine)
        {
            _engine = engine;
            _packages = new PackagesCollection(engine);
        }

        // Setup is separated from config by a line with only '-' characters
        // If no such line exists, then the entire script is treated as config
        public void Configure(string script, bool installPackages)
        {
            // If no script, nothing else to do
            if (string.IsNullOrWhiteSpace(script))
            {
                return;
            }

            // Setup (install packages, specify additional assemblies and namespaces, etc.)
            Tuple<string, string> configParts = GetConfigParts(script);
            if (!string.IsNullOrWhiteSpace(configParts.Item1))
            {
                Setup(configParts.Item1, installPackages);
            }

            // Configuration
            Config(configParts.Item2);
        }

        // Item1 = setup (possibly null), Item2 = config
        public Tuple<string, string> GetConfigParts(string script)
        {
            string setup = null;
            List<string> configLines = script.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            int setupLine = configLines.FindIndex(x =>
            {
                string trimmed = x.TrimEnd();
                return trimmed.Length > 0 && trimmed.All(y => y == '-');
            });
            if (setupLine != -1)
            {
                setup = string.Join(Environment.NewLine, configLines.Take(setupLine));
                configLines.RemoveRange(0, setupLine + 1);
            }
            return new Tuple<string, string>(setup, string.Join(Environment.NewLine, configLines));
        }

        private void Setup(string script, bool installPackages)
        {
            _engine.Trace.Verbose("Evaluating setup script...");
            int indent = _engine.Trace.Indent();
            try
            {
                // Create the script options
                ScriptOptions scriptOptions = new ScriptOptions()
                    .AddNamespaces(
                        "System",
                        "Wyam.Core",
                        "Wyam.Core.Configuration",
                        "Wyam.Core.NuGet",
                        "Wyam.Abstractions")
                    .AddReferences(
                        Assembly.GetAssembly(typeof(object)),  // System
                        Assembly.GetAssembly(typeof(Engine)),  //Wyam.Core
                        Assembly.GetAssembly(typeof(IAssemblyCollection)));  // Wyam.Abstractions

                // Evaluate the script
                CSharpScript.Eval(script, scriptOptions, new SetupGlobals(_engine, _packages, _assemblies, _namespaces));
                _engine.Trace.IndentLevel = indent;
                _engine.Trace.Verbose("Evaluated setup script.");

                // Install packages
                if (installPackages)
                {
                    _engine.Trace.Verbose("Installing packages...");
                    indent = _engine.Trace.Indent();
                    _packages.InstallPackages();
                    _engine.Trace.IndentLevel = indent;
                    _engine.Trace.Verbose("Packages installed.");
                }
            }
            catch (CompilationErrorException compilationError)
            {
                _engine.Trace.IndentLevel = indent;
                _engine.Trace.Error("Error compiling setup: {0}", compilationError.ToString());
                throw;
            }
            catch (Exception ex)
            {
                _engine.Trace.IndentLevel = indent;
                _engine.Trace.Error("Unexpected error during setup: {0}", ex.Message);
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
            _engine.Trace.Verbose("Initializing scripting environment...");
            int indent = _engine.Trace.Indent();
            try
            {

                HashSet<Assembly> assemblies = new HashSet<Assembly>(new AssemblyEqualityComparer())
                {
                    Assembly.GetAssembly(typeof (object)), // System
                    Assembly.GetAssembly(typeof (System.Collections.Generic.List<>)), // System.Collections.Generic 
                    Assembly.GetAssembly(typeof (System.Linq.ImmutableArrayExtensions)), // System.Linq
                    Assembly.GetAssembly(typeof (System.Dynamic.DynamicObject)), // System.Core (needed for dynamic)
                    Assembly.GetAssembly(typeof (Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo)), // Microsoft.CSharp (needed for dynamic)
                    Assembly.GetAssembly(typeof (System.IO.Path)), // System.IO
                    Assembly.GetAssembly(typeof (System.Diagnostics.TraceSource)), // System.Diagnostics
                    Assembly.GetAssembly(typeof (Wyam.Core.Engine)), // Wyam.Core
                    Assembly.GetAssembly(typeof (Wyam.Abstractions.IModule)) // Wyam.Abstractions
                };

                HashSet<string> namespaces = new HashSet<string>()
                {
                    "System",
                    "System.Collections.Generic",
                    "System.Linq",
                    "System.IO",
                    "System.Diagnostics",
                    "Wyam.Core",
                    "Wyam.Core.Configuration",
                    "Wyam.Core.Modules",
                    "Wyam.Core.Helpers",
                    "Wyam.Abstractions"
                };
                namespaces.AddRange(_namespaces.Namespaces);

                // Add specified assemblies from packages, etc.
                GetAssemblies(assemblies);

                // Get modules
                HashSet<Type> moduleTypes = GetModules(assemblies, namespaces);

                _engine.Trace.IndentLevel = indent;
                _engine.Trace.Verbose("Initialized scripting environment.");
                _engine.Trace.Verbose("Evaluating configuration script...");
                indent = _engine.Trace.Indent();

                // Generate the script
                script = GenerateScript(script, moduleTypes, namespaces);

                // Evaluate the script
                ScriptOptions options = new ScriptOptions()
                    .WithReferences(assemblies)
                    .WithNamespaces(namespaces);
                CSharpScript.Eval(script, options, new ConfigGlobals(_engine.Metadata, _engine.Pipelines));

                _engine.Trace.IndentLevel = indent;
                _engine.Trace.Verbose("Evaluated configuration script.");
            }
            catch (CompilationErrorException compilationError)
            {
                _engine.Trace.IndentLevel = indent;
                _engine.Trace.Error("Error compiling configuration: {0}", compilationError.Message);
                throw;
            }
            catch (Exception ex)
            {
                _engine.Trace.IndentLevel = indent;
                _engine.Trace.Error("Unexpected error during configuration evaluation: {0}", ex.Message);
                throw;
            }
        }
        
        // Adds all specified assemblies and those in packages path, finds all modules, and adds their namespaces and all assembly references to the options
        private void GetAssemblies(HashSet<Assembly> assemblies)
        {
            // Get path to all assemblies (except those specified by name)
            List<string> assemblyPaths = new List<string>();
            _engine.Trace.Verbose("Scanning for assemblies and modules in NuGet packages.");
            assemblyPaths.AddRange(_packages.GetCompatibleAssemblyPaths());
            assemblyPaths.AddRange(_assemblies.Directories
                .Select(x => new Tuple<string, SearchOption>(Path.Combine(_engine.RootFolder, x.Item1), x.Item2))
                .Where(x => Directory.Exists(x.Item1))
                .Select(x =>
                {
                    _engine.Trace.Verbose("Scanning for assemblies and modules in path {0}.", x.Item1);
                    return x;
                })
                .SelectMany(x => Directory.GetFiles(x.Item1, "*.dll", x.Item2)));
            assemblyPaths.AddRange(_assemblies.ByFile
                .Select(x => Path.Combine(_engine.RootFolder, x))
                .Where(File.Exists));

            // Iterate assemblies by path (making sure to add them to the current path if relative), add them to the script, and check for modules
            foreach (string assemblyPath in assemblyPaths.Distinct())
            {
                try
                {
                    _engine.Trace.Verbose("Loading assembly from {0}.", assemblyPath);
                    Assembly assembly = Assembly.LoadFile(assemblyPath);
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
                _engine.Trace.Verbose("Loading modules from assembly {0}...", assembly.FullName);
                foreach (Type moduleType in GetLoadableTypes(assembly).Where(x => typeof(IModule).IsAssignableFrom(x)
                    && x.IsPublic && !x.IsAbstract && x.IsClass && !x.ContainsGenericParameters))
                {
                    _engine.Trace.Verbose("* Found module {0} in assembly {1}.", moduleType.Name, assembly.FullName);
                    moduleTypes.Add(moduleType);
                    namespaces.Add(moduleType.Namespace);
                }
                _engine.Trace.Verbose("Loaded modules from assembly {0}.", assembly.FullName);
            }
            return moduleTypes;
        }

        public IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (Exception loaderException in ex.LoaderExceptions)
                {
                    _engine.Trace.Verbose("Loader Exception: {0}", loaderException.Message);
                }
                return ex.Types.Where(t => t != null);
            }
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

            return scriptBuilder.ToString();
        }
    }
}
