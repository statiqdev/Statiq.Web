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
using Wyam.Common.Configuration;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.NuGet;
using Wyam.Common.Pipelines;
using Wyam.Common.Tracing;
using Wyam.Core.Util;
using Wyam.Core.NuGet;

namespace Wyam.Core.Configuration
{
    // This just encapsulates configuration logic
    internal class Config : IConfig, IDisposable
    {
        private readonly AssemblyCollection _assemblyCollection = new AssemblyCollection();
        private readonly Dictionary<string, Assembly> _assemblies = new Dictionary<string, Assembly>();
        private readonly HashSet<string> _namespaces = new HashSet<string>();

        private readonly IConfigurableFileSystem _fileSystem;
        private readonly IInitialMetadata _initialMetadata;
        private readonly IPipelineCollection _pipelines;
        private readonly PackagesCollection _packages;

        private bool _disposed;
        private Assembly _setupAssembly;
        private string _configAssemblyFullName;
        private byte[] _rawSetupAssembly;
        private string _fileName;
        private bool _outputScripts;
        
        public bool Configured { get; private set; }

        public Config(IConfigurableFileSystem fileSystem, IInitialMetadata initialMetadata, IPipelineCollection pipelines)
        {
            _fileSystem = fileSystem;
            _initialMetadata = initialMetadata;
            _pipelines = pipelines;
            _packages = new PackagesCollection(fileSystem);

            // This is the default set of assemblies that should get loaded during configuration and in other dynamic modules
            AddAssembly(Assembly.GetAssembly(typeof(object))); // System
            AddAssembly(Assembly.GetAssembly(typeof(System.Collections.Generic.List<>))); // System.Collections.Generic 
            AddAssembly(Assembly.GetAssembly(typeof(System.Linq.ImmutableArrayExtensions))); // System.Linq
            AddAssembly(Assembly.GetAssembly(typeof(System.Dynamic.DynamicObject))); // System.Core (needed for dynamic)
            AddAssembly(Assembly.GetAssembly(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo))); // Microsoft.CSharp (needed for dynamic)
            AddAssembly(Assembly.GetAssembly(typeof(System.IO.Stream))); // System.IO
            AddAssembly(Assembly.GetAssembly(typeof(System.Diagnostics.TraceSource))); // System.Diagnostics
            AddAssembly(Assembly.GetAssembly(typeof(Wyam.Core.Engine))); // Wyam.Core
            AddAssembly(Assembly.GetAssembly(typeof(IModule))); // Wyam.Common

            // Manually resolve included assemblies
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
            AppDomain.CurrentDomain.SetupInformation.PrivateBinPathProbe = string.Empty; // non-null means exclude application base path
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
                throw new ObjectDisposedException(nameof(Config));
            }
        }

        IAssemblyCollection IConfig.Assemblies => _assemblyCollection;

        IPackagesCollection IConfig.Packages => _packages;

        public IEnumerable<Assembly> Assemblies => _assemblies.Values;

        public IEnumerable<string> Namespaces => _namespaces;

        public byte[] RawConfigAssembly => _rawSetupAssembly;

        // Setup is separated from config by a line with only '-' characters
        // If no such line exists, then the entire script is treated as config
        public void Configure(string script, bool updatePackages, string fileName, bool outputScripts)
        {
            CheckDisposed();
            if (Configured)
            {
                throw new InvalidOperationException("This engine has already been configured.");
            }
            Configured = true;
            _fileName = fileName;
            _outputScripts = outputScripts;

            // If no script, nothing else to do
            if (string.IsNullOrWhiteSpace(script))
            {
                Configure(null, null);
                return;
            }

            Tuple<string, string, string> configParts = GetConfigParts(script);

            // Setup (install packages, specify additional assemblies, etc.)
            if (!string.IsNullOrWhiteSpace(configParts.Item1))
            {
                Setup(configParts.Item1, updatePackages);
            }

            // Configuration
            Configure(configParts.Item2, configParts.Item3);
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
                using (Trace.WithIndent().Verbose("Evaluating setup script"))
                {
                    // Create the setup script
                    StringBuilder codeBuilder = new StringBuilder(@"
                        using System;
                        using Wyam.Common.Configuration;
                        using Wyam.Common.IO;
                        using Wyam.Common.NuGet;");
                    codeBuilder.AppendLine();
                    codeBuilder.AppendLine(string.Join(Environment.NewLine,
                        typeof(IModule).Assembly.GetTypes()
                            .Where(x => !string.IsNullOrWhiteSpace(x.Namespace))
                            .Select(x => "using " + x.Namespace + ";")
                            .Distinct()));
                    codeBuilder.AppendLine(@"
                        public static class SetupScript
                        {
                            public static void Run(IPackagesCollection Packages, IAssemblyCollection Assemblies, IConfigurableFileSystem FileSystem)
                            {");
                    codeBuilder.Append(code);
                    codeBuilder.AppendLine(@"
                            }
                        }");

                    // Assemblies
                    Assembly[] setupAssemblies =
                    {
                        Assembly.GetAssembly(typeof (object)), // System
                        Assembly.GetAssembly(typeof (Engine)), //Wyam.Core
                        Assembly.GetAssembly(typeof (IModule)) // Wyam.Common
                    };

                    // Load the dynamic assembly and invoke
                    _rawSetupAssembly = CompileScript("WyamSetup", setupAssemblies, codeBuilder.ToString());
                    _setupAssembly = Assembly.Load(_rawSetupAssembly);
                    var configScriptType = _setupAssembly.GetExportedTypes().First(t => t.Name == "SetupScript");
                    MethodInfo runMethod = configScriptType.GetMethod("Run", BindingFlags.Public | BindingFlags.Static);
                    runMethod.Invoke(null, new object[] { _packages, _assemblyCollection, _fileSystem });
                }

                // Install packages
                using (Trace.WithIndent().Verbose("Installing packages"))
                {
                    _packages.InstallPackages(updatePackages);
                }
            }
            catch (Exception ex)
            {
                Trace.Error("Unexpected error during setup: {0}", ex.Message);
                throw;
            }
        }

        private static readonly string[] DefaultNamespaces =
        {
            "System",
            "System.Collections.Generic",
            "System.Linq",
            "System.IO",
            "System.Diagnostics",
            "Wyam.Core",
            "Wyam.Core.Configuration"
        };

        private void Configure(string declarations, string code)
        {
            try
            {
                HashSet<Type> moduleTypes = new HashSet<Type>();
                using (Trace.WithIndent().Verbose("Initializing scripting environment"))
                {
                    // Initial default namespaces
                    _namespaces.AddRange(DefaultNamespaces);

                    // Add all module namespaces from Wyam.Core
                    _namespaces.AddRange(typeof(Engine).Assembly.GetTypes()
                        .Where(x => typeof(IModule).IsAssignableFrom(x))
                        .Select(x => x.Namespace));

                    // Also include all Wyam.Common namespaces
                    _namespaces.AddRange(typeof(IModule).Assembly.GetTypes()
                        .Where(x => !string.IsNullOrWhiteSpace(x.Namespace))
                        .Select(x => x.Namespace));

                    // Add specified assemblies from packages, etc.
                    GetAssemblies();

                    // Scan assemblies
                    ScanAssemblies(moduleTypes);
                }
                
                using (Trace.WithIndent().Verbose("Evaluating configuration script"))
                {
                    // Generate the script
                    code = GenerateScript(declarations, code, moduleTypes);

                    // Load the dynamic assembly and invoke
                    _rawSetupAssembly = CompileScript("WyamConfig", _assemblies.Values, code);
                    _setupAssembly = Assembly.Load(_rawSetupAssembly);
                    _configAssemblyFullName = _setupAssembly.FullName;
                    var configScriptType = _setupAssembly.GetExportedTypes().First(t => t.Name == "ConfigScript");
                    MethodInfo runMethod = configScriptType.GetMethod("Run", BindingFlags.Public | BindingFlags.Static);
                    runMethod.Invoke(null, new object[] { _initialMetadata, _pipelines, _fileSystem });
                }
            }
            catch (Exception ex)
            {
                Trace.Error("Unexpected error during configuration evaluation: {0}", ex.Message);
                throw;
            }
        }

        private byte[] CompileScript(string assemblyName, IEnumerable<Assembly> assemblies, string code)
        {
            // Output if requested
            if (_outputScripts)
            {
                File.WriteAllText(System.IO.Path.Combine(_fileSystem.RootPath.FullPath, $"{(string.IsNullOrWhiteSpace(_fileName) ? string.Empty : _fileName + ".")}{assemblyName}.cs"), code);
            }

            // Create the compilation
            var parseOptions = new CSharpParseOptions();
            var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(code, Encoding.UTF8), parseOptions, assemblyName);
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            var assemblyPath = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create(assemblyName, new[] { syntaxTree },
                assemblies.Select(x => MetadataReference.CreateFromFile(x.Location)), compilationOptions)
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

        // Adds all specified assemblies and those in packages path, finds all modules, and adds their namespaces and all assembly references to the options
        private void GetAssemblies()
        {
            // Get path to all assemblies (except those specified by name)
            List<string> assemblyPaths = new List<string>();
            assemblyPaths.AddRange(_packages.GetCompatibleAssemblyPaths());
            assemblyPaths.AddRange(Directory.GetFiles(System.IO.Path.GetDirectoryName(typeof(Config).Assembly.Location), "*.dll", SearchOption.AllDirectories));
            assemblyPaths.AddRange(_assemblyCollection.Directories
                .Select(x => new Tuple<string, SearchOption>(System.IO.Path.Combine(_fileSystem.RootPath.FullPath, x.Item1), x.Item2))
                .Where(x => Directory.Exists(x.Item1))
                .SelectMany(x => Directory.GetFiles(x.Item1, "*.dll", x.Item2)));
            assemblyPaths.AddRange(_assemblyCollection.ByFile
                .Select(x => new Tuple<string, string>(x, System.IO.Path.Combine(_fileSystem.RootPath.FullPath, x)))
                .Select(x => File.Exists(x.Item2) ? x.Item2 : x.Item1));

            // Add all paths to the PrivateBinPath search location (to ensure they load in the default context)
            AppDomain.CurrentDomain.SetupInformation.PrivateBinPath =
                string.Join(";", new[] { AppDomain.CurrentDomain.SetupInformation.PrivateBinPath }
                    .Concat(assemblyPaths.Select(x => System.IO.Path.GetDirectoryName(x).Distinct())));

            // Iterate assemblies by path (making sure to add them to the current path if relative), add them to the script, and check for modules
            // If this approach causes problems, could also try loading assemblies in custom app domain:
            // http://stackoverflow.com/questions/6626647/custom-appdomain-and-privatebinpath
            foreach (string assemblyPath in assemblyPaths.Distinct())
            {
                try
                {
                    Trace.Verbose("Loading assembly file {0}", assemblyPath);
                    AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyPath);
                    Assembly assembly = Assembly.Load(assemblyName);
                    if (!AddAssembly(assembly))
                    {
                        Trace.Verbose("Skipping assembly file {0} because it was already added", assemblyPath);
                    }
                    else
                    {
                        LoadReferencedAssemblies(assembly.GetReferencedAssemblies());
                    }
                }
                catch (Exception ex)
                {
                    Trace.Verbose("{0} exception while loading assembly file {1}: {2}", ex.GetType().Name, assemblyPath, ex.Message);
                }
            }

            // Also iterate assemblies specified by name
            foreach (string assemblyName in _assemblyCollection.ByName)
            {
                try
                {
                    Trace.Verbose("Loading assembly {0}", assemblyName);
                    Assembly assembly = Assembly.Load(assemblyName);
                    if (!AddAssembly(assembly))
                    {
                        Trace.Verbose("Skipping assembly {0} because it was already added", assemblyName);
                    }
                    else
                    {
                        LoadReferencedAssemblies(assembly.GetReferencedAssemblies());
                    }
                }
                catch (Exception ex)
                {
                    Trace.Verbose("{0} exception while loading assembly {1}: {2}", ex.GetType().Name, assemblyName, ex.Message);
                }
            }
        }

        private void LoadReferencedAssemblies(IEnumerable<AssemblyName> assemblyNames)
        {
            foreach (AssemblyName assemblyName in assemblyNames.Where(x => !_assemblies.ContainsKey(x.FullName)))
            {
                try
                {
                    Trace.Verbose("Loading referenced assembly {0}", assemblyName);
                    Assembly assembly = Assembly.Load(assemblyName);
                    AddAssembly(assembly);
                    LoadReferencedAssemblies(assembly.GetReferencedAssemblies());
                }
                catch (Exception ex)
                {
                    Trace.Verbose("{0} exception while loading referenced assembly {1}: {2}", ex.GetType().Name, assemblyName, ex.Message);
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

        private void ScanAssemblies(HashSet<Type> moduleTypes)
        {
            foreach (Assembly assembly in _assemblies.Values)
            {
                using (Trace.WithIndent().Verbose("Scanning assembly {0}", assembly.FullName))
                {
                    Type[] loadableTypes = GetLoadableTypes(assembly);
                    GetModuleTypes(assembly, loadableTypes, moduleTypes);
                }
            }
        }

        private Type[] GetLoadableTypes(Assembly assembly)
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
                    Trace.Verbose("Loader Exception: {0}", loaderException.Message);
                }
                return ex.Types.Where(t => t != null).ToArray();
            }
        }

        private void GetModuleTypes(Assembly assembly, IEnumerable<Type> loadableTypes, HashSet<Type> types)
        {
            foreach (Type type in loadableTypes.Where(x => typeof(IModule).IsAssignableFrom(x) 
                && x.IsPublic && !x.IsAbstract && x.IsClass))
            {
                Trace.Verbose("Found module {0} in assembly {1}", type.Name, assembly.FullName);
                types.Add(type);
                _namespaces.Add(type.Namespace);
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
                    public static void Run(IInitialMetadata InitialMetadata, IPipelineCollection Pipelines, IFileSystem FileSystem)
                    {" + Environment.NewLine + script + @"
                    }");
            
            // Add static methods to construct each module
            Dictionary<string, string> moduleNames = new Dictionary<string, string>();
            foreach (Type moduleType in moduleTypes)
            {
                scriptBuilder.Append(GenerateModuleConstructorMethods(moduleType, moduleNames));
            }

            scriptBuilder.Append("}");

            // Need to replace all instances of module type method name shortcuts to make them fully-qualified
            SyntaxTree scriptTree = CSharpSyntaxTree.ParseText(scriptBuilder.ToString());
            ConfigRewriter configRewriter = new ConfigRewriter(moduleTypes);
            script = configRewriter.Visit(scriptTree.GetRoot()).ToFullString();
            return script;
        }

        private string GenerateModuleConstructorMethods(Type moduleType, Dictionary<string, string> memberNames)
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
    }
}
