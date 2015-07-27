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
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using NuGet;
using Wyam.Core.NuGet;
using Wyam.Abstractions;

namespace Wyam.Core.Configuration
{
    // This just encapsulates configuration logic
    internal class Configurator : IDisposable
    {
        private readonly Engine _engine;
        private readonly PackagesCollection _packages;
        private readonly AssemblyCollection _assemblyCollection = new AssemblyCollection();
        private readonly Dictionary<string, Assembly> _assemblies = new Dictionary<string, Assembly>(); 
        private bool _disposed;
        private Assembly _setupAssembly;
        private string _configAssemblyFullName;
        private byte[] _rawSetupAssembly;

        public Configurator(Engine engine)
        {
            _engine = engine;
            _packages = new PackagesCollection(engine);

            // This is the default set of assemblies that should get loaded during configuration and in other dynamic modules
            AddAssembly(Assembly.GetAssembly(typeof (object))); // System
            AddAssembly(Assembly.GetAssembly(typeof (System.Collections.Generic.List<>))); // System.Collections.Generic 
            AddAssembly(Assembly.GetAssembly(typeof (System.Linq.ImmutableArrayExtensions))); // System.Linq
            AddAssembly(Assembly.GetAssembly(typeof (System.Dynamic.DynamicObject))); // System.Core (needed for dynamic)
            AddAssembly(Assembly.GetAssembly(typeof (Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo))); // Microsoft.CSharp (needed for dynamic)
            AddAssembly(Assembly.GetAssembly(typeof (System.IO.Stream))); // System.IO
            AddAssembly(Assembly.GetAssembly(typeof (System.Diagnostics.TraceSource))); // System.Diagnostics
            AddAssembly(Assembly.GetAssembly(typeof (Wyam.Core.Engine))); // Wyam.Core
            AddAssembly(Assembly.GetAssembly(typeof(Wyam.Abstractions.IModule))); // Wyam.Abstractions

            // Manually resolve included assemblies
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("Configurator");
            }
            _disposed = true;
            AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly;
        }

        public IEnumerable<Assembly> Assemblies
        {
            get { return _assemblies.Values; }
        }
        
        public byte[] RawConfigAssembly
        {
            get {  return _rawSetupAssembly; }
        }

        // Setup is separated from config by a line with only '-' characters
        // If no such line exists, then the entire script is treated as config
        public void Configure(string script, bool updatePackages)
        {
            // If no script, nothing else to do
            if (string.IsNullOrWhiteSpace(script))
            {
                return;
            }

            Tuple<string, string, string> configParts = GetConfigParts(script);

            // Setup (install packages, specify additional assemblies, etc.)
            if (!string.IsNullOrWhiteSpace(configParts.Item1))
            {
                Setup(configParts.Item1, updatePackages);
            }

            // Configuration
            Config(configParts.Item2, configParts.Item3);
        }

        // Item1 = setup (possibly null), Item2 = declarations (possibly null), Item2 = config
        public Tuple<string, string, string> GetConfigParts(string code)
        {
            List<string> configLines = code.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            // Get setup
            string setup = null;
            int setupLine = configLines.FindIndex(x =>
            {
                string trimmed = x.TrimEnd();
                return trimmed.Length > 0 && trimmed.All(y => y == '=');
            });
            if (setupLine != -1)
            {
                setup = string.Join(Environment.NewLine, configLines.Take(setupLine));
                configLines.RemoveRange(0, setupLine + 1);
            }

            // Get declarations
            string declarations = null;
            int declarationLine = configLines.FindIndex(x =>
            {
                string trimmed = x.TrimEnd();
                return trimmed.Length > 0 && trimmed.All(y => y == '-');
            });
            if (declarationLine != -1)
            {
                declarations = string.Join(Environment.NewLine, configLines.Take(declarationLine));
                configLines.RemoveRange(0, declarationLine + 1);
            }

            return new Tuple<string, string, string>(setup, declarations, string.Join(Environment.NewLine, configLines));
        }

        private void Setup(string code, bool updatePackages)
        {
            try
            {
                using (_engine.Trace.WithIndent().Verbose("Evaluating setup script"))
                {
                    // Create the setup script
                    code = @"
                        using System;
                        using Wyam.Core;
                        using Wyam.Core.Configuration;
                        using Wyam.Core.NuGet;
                        using Wyam.Abstractions;

                        public static class SetupScript
                        {
                            public static void Run(IPackagesCollection Packages, IAssemblyCollection Assemblies, string RootFolder, string InputFolder, string OutputFolder)
                            {
                                " + code + @"
                            }
                        }";

                    // Assemblies
                    Assembly[] setupAssemblies = new[]
                    {
                        Assembly.GetAssembly(typeof (object)), // System
                        Assembly.GetAssembly(typeof (Wyam.Core.Engine)), //Wyam.Core
                        Assembly.GetAssembly(typeof (Wyam.Abstractions.IModule)) // Wyam.Abstractions
                    };

                    // Load the dynamic assembly and invoke
                    _rawSetupAssembly = CompileScript("WyamSetup", setupAssemblies, code, _engine.Trace);
                    _setupAssembly = Assembly.Load(_rawSetupAssembly);
                    var configScriptType = _setupAssembly.GetExportedTypes().First(t => t.Name == "SetupScript");
                    MethodInfo runMethod = configScriptType.GetMethod("Run", BindingFlags.Public | BindingFlags.Static);
                    runMethod.Invoke(null, new object[] { _packages, _assemblyCollection, _engine.RootFolder, _engine.InputFolder, _engine.OutputFolder });
                }

                // Install packages
                using (_engine.Trace.WithIndent().Verbose("Installing packages"))
                {
                    _packages.InstallPackages(updatePackages);
                }
            }
            catch (Exception ex)
            {
                _engine.Trace.Error("Unexpected error during setup: {0}", ex.Message);
                throw;
            }
        }

        private void Config(string declarations, string code)
        {
            try
            {
                HashSet<string> namespaces;
                HashSet<Type> moduleTypes;
                using (_engine.Trace.WithIndent().Verbose("Initializing scripting environment"))
                {
                    namespaces = new HashSet<string>()
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

                    // Add specified assemblies from packages, etc.
                    GetAssemblies();

                    // Get modules
                    moduleTypes = GetModules(namespaces);
                }

                using (_engine.Trace.WithIndent().Verbose("Evaluating configuration script"))
                {

                    // Generate the script
                    code = GenerateScript(declarations, code, moduleTypes, namespaces);

                    // Load the dynamic assembly and invoke
                    _rawSetupAssembly = CompileScript("WyamConfig", _assemblies.Values, code, _engine.Trace);
                    _setupAssembly = Assembly.Load(_rawSetupAssembly);
                    _configAssemblyFullName = _setupAssembly.FullName;
                    var configScriptType = _setupAssembly.GetExportedTypes().First(t => t.Name == "ConfigScript");
                    MethodInfo runMethod = configScriptType.GetMethod("Run", BindingFlags.Public | BindingFlags.Static);
                    runMethod.Invoke(null, new object[] { _engine.Metadata, _engine.Pipelines, _engine.RootFolder, _engine.InputFolder, _engine.OutputFolder });
                }
            }
            catch (Exception ex)
            {
                _engine.Trace.Error("Unexpected error during configuration evaluation: {0}", ex.Message);
                throw;
            }
        }

        private static byte[] CompileScript(string assemblyName, IEnumerable<Assembly> assemblies, string code, ITrace trace)
        {
            // Create the compilation
            var parseOptions = new CSharpParseOptions();
            var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(code, Encoding.UTF8), parseOptions, assemblyName);
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create(assemblyName, new[] { syntaxTree },
                assemblies.Select(x => MetadataReference.CreateFromFile(x.Location)), compilationOptions)
                .AddReferences(
                // For some reason, Roslyn really wants these added by filename
                // See http://stackoverflow.com/questions/23907305/roslyn-has-no-reference-to-system-runtime
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll"))
            );

            // Emit the assembly
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);
                if (!result.Success)
                {
                    trace.Error("{0} errors compiling configuration:{1}{2}", result.Diagnostics.Length, Environment.NewLine,
                        string.Join(Environment.NewLine, result.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error)));
                    throw new AggregateException(result.Diagnostics.Select(x => new Exception(x.ToString())));
                }
                ms.Seek(0, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }
        
        // Adds all specified assemblies and those in packages path, finds all modules, and adds their namespaces and all assembly references to the options
        private void GetAssemblies()
        {
            // Get path to all assemblies (except those specified by name)
            List<string> assemblyPaths = new List<string>();
            assemblyPaths.AddRange(_packages.GetCompatibleAssemblyPaths());
            assemblyPaths.AddRange(Directory.GetFiles(Path.GetDirectoryName(typeof(Configurator).Assembly.Location), "*.dll", SearchOption.AllDirectories));
            assemblyPaths.AddRange(_assemblyCollection.Directories
                .Select(x => new Tuple<string, SearchOption>(Path.Combine(_engine.RootFolder, x.Item1), x.Item2))
                .Where(x => Directory.Exists(x.Item1))
                .SelectMany(x => Directory.GetFiles(x.Item1, "*.dll", x.Item2)));
            assemblyPaths.AddRange(_assemblyCollection.ByFile
                .Select(x => new Tuple<string, string>(x, Path.Combine(_engine.RootFolder, x)))
                .Select(x => File.Exists(x.Item2) ? x.Item2 : x.Item1));

            List<AssemblyName> referencedAssemblies = new List<AssemblyName>();

            // Iterate assemblies by path (making sure to add them to the current path if relative), add them to the script, and check for modules
            foreach (string assemblyPath in assemblyPaths.Distinct())
            {
                try
                {
                    _engine.Trace.Verbose("Loading assembly file {0}", assemblyPath);
                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    if (!AddAssembly(assembly))
                    {
                        _engine.Trace.Verbose("Skipping assembly file {0} because it was already added", assemblyPath);
                    }
                    else
                    {
                        LoadReferencedAssemblies(assembly.GetReferencedAssemblies());
                    }
                }
                catch (Exception ex)
                {
                    _engine.Trace.Verbose("{0} exception while loading assembly file {1}: {2}", ex.GetType().Name, assemblyPath, ex.Message);
                }
            }

            // Also iterate assemblies specified by name
            foreach (string assemblyName in _assemblyCollection.ByName)
            {
                try
                {
                    _engine.Trace.Verbose("Loading assembly {0}", assemblyName);
                    Assembly assembly = Assembly.Load(assemblyName);
                    if (!AddAssembly(assembly))
                    {
                        _engine.Trace.Verbose("Skipping assembly {0} because it was already added", assemblyName);
                    }
                    else
                    {
                        LoadReferencedAssemblies(assembly.GetReferencedAssemblies());
                    }
                }
                catch (Exception ex)
                {
                    _engine.Trace.Verbose("{0} exception while loading assembly {1}: {2}", ex.GetType().Name, assemblyName, ex.Message);
                }
            }
        }

        private void LoadReferencedAssemblies(IEnumerable<AssemblyName> assemblyNames)
        {
            foreach (AssemblyName assemblyName in assemblyNames.Where(x => !_assemblies.ContainsKey(x.FullName)))
            {
                try
                {
                    _engine.Trace.Verbose("Loading referenced assembly {0}", assemblyName);
                    Assembly assembly = Assembly.Load(assemblyName);
                    AddAssembly(assembly);
                    LoadReferencedAssemblies(assembly.GetReferencedAssemblies());
                }
                catch (Exception ex)
                {
                    _engine.Trace.Verbose("{0} exception while loading referenced assembly {1}: {2}", ex.GetType().Name, assemblyName, ex.Message);
                }
                
            }
        }

        private bool AddAssembly(Assembly assembly)
        {
            if (_assemblies.ContainsKey(assembly.FullName))
            {
                return false;
            }
            _assemblies.Add(assembly.FullName, assembly);
            return true;
        }

        private Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            // Only start resolving after we've generated the config assembly
            if (_setupAssembly != null)
            {
                if (args.Name == _configAssemblyFullName)
                {
                    return _setupAssembly;
                }

                Assembly assembly;
                if (_assemblies.TryGetValue(args.Name, out assembly))
                {
                    return assembly;
                }
            }
            return null;
        }

        private HashSet<Type> GetModules(HashSet<string> namespaces)
        {
            HashSet<Type> moduleTypes = new HashSet<Type>();
            foreach (Assembly assembly in _assemblies.Values)
            {
                using (_engine.Trace.WithIndent().Verbose("Searching for modules in assembly {0}", assembly.FullName))
                {
                    foreach (Type moduleType in GetLoadableTypes(assembly).Where(x => typeof(IModule).IsAssignableFrom(x)
                        && x.IsPublic && !x.IsAbstract && x.IsClass && !x.ContainsGenericParameters))
                    {
                        _engine.Trace.Verbose("Found module {0} in assembly {1}", moduleType.Name, assembly.FullName);
                        moduleTypes.Add(moduleType);
                        namespaces.Add(moduleType.Namespace);
                    }
                }
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
        private string GenerateScript(string declarations, string script, HashSet<Type> moduleTypes, HashSet<string> namespaces)
        {
            // Need to replace all instances of module type method name shortcuts to make them fully-qualified
            SyntaxTree scriptTree = CSharpSyntaxTree.ParseText(script, new CSharpParseOptions(kind: SourceCodeKind.Regular));
            ModuleMethodQualifier moduleMethodQualifier = new ModuleMethodQualifier(moduleTypes);
            script = moduleMethodQualifier.Visit(scriptTree.GetRoot()).ToFullString();

            // Start the script, adding all requested namespaces
            StringBuilder scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine(string.Join(Environment.NewLine, namespaces.Select(x => "using " + x + ";")));
            if (declarations != null)
            {
                scriptBuilder.AppendLine(declarations);
            }
            scriptBuilder.Append(@"
                public static class ConfigScript
                {
                    public static void Run(IDictionary<string, object> Metadata, IPipelineCollection Pipelines, string RootFolder, string InputFolder, string OutputFolder)
                    {
                        " + script + @"
                    }");

            // Add static methods to construct each module
            // Use Roslyn to get a display string for each constructor
            foreach (Type moduleType in moduleTypes)
            {
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

            scriptBuilder.Append("}");
            return scriptBuilder.ToString();
        }
    }
}
