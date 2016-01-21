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
        private ConfigScript _configScript;
        private string _configAssemblyFullName;
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
        
        public byte[] RawConfigAssembly => _configScript?.RawAssembly;

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
                Configure(new ConfigParts());
                return;
            }

            ConfigParts configParts = ConfigSplitter.Split(script);

            // Setup (install packages, specify additional assemblies, etc.)
            if (configParts.HasSetup)
            {
                Setup(configParts.Setup, updatePackages);
            }

            // Configuration
            Configure(configParts);
        }

        private void Setup(string code, bool updatePackages)
        {
            try
            {
                // Compile and evaluate the script
                using (Trace.WithIndent().Verbose("Evaluating setup script"))
                {
                    SetupScript setupScript = new SetupScript(code);
                    OutputScript(SetupScript.AssemblyName, setupScript.Code);
                    setupScript.Compile();
                    setupScript.Invoke(_packages, _assemblyCollection, _fileSystem);
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

        private void Configure(ConfigParts configParts)
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
                    string script = GenerateScript(configParts, moduleTypes);

                    // Load the dynamic assembly and invoke
                    _rawConfigAssembly = CompileScript("WyamConfig", _assemblies.Values, script);
                    _configAssembly = Assembly.Load(_rawConfigAssembly);
                    _configAssemblyFullName = _configAssembly.FullName;
                    var configScriptType = _configAssembly.GetExportedTypes().First(t => t.Name == "ConfigScript");
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

        private void OutputScript(string assemblyName, string code)
        {
            // Output only if requested
            if (_outputScripts)
            {
                File.WriteAllText(System.IO.Path.Combine(_fileSystem.RootPath.FullPath, $"{(string.IsNullOrWhiteSpace(_fileName) ? string.Empty : _fileName + ".")}{assemblyName}.cs"), code);
            }
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
            if (_configAssembly != null)
            {
                if (args.Name == _configAssemblyFullName)
                {
                    return _configAssembly;
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
        internal string GenerateScript(ConfigParts configParts, HashSet<Type> moduleTypes)
        {
            // Start the script, adding all requested namespaces
            StringBuilder scriptBuilder = new StringBuilder();
            scriptBuilder.AppendLine(string.Join(Environment.NewLine, _namespaces.Select(x => "using " + x + ";")));
            if (configParts.HasDeclarations)
            {
                scriptBuilder.AppendLine(configParts.Declarations);
            }
            scriptBuilder.Append(@"
                public static class ConfigScript
                {
                    public static void Run(IInitialMetadata InitialMetadata, IPipelineCollection Pipelines, IFileSystem FileSystem)
                    {" + Environment.NewLine + configParts.Config + @"
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
            return configRewriter.Visit(scriptTree.GetRoot()).ToFullString();
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
