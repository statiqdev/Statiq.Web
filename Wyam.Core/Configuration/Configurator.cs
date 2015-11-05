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
using Wyam.Common.Modules;
using Wyam.Common.Tracing;

namespace Wyam.Core.Configuration
{
    // This just encapsulates configuration logic
    internal class Configurator : IDisposable
    {
        private readonly Engine _engine;
        private readonly string _fileName;
        private readonly bool _outputScripts;
        private readonly PackagesCollection _packages;
        private readonly AssemblyCollection _assemblyCollection = new AssemblyCollection();
        private readonly Dictionary<string, Assembly> _assemblies = new Dictionary<string, Assembly>(); 
        private readonly HashSet<string> _namespaces = new HashSet<string>();
        private bool _disposed;
        private Assembly _setupAssembly;
        private string _configAssemblyFullName;
        private byte[] _rawSetupAssembly;


        /// <summary>
        /// Initializes a new instance of the <see cref="Configurator" /> class.
        /// </summary>
        /// <param name="engine">The engine.</param>
        /// <param name="fileName">Name of the input file, used for error reporting.</param>
        /// <param name="outputScripts">if set to <c>true</c> outputs .cs files for the generated scripts.</param>
        public Configurator(Engine engine, string fileName, bool outputScripts)
        {
            _engine = engine;
            _fileName = fileName;
            _outputScripts = outputScripts;
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
            AddAssembly(Assembly.GetAssembly(typeof(IModule))); // Wyam.Common

            // Manually resolve included assemblies
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly;
            _disposed = true;
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Configurator));
            }
        }

        public IEnumerable<Assembly> Assemblies => _assemblies.Values;

        public IEnumerable<string> Namespaces => _namespaces; 

        public byte[] RawConfigAssembly => _rawSetupAssembly;

        // Setup is separated from config by a line with only '-' characters
        // If no such line exists, then the entire script is treated as config
        public void Configure(string script, bool updatePackages)
        {
            CheckDisposed();

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
            CheckDisposed();

            List<string> configLines = code.Replace("\r", "").Split(new[] { '\n' }, StringSplitOptions.None).ToList();

            // Get setup
            int startLine = 1;
            string setup = null;
            int setupLine = configLines.FindIndex(x =>
            {
                string trimmed = x.TrimEnd();
                return trimmed.Length > 0 && trimmed.All(y => y == '=');
            });
            if (setupLine != -1)
            {
                List<string> setupLines = configLines.Take(setupLine).ToList();
                setup = $"#line {startLine}{Environment.NewLine}{string.Join(Environment.NewLine, setupLines)}";
                startLine = setupLines.Count + 2;
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
                List<string> declarationLines = configLines.Take(declarationLine).ToList();
                declarations = $"#line {startLine}{Environment.NewLine}{string.Join(Environment.NewLine, declarationLines)}";
                startLine += declarationLines.Count + 1;
                configLines.RemoveRange(0, declarationLine + 1);
            }

            // Get config
            string config = $"#line {startLine}{Environment.NewLine}{string.Join(Environment.NewLine, configLines)}";

            return new Tuple<string, string, string>(setup, declarations, config);
        }

        private void Setup(string code, bool updatePackages)
        {
            try
            {
                using (_engine.Trace.WithIndent().Verbose("Evaluating setup script"))
                {
                    // Create the setup script
                    StringBuilder codeBuilder = new StringBuilder(@"
                        using System;
                        using Wyam.Core;
                        using Wyam.Core.Configuration;
                        using Wyam.Core.NuGet;");
                    codeBuilder.AppendLine();
                    codeBuilder.AppendLine(string.Join(Environment.NewLine, 
                        typeof(IModule).Assembly.GetTypes()
                            .Where(x => !string.IsNullOrWhiteSpace(x.Namespace))
                            .Select(x => "using " + x.Namespace + ";")
                            .Distinct()));
                    codeBuilder.AppendLine(@"
                        public static class SetupScript
                        {
                            public static void Run(IPackagesCollection Packages, IAssemblyCollection Assemblies, string RootFolder, string InputFolder, string OutputFolder)
                            {");
                    codeBuilder.Append(code);
                    codeBuilder.AppendLine(@"
                            }
                        }");

                    // Assemblies
                    Assembly[] setupAssemblies = new[]
                    {
                        Assembly.GetAssembly(typeof (object)), // System
                        Assembly.GetAssembly(typeof (Wyam.Core.Engine)), //Wyam.Core
                        Assembly.GetAssembly(typeof (IModule)) // Wyam.Common
                    };

                    // Load the dynamic assembly and invoke
                    _rawSetupAssembly = CompileScript("WyamSetup", setupAssemblies, codeBuilder.ToString(), _engine.Trace);
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
                HashSet<Type> moduleTypes;
                using (_engine.Trace.WithIndent().Verbose("Initializing scripting environment"))
                {
                    // Initial default namespaces
                    _namespaces.AddRange(new []
                    {
                        "System",
                        "System.Collections.Generic",
                        "System.Linq",
                        "System.IO",
                        "System.Diagnostics",
                        "Wyam.Core",
                        "Wyam.Core.Configuration",
                        "Wyam.Core.Modules"
                    });

                    // Also include all Wyam.Common namespaces
                    _namespaces.AddRange(typeof (IModule).Assembly.GetTypes()
                        .Where(x => !string.IsNullOrWhiteSpace(x.Namespace))
                        .Select(x => x.Namespace)
                        .Distinct());

                    // Add specified assemblies from packages, etc.
                    GetAssemblies();

                    // Get modules
                    moduleTypes = GetModules();
                }

                using (_engine.Trace.WithIndent().Verbose("Evaluating configuration script"))
                {

                    // Generate the script
                    code = GenerateScript(declarations, code, moduleTypes);

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

        private byte[] CompileScript(string assemblyName, IEnumerable<Assembly> assemblies, string code, ITrace trace)
        {
            // Output if requested
            if (_outputScripts)
            {
                File.WriteAllText(Path.Combine(_engine.RootFolder, $"{(string.IsNullOrWhiteSpace(_fileName) ? string.Empty : _fileName + ".")}{assemblyName}.cs"), code);
            }

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
                    List<string> diagnosticMessages = result.Diagnostics
                        .Where(x => x.Severity == DiagnosticSeverity.Error)
                        .Select(GetCompilationErrorMessage)
                        .ToList();
                    trace.Error("{0} errors compiling configuration:{1}{2}", result.Diagnostics.Length, Environment.NewLine,
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

        private HashSet<Type> GetModules()
        {
            HashSet<Type> moduleTypes = new HashSet<Type>();
            foreach (Assembly assembly in _assemblies.Values)
            {
                using (_engine.Trace.WithIndent().Verbose("Searching for modules in assembly {0}", assembly.FullName))
                {
                    foreach (Type moduleType in GetLoadableTypes(assembly).Where(x => typeof(IModule).IsAssignableFrom(x)
                        && x.IsPublic && !x.IsAbstract && x.IsClass))
                    {
                        _engine.Trace.Verbose("Found module {0} in assembly {1}", moduleType.Name, assembly.FullName);
                        moduleTypes.Add(moduleType);
                        _namespaces.Add(moduleType.Namespace);
                    }
                }
            }
            return moduleTypes;
        }

        public IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            CheckDisposed();

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

        // This creates a wrapper class for the config script that contains static methods for constructing modules
        internal string GenerateScript(string declarations, string script, HashSet<Type> moduleTypes)
        {
            // Start the script, adding all requested namespaces
            StringBuilder scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine(string.Join(Environment.NewLine, _namespaces.Select(x => "using " + x + ";")));
            if (declarations != null)
            {
                scriptBuilder.AppendLine(declarations);
            }
            scriptBuilder.Append(@"
                public static class ConfigScript
                {
                    public static void Run(IDictionary<string, object> Metadata, IPipelineCollection Pipelines, string RootFolder, string InputFolder, string OutputFolder)
                    {" + Environment.NewLine + script + @"
                    }");

            // Add static methods to construct each module
            // Use Roslyn to get a display string for each constructor
            foreach (Type moduleType in moduleTypes)
            {
                scriptBuilder.Append(GenerateModuleConstructorMethods(moduleType));
            }

            scriptBuilder.Append("}");

            // Need to replace all instances of module type method name shortcuts to make them fully-qualified
            SyntaxTree scriptTree = CSharpSyntaxTree.ParseText(scriptBuilder.ToString());
            ConfigRewriter configRewriter = new ConfigRewriter(moduleTypes);
            script = configRewriter.Visit(scriptTree.GetRoot()).ToFullString();
            return script;
        }

        public string GenerateModuleConstructorMethods(Type moduleType)
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
    }
}
